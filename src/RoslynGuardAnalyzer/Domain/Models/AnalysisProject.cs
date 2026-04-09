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

namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Represents a project being analyzed, including its metadata and file information.
/// </summary>
public sealed class AnalysisProject
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public string? TargetFramework { get; set; }
    public List<string> SourceFiles { get; set; }
    public List<string> ReferencedProjects { get; set; }
    public Dictionary<string, string> Properties { get; set; }
    public bool IsNetCore { get; set; }
    public string? Language { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public int FileCount { get; set; }

    public AnalysisProject()
    {
        Id = Guid.NewGuid().ToString();
        Name = string.Empty;
        Path = string.Empty;
        SourceFiles = new List<string>();
        ReferencedProjects = new List<string>();
        Properties = new Dictionary<string, string>();
        Language = "C#";
        AnalyzedAt = DateTime.UtcNow;
    }

    public AnalysisProject(string name, string path)
        : this()
    {
        Name = name;
        Path = path;
    }

    /// <summary>
    /// Adds a source file to the project's file list.
    /// </summary>
    public void AddSourceFile(string filePath)
    {
        if (!string.IsNullOrWhiteSpace(filePath) && !SourceFiles.Contains(filePath))
        {
            SourceFiles.Add(filePath);
            FileCount = SourceFiles.Count;
        }
    }

    /// <summary>
    /// Adds a referenced project.
    /// </summary>
    public void AddReferencedProject(string projectPath)
    {
        if (!string.IsNullOrWhiteSpace(projectPath) && !ReferencedProjects.Contains(projectPath))
        {
            ReferencedProjects.Add(projectPath);
        }
    }

    /// <summary>
    /// Gets all C# source files in the project.
    /// </summary>
    public IEnumerable<string> GetCSharpFiles()
    {
        return SourceFiles.Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the project's property value.
    /// </summary>
    public string? GetProperty(string key, string? defaultValue = null)
    {
        return Properties.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Sets a project property.
    /// </summary>
    public void SetProperty(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Property key cannot be null or empty.", nameof(key));

        Properties[key] = value ?? string.Empty;
    }

    /// <summary>
    /// Determines if this is a .NET Core/.NET 5+ project.
    /// </summary>
    /// <returns>True if the target framework starts with "net" (e.g., net6.0, net10.0).</returns>
    public bool IsModernDotNet()
    {
        if (string.IsNullOrWhiteSpace(TargetFramework))
            return false;

        return TargetFramework.StartsWith("net", StringComparison.OrdinalIgnoreCase)
            && !TargetFramework.StartsWith("netframework", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets statistics about the project's code.
    /// </summary>
    public ProjectStatistics GetStatistics()
    {
        var stats = new ProjectStatistics
        {
            TotalFiles = SourceFiles.Count,
            CSharpFiles = GetCSharpFiles().Count(),
            ReferencedProjectCount = ReferencedProjects.Count
        };

        return stats;
    }

    /// <summary>
    /// Validates the project has required information.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(Path)
            && Directory.Exists(Path);
    }

    /// <summary>
    /// Gets the project directory path.
    /// </summary>
    public string GetDirectoryPath()
    {
        return Directory.Exists(Path) ? Path : Path.GetDirectoryName(Path) ?? string.Empty;
    }
}

/// <summary>
/// Contains statistical information about a project.
/// </summary>
public sealed class ProjectStatistics
{
    public int TotalFiles { get; set; }
    public int CSharpFiles { get; set; }
    public int ReferencedProjectCount { get; set; }
    public int TotalLineCount { get; set; }
    public int DocumentedElementCount { get; set; }
    public double AverageCyclomaticComplexity { get; set; }
}
