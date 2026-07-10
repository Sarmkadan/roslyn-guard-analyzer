#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoslynGuardAnalyzer.Core;

namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Extension methods for <see cref="RuleViolation"/> to provide additional functionality
/// for working with rule violations in various scenarios.
/// </summary>
public static class RuleViolationExtensions
{
    /// <summary>
    /// Creates a new violation with updated message while preserving all other properties.
    /// </summary>
    /// <param name="violation">The original violation to copy from.</param>
    /// <param name="newMessage">The new message to set.</param>
    /// <returns>A new RuleViolation instance with the updated message.</returns>
    public static RuleViolation WithMessage(this RuleViolation violation, string newMessage)
    {
        if (violation == null)
            throw new ArgumentNullException(nameof(violation));

        if (string.IsNullOrWhiteSpace(newMessage))
            throw new ArgumentException("Message cannot be null or empty.", nameof(newMessage));

        var copy = new RuleViolation
        {
            Id = Guid.NewGuid().ToString(),
            RuleId = violation.RuleId,
            RuleName = violation.RuleName,
            Message = newMessage,
            Severity = violation.Severity,
            FilePath = violation.FilePath,
            LineNumber = violation.LineNumber,
            ColumnNumber = violation.ColumnNumber,
            CodeSnippet = violation.CodeSnippet,
            SuggestedFix = violation.SuggestedFix,
            DetectedAt = DateTime.UtcNow,
            ProjectName = violation.ProjectName,
            Category = violation.Category,
            Metadata = new Dictionary<string, string>(violation.Metadata)
        };

        return copy;
    }

    /// <summary>
    /// Creates a new violation with updated file location while preserving all other properties.
    /// </summary>
    /// <param name="violation">The original violation to copy from.</param>
    /// <param name="newFilePath">The new file path.</param>
    /// <param name="newLineNumber">The new line number.</param>
    /// <param name="newColumnNumber">The new column number.</param>
    /// <returns>A new RuleViolation instance with the updated location.</returns>
    public static RuleViolation WithLocation(this RuleViolation violation, string newFilePath, int newLineNumber, int newColumnNumber = 0)
    {
        if (violation == null)
            throw new ArgumentNullException(nameof(violation));

        if (string.IsNullOrWhiteSpace(newFilePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(newFilePath));

        if (newLineNumber <= 0)
            throw new ArgumentException("Line number must be positive.", nameof(newLineNumber));

        if (newColumnNumber < 0)
            throw new ArgumentException("Column number cannot be negative.", nameof(newColumnNumber));

        var copy = new RuleViolation
        {
            Id = Guid.NewGuid().ToString(),
            RuleId = violation.RuleId,
            RuleName = violation.RuleName,
            Message = violation.Message,
            Severity = violation.Severity,
            FilePath = newFilePath,
            LineNumber = newLineNumber,
            ColumnNumber = newColumnNumber,
            CodeSnippet = violation.CodeSnippet,
            SuggestedFix = violation.SuggestedFix,
            DetectedAt = DateTime.UtcNow,
            ProjectName = violation.ProjectName,
            Category = violation.Category,
            Metadata = new Dictionary<string, string>(violation.Metadata)
        };

        return copy;
    }

    /// <summary>
    /// Creates a new violation with updated severity level while preserving all other properties.
    /// </summary>
    /// <param name="violation">The original violation to copy from.</param>
    /// <param name="newSeverity">The new severity level.</param>
    /// <returns>A new RuleViolation instance with the updated severity.</returns>
    public static RuleViolation WithSeverity(this RuleViolation violation, SeverityLevel newSeverity)
    {
        if (violation == null)
            throw new ArgumentNullException(nameof(violation));

        var copy = new RuleViolation
        {
            Id = Guid.NewGuid().ToString(),
            RuleId = violation.RuleId,
            RuleName = violation.RuleName,
            Message = violation.Message,
            Severity = newSeverity,
            FilePath = violation.FilePath,
            LineNumber = violation.LineNumber,
            ColumnNumber = violation.ColumnNumber,
            CodeSnippet = violation.CodeSnippet,
            SuggestedFix = violation.SuggestedFix,
            DetectedAt = DateTime.UtcNow,
            ProjectName = violation.ProjectName,
            Category = violation.Category,
            Metadata = new Dictionary<string, string>(violation.Metadata)
        };

        return copy;
    }

    /// <summary>
    /// Creates a new violation with additional metadata while preserving all existing metadata.
    /// </summary>
    /// <param name="violation">The original violation to copy from.</param>
    /// <param name="key">The metadata key to add or update.</param>
    /// <param name="value">The metadata value to set.</param>
    /// <returns>A new RuleViolation instance with the additional metadata.</returns>
    public static RuleViolation WithMetadata(this RuleViolation violation, string key, string value)
    {
        if (violation == null)
            throw new ArgumentNullException(nameof(violation));

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty.", nameof(key));

        var copy = new RuleViolation
        {
            Id = Guid.NewGuid().ToString(),
            RuleId = violation.RuleId,
            RuleName = violation.RuleName,
            Message = violation.Message,
            Severity = violation.Severity,
            FilePath = violation.FilePath,
            LineNumber = violation.LineNumber,
            ColumnNumber = violation.ColumnNumber,
            CodeSnippet = violation.CodeSnippet,
            SuggestedFix = violation.SuggestedFix,
            DetectedAt = DateTime.UtcNow,
            ProjectName = violation.ProjectName,
            Category = violation.Category,
            Metadata = new Dictionary<string, string>(violation.Metadata)
        };

        copy.AddMetadata(key, value);
        return copy;
    }

    /// <summary>
    /// Creates a new violation with the same properties but a different detection timestamp.
    /// </summary>
    /// <param name="violation">The original violation to copy from.</param>
    /// <param name="newDetectedAt">The new detection timestamp.</param>
    /// <returns>A new RuleViolation instance with the updated timestamp.</returns>
    public static RuleViolation WithDetectedAt(this RuleViolation violation, DateTime newDetectedAt)
    {
        if (violation == null)
            throw new ArgumentNullException(nameof(violation));

        var copy = new RuleViolation
        {
            Id = Guid.NewGuid().ToString(),
            RuleId = violation.RuleId,
            RuleName = violation.RuleName,
            Message = violation.Message,
            Severity = violation.Severity,
            FilePath = violation.FilePath,
            LineNumber = violation.LineNumber,
            ColumnNumber = violation.ColumnNumber,
            CodeSnippet = violation.CodeSnippet,
            SuggestedFix = violation.SuggestedFix,
            DetectedAt = newDetectedAt,
            ProjectName = violation.ProjectName,
            Category = violation.Category,
            Metadata = new Dictionary<string, string>(violation.Metadata)
        };

        return copy;
    }

    /// <summary>
    /// Determines if this violation belongs to a specific category.
    /// </summary>
    /// <param name="violation">The violation to check.</param>
    /// <param name="category">The category to check against.</param>
    /// <returns>True if the violation's category matches the specified category.</returns>
    public static bool HasCategory(this RuleViolation violation, RuleCategory category)
    {
        if (violation == null)
            throw new ArgumentNullException(nameof(violation));

        return violation.Category == category;
    }

    /// <summary>
    /// Determines if this violation belongs to any of the specified categories.
    /// </summary>
    /// <param name="violation">The violation to check.</param>
    /// <param name="categories">The categories to check against.</param>
    /// <returns>True if the violation's category matches any of the specified categories.</returns>
    public static bool HasAnyCategory(this RuleViolation violation, params RuleCategory[] categories)
    {
        if (violation == null)
            throw new ArgumentNullException(nameof(violation));

        if (categories == null || categories.Length == 0)
            return false;

        return categories.Contains(violation.Category);
    }

    /// <summary>
    /// Gets a formatted string containing the violation's code snippet if available.
    /// </summary>
    /// <param name="violation">The violation to get snippet from.</param>
    /// <returns>Formatted code snippet with line numbers, or null if no snippet exists.</returns>
    public static string? GetFormattedCodeSnippet(this RuleViolation violation)
    {
        if (violation == null)
            throw new ArgumentNullException(nameof(violation));

        if (string.IsNullOrWhiteSpace(violation.CodeSnippet))
            return null;

        var lines = violation.CodeSnippet.Split(new[] { '\n' }, StringSplitOptions.None);
        var sb = new StringBuilder();

        for (int i = 0; i < lines.Length; i++)
        {
            sb.AppendLine($"{violation.LineNumber + i,4} | {lines[i]}");
        }

        return sb.ToString().TrimEnd();
    }
}