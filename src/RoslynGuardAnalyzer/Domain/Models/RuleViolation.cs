#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using RoslynGuardAnalyzer.Core;
using PathNormalizer = RoslynGuardAnalyzer.Domain.Models.PathNormalizer;

namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Represents a violation of an architectural rule found during code analysis.
/// Contains location information and details about the violation.
/// </summary>
public sealed class RuleViolation
{
    public string Id { get; set; }
    public string RuleId { get; set; }
    public string RuleName { get; set; }
    public string Message { get; set; }
    public SeverityLevel Severity { get; set; }

    private string _filePath = string.Empty;
    /// <summary>
    /// File path of the source file where the violation was detected.
    /// The path is always stored as a repository‑relative path.
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set => _filePath = NormalizePath(value);
    }

    public int LineNumber { get; set; }
    public int ColumnNumber { get; set; }
    public string? CodeSnippet { get; set; }
    public string? SuggestedFix { get; set; }
    public DateTime DetectedAt { get; set; }
    public string? ProjectName { get; set; }
    public RuleCategory Category { get; set; }
    public Dictionary<string, string> Metadata { get; set; }

    public RuleViolation()
    {
        Id = Guid.NewGuid().ToString();
        RuleId = string.Empty;
        RuleName = string.Empty;
        Message = string.Empty;
        FilePath = string.Empty;
        Severity = SeverityLevel.Warning;
        LineNumber = 0;
        ColumnNumber = 0;
        DetectedAt = DateTime.UtcNow;
        Category = RuleCategory.CodeStructure;
        Metadata = new Dictionary<string, string>();
    }

    public RuleViolation(string ruleId, string ruleName, string message, string filePath)
        : this()
    {
        ArgumentException.ThrowIfNullOrEmpty(ruleId);
        ArgumentException.ThrowIfNullOrEmpty(ruleName);
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        RuleId = ruleId;
        RuleName = ruleName;
        Message = message;
        FilePath = filePath; // setter normalizes the path
    }

    /// <summary>
    /// Normalizes a file path to be repository‑relative.
    /// If the supplied path is absolute, it is converted to a path relative to the current working directory
    /// (which, in CI, is the repository root). Relative paths are left unchanged.
    /// </summary>
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        // If the path is already relative, return it as‑is.
        if (!Path.IsPathRooted(path))
            return path;

        try
        {
            // Convert to a path relative to the current directory (repo root).
            var repoRoot = Directory.GetCurrentDirectory();
            var relative = Path.GetRelativePath(repoRoot, path);
            return relative;
        }
        catch
        {
            // If any exception occurs (e.g., invalid characters), fall back to the original path.
            return path;
        }
    }

    /// <summary>
    /// Gets a human‑readable location string for the violation.
    /// </summary>
    /// <returns>Formatted location string (e.g., "file.cs(42, 15)").</returns>
    public string GetFormattedLocation()
    {
        var fileName = PathNormalizer.GetFileName(FilePath);
        return $"{fileName}({LineNumber}, {ColumnNumber})";
    }

    /// <summary>
    /// Gets a complete description of the violation with location and message.
    /// </summary>
    /// <returns>Formatted violation description.</returns>
    public string GetFullDescription()
    {
        return $"[{RuleId}] {Severity}: {Message} at {GetFormattedLocation()}";
    }

    /// <summary>
    /// Adds or updates metadata associated with this violation.
    /// </summary>
    public void AddMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty.", nameof(key));
        ArgumentException.ThrowIfNullOrEmpty(value);

        Metadata[key] = value ?? string.Empty;
    }

    /// <summary>
    /// Gets metadata value with optional default fallback.
    /// </summary>
    public string? GetMetadata(string key, string? defaultValue = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return Metadata.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Determines if this violation is critical and must be fixed.
    /// </summary>
    /// <returns>True if severity is Critical or Error.</returns>
    public bool IsCritical()
    {
        return Severity == SeverityLevel.Critical || Severity == SeverityLevel.Error;
    }

    /// <summary>
    /// Creates a copy of this violation with updated severity.
    /// </summary>
    public RuleViolation WithSeverity(SeverityLevel newSeverity)
    {
        var copy = new RuleViolation
        {
            Id = Guid.NewGuid().ToString(),
            RuleId = RuleId,
            RuleName = RuleName,
            Message = Message,
            FilePath = FilePath, // already normalized
            LineNumber = LineNumber,
            ColumnNumber = ColumnNumber,
            CodeSnippet = CodeSnippet,
            SuggestedFix = SuggestedFix,
            ProjectName = ProjectName,
            Category = Category,
            Severity = newSeverity,
            DetectedAt = DateTime.UtcNow
        };

        foreach (var kvp in Metadata)
        {
            copy.Metadata[kvp.Key] = kvp.Value;
        }

        return copy;
    }

    /// <summary>
    /// Validates the violation has required information.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(RuleId)
            && !string.IsNullOrWhiteSpace(RuleName)
            && !string.IsNullOrWhiteSpace(Message)
            && !string.IsNullOrWhiteSpace(FilePath)
            && LineNumber > 0
            && ColumnNumber >= 0;
    }
}
