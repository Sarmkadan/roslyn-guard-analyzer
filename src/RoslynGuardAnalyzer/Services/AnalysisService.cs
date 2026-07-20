#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;

using Microsoft.Extensions.Logging;

using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Exceptions;

namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Orchestrates the complete analysis workflow for projects and files.
/// </summary>
public sealed class AnalysisService : IAnalysisService
{
    private readonly IRuleEngine _ruleEngine;
    private readonly IValidationService _validationService;

    public AnalysisService(IRuleEngine ruleEngine, IValidationService validationService)
    {
        _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }

    /// <summary>
    /// Analyzes a project asynchronously and returns the analysis result.
    /// </summary>
    public async Task<AnalysisResult> AnalyzeProjectAsync(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
            throw new ArgumentException("Project path cannot be null or empty.", nameof(projectPath));

        var validation = _validationService.ValidateProjectPath(projectPath);
        if (!validation.IsValid)
            throw new ConfigurationException(validation.Error ?? "Invalid project path");

        var result = new AnalysisResult(
            Path.GetFileNameWithoutExtension(projectPath),
            projectPath);

        try
        {
            var project = LoadProjectMetadata(projectPath);
            result.TotalFilesAnalyzed = project.SourceFiles.Count;

            var codeElements = await ExtractCodeElementsAsync(project);
            result.TotalElementsAnalyzed = codeElements.Count;

            foreach (var element in codeElements)
            {
                result.AddAnalyzedElement(element);
            }

            var violations = await _ruleEngine.ExecuteAllRulesAsync(codeElements);
            result.AddViolations(violations);

            result.MarkAsCompleted();

            Console.WriteLine(AnalyzerConstants.Messages.AnalysisCompleted);
        }
        catch (Exception ex)
        {
            result.MarkAsFailed($"Analysis failed: {ex.Message}");
            throw new AnalysisException($"Failed to analyze project at {projectPath}", ex);
        }

        return result;
    }

    /// <summary>
    /// Analyzes a single file asynchronously.
    /// </summary>
    public async Task<AnalysisResult> AnalyzeFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileAccessException(filePath, $"File not found: {filePath}");

        if (!filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            throw new FileAccessException(filePath, "Only C# files (.cs) are supported");

        var result = new AnalysisResult(
            Path.GetFileNameWithoutExtension(filePath),
            filePath);

        try
        {
            var codeElements = await ExtractCodeElementsFromFileAsync(filePath);
            result.TotalElementsAnalyzed = codeElements.Count;
            result.TotalFilesAnalyzed = 1;

            foreach (var element in codeElements)
            {
                result.AddAnalyzedElement(element);
            }

            var violations = await _ruleEngine.ExecuteAllRulesAsync(codeElements);
            result.AddViolations(violations);

            result.MarkAsCompleted();
        }
        catch (Exception ex)
        {
            result.MarkAsFailed($"File analysis failed: {ex.Message}");
            throw new AnalysisException($"Failed to analyze file {filePath}", ex);
        }

        return result;
    }

    /// <summary>
    /// Loads project metadata from the project file.
    /// </summary>
    private AnalysisProject LoadProjectMetadata(string projectPath)
    {
        var project = new AnalysisProject(
            Path.GetFileNameWithoutExtension(projectPath),
            projectPath);

        var projectDir = Path.GetDirectoryName(projectPath) ?? Directory.GetCurrentDirectory();

        var csFiles = Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories);
        foreach (var file in csFiles)
        {
            // Skip common exclusion patterns
            if (file.Contains("bin") || file.Contains("obj") || file.Contains(".git"))
                continue;

            project.AddSourceFile(file);
        }

        // Set .NET 10 as default
        project.TargetFramework = "net10.0";
        project.SetProperty("TargetFramework", "net10.0");

        return project;
    }

    /// <summary>
    /// Extracts code elements from all files in a project.
    /// </summary>
    private async Task<List<CodeElement>> ExtractCodeElementsAsync(AnalysisProject project)
    {
        var elements = new List<CodeElement>();

        foreach (var file in project.GetCSharpFiles())
        {
            try
            {
                var fileElements = await ExtractCodeElementsFromFileAsync(file);
                elements.AddRange(fileElements);
            }
            catch (ParseException ex)
            {
                Console.WriteLine($"Warning: Could not parse {file}: {ex.Message}");
            }
        }

        // Merge partial classes across multiple files
        var mergedElements = new List<CodeElement>();
        var groupedClasses = elements
            .Where(e => e.ElementType == CodeElementType.Class)
            .GroupBy(e => new { e.Namespace, e.Name })
            .ToList();

        foreach (var group in groupedClasses)
        {
            var first = group.First();
            if (group.Count() > 1)
            {
                // Merge attributes and dependencies
                foreach (var other in group.Skip(1))
                {
                    first.Attributes.AddRange(other.Attributes);
                    first.Dependencies.AddRange(other.Dependencies);
                    first.Attributes = first.Attributes.Distinct().ToList();
                    first.Dependencies = first.Dependencies.Distinct().ToList();
                }
            }
            mergedElements.Add(first);
        }

        // Add non-class elements
        mergedElements.AddRange(elements.Where(e => e.ElementType != CodeElementType.Class));

        return mergedElements;
    }

    /// <summary>
    /// Extracts code elements from a single file.
    /// </summary>
    private async Task<List<CodeElement>> ExtractCodeElementsFromFileAsync(string filePath)
    {
        var elements = new List<CodeElement>();

        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(content))
                return elements;

            // Simple parsing: extract class, interface, method declarations
            var lines = content.Split('\n');
            var currentNamespace = string.Empty;
            var currentClass = string.Empty;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (line.StartsWith("namespace "))
                {
                    currentNamespace = ExtractIdentifier(line, "namespace ");
                }

                if (line.Contains("class "))
                {
                    var className = ExtractIdentifier(line, "class ");
                    currentClass = className;

                    var classElement = new CodeElement(className, CodeElementType.Class, filePath)
                    {
                        Namespace = currentNamespace,
                        StartLineNumber = i + 1,
                        IsPublic = line.Contains("public"),
                        IsAbstract = line.Contains("abstract"),
                        IsStatic = line.Contains("static")
                    };

                    elements.Add(classElement);
                }

                if (line.Contains("interface "))
                {
                    var interfaceName = ExtractIdentifier(line, "interface ");

                    var interfaceElement = new CodeElement(interfaceName, CodeElementType.Interface, filePath)
                    {
                        Namespace = currentNamespace,
                        StartLineNumber = i + 1,
                        IsPublic = line.Contains("public")
                    };

                    elements.Add(interfaceElement);
                }

                if (line.Contains("public ") && line.Contains("(") && !line.Contains("class ") && !line.Contains("interface "))
                {
                    var methodName = ExtractMethodName(line);
                    if (!string.IsNullOrEmpty(methodName))
                    {
                        var methodElement = new CodeElement(methodName, CodeElementType.Method, filePath)
                        {
                            Namespace = currentNamespace,
                            ParentName = currentClass,
                            StartLineNumber = i + 1,
                            IsPublic = true,
                            IsAsync = line.Contains("async"),
                            ReturnType = ExtractReturnType(line)
                        };

                        elements.Add(methodElement);
                    }

        // Detect catch blocks
        if (line.Contains("catch") && line.Contains("("))
        {
            // Extract catch block identifier (exception variable name)
            var catchKeywordIndex = line.IndexOf("catch", StringComparison.OrdinalIgnoreCase);
            var openParenIndex = line.IndexOf('(', catchKeywordIndex);
            var closeParenIndex = line.IndexOf(')', openParenIndex);

            if (openParenIndex > 0 && closeParenIndex > openParenIndex)
            {
                var exceptionTypePart = line.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1).Trim();
                var exceptionVarName = string.Empty;

                // Extract exception variable name if present (e.g., "catch (Exception ex)")
                if (!string.IsNullOrEmpty(exceptionTypePart))
                {
                    var spaceIndex = exceptionTypePart.LastIndexOf(' ');
                    if (spaceIndex > 0 && spaceIndex < exceptionTypePart.Length - 1)
                    {
                        exceptionVarName = exceptionTypePart.Substring(spaceIndex + 1).Trim();
                    }
                }

                var catchBlockName = string.IsNullOrEmpty(exceptionVarName)
                    ? "catch"
                    : $"catch ({exceptionVarName})" ;

                var catchElement = new CodeElement(catchBlockName, CodeElementType.CatchBlock, filePath)
                {
                    Namespace = currentNamespace,
                    ParentName = currentClass,
                    StartLineNumber = i + 1,
                    IsPublic = true
                };

                elements.Add(catchElement);
            }
        }
                }
            }
        }
        catch (Exception ex)
        {
            throw new ParseException(filePath, $"Error parsing file: {ex.Message}", ex);
        }

        return elements;
    }

    /// <summary>
    /// Extracts an identifier from a declaration line.
    /// </summary>
    private string ExtractIdentifier(string line, string keyword)
    {
        var index = line.IndexOf(keyword) + keyword.Length;
        var endIndex = line.IndexOfAny(new[] { ' ', ':', '{', '(' }, index);
        if (endIndex == -1) endIndex = line.Length;

        return line.Substring(index, endIndex - index).Trim();
    }

    /// <summary>
    /// Extracts method name from a method declaration.
    /// </summary>
    private string ExtractMethodName(string line)
    {
        var startIndex = line.LastIndexOf(' ') + 1;
        var endIndex = line.IndexOf('(', startIndex);

        if (endIndex <= startIndex) return string.Empty;

        return line.Substring(startIndex, endIndex - startIndex).Trim();
    }

    /// <summary>
    /// Extracts return type from a method declaration.
    /// </summary>
    private string ExtractReturnType(string line)
    {
        var words = line.Split(' ');
        // Return type is typically before the method name, after access modifier
        for (int i = 0; i < words.Length - 1; i++)
        {
            if ((words[i] == "public" || words[i] == "private" || words[i] == "protected")
                && words[i + 1] != "class" && words[i + 1] != "interface")
            {
                if (i + 2 < words.Length && words[i + 1] != "async")
                    return words[i + 1];

                if (i + 3 < words.Length && words[i + 1] == "async")
                    return words[i + 2];
            }
        }

        return string.Empty;
    }
}
