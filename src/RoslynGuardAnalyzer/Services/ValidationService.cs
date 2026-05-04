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

using RoslynGuardAnalyzer.Domain.Models;
namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Validates configurations, rules, and project paths.
/// </summary>
public sealed class ValidationService : IValidationService
{
    /// <summary>
    /// Validates a rule configuration for consistency and completeness.
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateRuleConfiguration(RuleConfiguration config)
    {
        var errors = new List<string>();

        if (config == null)
        {
            errors.Add("Configuration cannot be null");
            return (false, errors);
        }

        if (string.IsNullOrWhiteSpace(config.Name))
            errors.Add("Configuration name is required");

        if (config.Name?.Length < 3)
            errors.Add("Configuration name must be at least 3 characters");

        if (!config.EnabledRules.Any())
            errors.Add("At least one rule must be enabled");

        if (config.MaxViolationsToReport <= 0)
            errors.Add("MaxViolationsToReport must be greater than 0");

        if (config.AnalysisTimeoutSeconds <= 0)
            errors.Add("AnalysisTimeoutSeconds must be greater than 0");

        if (config.AnalysisTimeoutSeconds > 3600)
            errors.Add("AnalysisTimeoutSeconds should not exceed 3600 seconds (1 hour)");

        // Validate all enabled rules
        foreach (var rule in config.EnabledRules)
        {
            var (ruleValid, ruleErrors) = ValidateRule(rule);
            if (!ruleValid)
                errors.AddRange(ruleErrors);
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Validates an analysis rule.
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateRule(AnalysisRule rule)
    {
        var errors = new List<string>();

        if (rule == null)
        {
            errors.Add("Rule cannot be null");
            return (false, errors);
        }

        if (string.IsNullOrWhiteSpace(rule.Id))
            errors.Add("Rule ID is required");

        if (rule.Id?.Length < 3 || rule.Id?.Length > 10)
            errors.Add("Rule ID must be between 3 and 10 characters");

        if (string.IsNullOrWhiteSpace(rule.Name))
            errors.Add("Rule name is required");

        if (rule.Name?.Length < 5 || rule.Name?.Length > 100)
            errors.Add("Rule name must be between 5 and 100 characters");

        if (string.IsNullOrWhiteSpace(rule.Description))
            errors.Add("Rule description is required");

        if (rule.Description?.Length < 10 || rule.Description?.Length > 500)
            errors.Add("Rule description must be between 10 and 500 characters");

        // Validate rule pattern if provided
        if (!string.IsNullOrWhiteSpace(rule.RulePattern))
        {
            if (!IsValidRegexPattern(rule.RulePattern))
                errors.Add($"Rule pattern is not a valid regex: {rule.RulePattern}");
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Validates that a project path exists and is accessible.
    /// </summary>
    public (bool IsValid, string? Error) ValidateProjectPath(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
            return (false, "Project path cannot be null or empty");

        var expandedPath = Environment.ExpandEnvironmentVariables(projectPath);

        if (!Directory.Exists(expandedPath) && !File.Exists(expandedPath))
            return (false, $"Path does not exist: {projectPath}");

        if (File.Exists(expandedPath))
        {
            // If it's a file, it should be a .csproj or .cs file
            var extension = Path.GetExtension(expandedPath);
            if (extension != ".csproj" && extension != ".cs" && extension != ".sln")
                return (false, "File must be a .csproj, .cs, or .sln file");
        }

        // Check if we have read access
        try
        {
            _ = Directory.GetAccessControl(expandedPath);
        }
        catch (UnauthorizedAccessException)
        {
            return (false, $"No read access to path: {projectPath}");
        }
        catch (System.IO.FileNotFoundException)
        {
            return (false, $"Path not found: {projectPath}");
        }

        return (true, null);
    }

    /// <summary>
    /// Validates a code element.
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateCodeElement(CodeElement element)
    {
        var errors = new List<string>();

        if (element == null)
        {
            errors.Add("Code element cannot be null");
            return (false, errors);
        }

        if (string.IsNullOrWhiteSpace(element.Name))
            errors.Add("Element name is required");

        if (string.IsNullOrWhiteSpace(element.FilePath))
            errors.Add("Element file path is required");

        if (element.StartLineNumber <= 0)
            errors.Add("Start line number must be positive");

        if (element.EndLineNumber < element.StartLineNumber)
            errors.Add("End line number must be greater than or equal to start line number");

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Checks if a string is a valid regex pattern.
    /// </summary>
    private bool IsValidRegexPattern(string pattern)
    {
        try
        {
            _ = new System.Text.RegularExpressions.Regex(pattern);
            return true;
        }
        catch (System.Text.RegularExpressions.RegexParseException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates analysis results.
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateAnalysisResult(AnalysisResult result)
    {
        var errors = new List<string>();

        if (result == null)
        {
            errors.Add("Analysis result cannot be null");
            return (false, errors);
        }

        if (string.IsNullOrWhiteSpace(result.ProjectName))
            errors.Add("Project name is required");

        if (string.IsNullOrWhiteSpace(result.ProjectPath))
            errors.Add("Project path is required");

        if (result.AnalysisEndTime < result.AnalysisStartTime)
            errors.Add("Analysis end time must be after start time");

        if (result.TotalFilesAnalyzed < 0)
            errors.Add("Total files analyzed cannot be negative");

        if (result.TotalElementsAnalyzed < 0)
            errors.Add("Total elements analyzed cannot be negative");

        return (errors.Count == 0, errors);
    }
}

/// <summary>
/// Extension methods for validation.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Determines if a string is a valid C# identifier.
    /// </summary>
    public static bool IsValidIdentifier(this string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return false;

        if (!char.IsLetter(identifier[0]) && identifier[0] != '_')
            return false;

        return identifier.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    /// <summary>
    /// Determines if a string follows PascalCase convention.
    /// </summary>
    public static bool IsPascalCase(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return char.IsUpper(text[0]) && !text.Contains('_');
    }

    /// <summary>
    /// Determines if a string follows camelCase convention.
    /// </summary>
    public static bool IsCamelCase(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return char.IsLower(text[0]) && !text.Contains('_');
    }

    /// <summary>
    /// Determines if a string follows UPPER_CASE convention.
    /// </summary>
    public static bool IsUpperSnakeCase(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return text == text.ToUpper() && text.All(c => char.IsLetterOrDigit(c) || c == '_');
    }
}
