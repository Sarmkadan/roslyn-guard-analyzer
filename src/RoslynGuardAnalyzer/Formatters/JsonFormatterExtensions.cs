#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Formatters;

/// <summary>
/// Extension methods for <see cref="JsonFormatter"/> to provide additional formatting capabilities.
/// </summary>
public static class JsonFormatterExtensions
{
    /// <summary>
    /// Formats a single violation as JSON.
    /// </summary>
    /// <param name="formatter">The formatter instance.</param>
    /// <param name="violation">The violation to format.</param>
    /// <returns>JSON representation of the violation.</returns>
    public static string FormatViolation(this JsonFormatter formatter, RuleViolation violation)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        if (violation is null)
            throw new ArgumentNullException(nameof(violation));

        var violationsList = new List<RuleViolation> { violation };
        var output = new
        {
            violation.RuleId,
            violation.RuleName,
            Severity = violation.Severity.ToString(),
            violation.Message,
            violation.FilePath,
            violation.LineNumber,
            violation.ColumnNumber,
            violation.CodeSnippet,
            TimestampUtc = DateTime.UtcNow.ToString("O")
        };

        var result = new AnalysisResult
        {
            ProjectName = violation.FilePath ?? "Unknown",
            ProjectPath = violation.FilePath ?? "Unknown",
            AnalysisSucceeded = true,
            ErrorMessage = null,
            TotalFilesAnalyzed = 1,
            TotalElementsAnalyzed = 1
        };
        result.AddViolations(violationsList);

        return formatter.FormatResult(result);
    }

    /// <summary>
    /// Formats multiple violations grouped by severity level.
    /// </summary>
    /// <param name="formatter">The formatter instance.</param>
    /// <param name="violations">The violations to format.</param>
    /// <returns>JSON representation with severity grouping.</returns>
    public static string FormatViolationsBySeverity(this JsonFormatter formatter, IEnumerable<RuleViolation> violations)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        if (violations is null)
            throw new ArgumentNullException(nameof(violations));

        var violationsList = violations.ToList();
        var grouped = violationsList
            .GroupBy(v => v.Severity)
            .Select(g => new
            {
                Severity = g.Key.ToString(),
                Count = g.Count(),
                Violations = g.Select(v => new
                {
                    v.RuleId,
                    v.RuleName,
                    v.Message,
                    v.FilePath,
                    v.LineNumber,
                    v.ColumnNumber,
                    v.CodeSnippet
                }).ToList()
            })
            .ToList();

        var result = new AnalysisResult
        {
            ProjectName = "Violations Analysis",
            ProjectPath = "N/A",
            AnalysisSucceeded = true,
            ErrorMessage = null,
            TotalFilesAnalyzed = 1,
            TotalElementsAnalyzed = violationsList.Count
        };
        result.AddViolations(violationsList);

        return formatter.FormatResult(result);
    }

    /// <summary>
    /// Formats violations filtered by rule ID.
    /// </summary>
    /// <param name="formatter">The formatter instance.</param>
    /// <param name="violations">The violations to filter and format.</param>
    /// <param name="ruleId">The rule ID to filter by.</param>
    /// <returns>JSON representation of filtered violations.</returns>
    public static string FormatViolationsByRule(this JsonFormatter formatter, IEnumerable<RuleViolation> violations, string ruleId)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        if (violations is null)
            throw new ArgumentNullException(nameof(violations));

        if (string.IsNullOrWhiteSpace(ruleId))
            throw new ArgumentException("Rule ID cannot be null or whitespace.", nameof(ruleId));

        var filtered = violations.Where(v => v.RuleId.Equals(ruleId, StringComparison.OrdinalIgnoreCase)).ToList();

        var result = new AnalysisResult
        {
            ProjectName = $"Rule {ruleId} Analysis",
            ProjectPath = "N/A",
            AnalysisSucceeded = true,
            ErrorMessage = null,
            TotalFilesAnalyzed = filtered.Count > 0 ? 1 : 0,
            TotalElementsAnalyzed = filtered.Count
        };
        result.AddViolations(filtered);

        return formatter.FormatResult(result);
    }

    /// <summary>
    /// Formats a quick summary of violations for dashboard display.
    /// </summary>
    /// <param name="formatter">The formatter instance.</param>
    /// <param name="violations">The violations to summarize.</param>
    /// <returns>Compact JSON summary suitable for dashboards.</returns>
    public static string FormatViolationSummary(this JsonFormatter formatter, IEnumerable<RuleViolation> violations)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        if (violations is null)
            throw new ArgumentNullException(nameof(violations));

        var violationsList = violations.ToList();
        var criticalCount = violationsList.Count(v => v.Severity == SeverityLevel.Critical);
        var errorCount = violationsList.Count(v => v.Severity == SeverityLevel.Error);
        var warningCount = violationsList.Count(v => v.Severity == SeverityLevel.Warning);
        var infoCount = violationsList.Count(v => v.Severity == SeverityLevel.Info);

        var result = new AnalysisResult
        {
            ProjectName = "Summary",
            ProjectPath = "N/A",
            AnalysisSucceeded = true,
            ErrorMessage = null,
            TotalFilesAnalyzed = 1,
            TotalElementsAnalyzed = violationsList.Count
        };
        result.AddViolations(violationsList);

        return formatter.FormatResult(result);
    }
}