#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Formatters;

/// <summary>
/// Formats analysis results as CSV (Comma-Separated Values) output.
/// Suitable for import into spreadsheet applications and data analysis tools.
/// </summary>
public sealed class CsvFormatter : IOutputFormatter
{
    public string Format => "csv";

    public bool CanFormat(string format)
    {
        ArgumentException.ThrowIfNullOrEmpty(format);
        return format.Equals(Format, StringComparison.OrdinalIgnoreCase);
    }

    public string FormatResult(AnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return FormatViolations(result.Violations);
    }

    public string FormatViolations(IEnumerable<RuleViolation> violations)
    {
        ArgumentNullException.ThrowIfNull(violations);
        var sb = new StringBuilder();
        sb.AppendLine("Rule,Severity,Message,File,Line,Column,Code");

        foreach (var violation in violations)
            sb.AppendLine(FormatViolationAsCsv(violation));

        return sb.ToString();
    }

    public string FormatReport(ViolationReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var violations = report.ViolationGroups.SelectMany(g => g.Violations).ToList();
        var sb = new StringBuilder();

        sb.AppendLine("SUMMARY");
        sb.AppendLine($"Title,{CsvEscape(report.Title)}");
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture, $"Generated,{report.GeneratedAt:yyyy-MM-dd HH:mm:ss}"));
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture, $"Total Violations,{violations.Count}"));
        sb.AppendLine();

        sb.AppendLine("SEVERITY SUMMARY");
        sb.AppendLine("Severity,Count");
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture, $"Critical,{violations.Count(v => v.Severity == SeverityLevel.Critical)}"));
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture, $"High,{violations.Count(v => v.Severity == SeverityLevel.Error)}"));
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture, $"Medium,{violations.Count(v => v.Severity == SeverityLevel.Warning)}"));
        sb.AppendLine(string.Create(CultureInfo.InvariantCulture, $"Low,{violations.Count(v => v.Severity == SeverityLevel.Info)}"));
        sb.AppendLine();

        sb.AppendLine("VIOLATIONS BY RULE");
        sb.AppendLine("Rule,Count,Severity");
        foreach (var group in violations
                     .GroupBy(v => v.RuleName)
                     .OrderByDescending(g => g.Count())
                     .ThenBy(g => g.Key, StringComparer.Ordinal))
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture, $"{CsvEscape(group.Key)},{group.Count()},{group.Max(v => v.Severity)}"));

        sb.AppendLine();
        sb.AppendLine("DETAILED VIOLATIONS");
        sb.AppendLine("Rule,Severity,Message,File,Line,Column,Code");
        foreach (var violation in violations)
            sb.AppendLine(FormatViolationAsCsv(violation));

        return sb.ToString();
    }

    /// <summary>
    /// Formats a single violation as a CSV line.
    /// </summary>
    private static string FormatViolationAsCsv(RuleViolation violation)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{CsvEscape(violation.RuleName)},{violation.Severity},{CsvEscape(violation.Message)},{CsvEscape(violation.FilePath)},{violation.LineNumber},{violation.ColumnNumber},{CsvEscape(violation.CodeSnippet ?? "N/A")}");
    }

    /// <summary>
    /// Escapes a string for CSV output.
    /// </summary>
    private static string CsvEscape(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "\"\"";

        if (text.Contains(',') || text.Contains('"') || text.Contains('\n') || text.Contains('\r'))
            return "\"" + text.Replace("\"", "\"\"") + "\"";

        return text;
    }
}
