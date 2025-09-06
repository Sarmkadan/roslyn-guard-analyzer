// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        return format.Equals(Format, StringComparison.OrdinalIgnoreCase);
    }

    public string FormatResult(AnalysisResult result)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("Rule,Severity,Message,File,Line,Column,Code");

        // Violations
        foreach (var violation in result.Violations)
        {
            sb.AppendLine(FormatViolationAsCsv(violation));
        }

        return sb.ToString();
    }

    public string FormatViolations(IEnumerable<RuleViolation> violations)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("Rule,Severity,Message,File,Line,Column,Code");

        // Violations
        foreach (var violation in violations)
        {
            sb.AppendLine(FormatViolationAsCsv(violation));
        }

        return sb.ToString();
    }

    public string FormatReport(ViolationReport report)
    {
        var sb = new StringBuilder();

        // Summary section
        sb.AppendLine("SUMMARY");
        sb.AppendLine($"Title,{CsvEscape(report.Title)}");
        sb.AppendLine($"Generated,{report.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Total Violations,{report.Violations.Count}");
        sb.AppendLine();

        // Severity summary
        sb.AppendLine("SEVERITY SUMMARY");
        sb.AppendLine("Severity,Count");
        sb.AppendLine($"Critical,{report.Violations.Count(v => v.Severity == "Critical")}");
        sb.AppendLine($"High,{report.Violations.Count(v => v.Severity == "High")}");
        sb.AppendLine($"Medium,{report.Violations.Count(v => v.Severity == "Medium")}");
        sb.AppendLine($"Low,{report.Violations.Count(v => v.Severity == "Low")}");
        sb.AppendLine();

        // Rules summary
        sb.AppendLine("VIOLATIONS BY RULE");
        sb.AppendLine("Rule,Count,Severity");

        var ruleGroups = report.Violations.GroupBy(v => v.RuleName);
        foreach (var group in ruleGroups)
        {
            var severity = group.First().Severity;
            sb.AppendLine($"{CsvEscape(group.Key)},{group.Count()},{severity}");
        }

        sb.AppendLine();

        // Detailed violations
        sb.AppendLine("DETAILED VIOLATIONS");
        sb.AppendLine("Rule,Severity,Message,File,Line,Column,Code");

        foreach (var violation in report.Violations)
        {
            sb.AppendLine(FormatViolationAsCsv(violation));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats a single violation as a CSV line.
    /// </summary>
    private static string FormatViolationAsCsv(RuleViolation violation)
    {
        return $"{CsvEscape(violation.RuleName)},{violation.Severity}," +
               $"{CsvEscape(violation.Message)},{CsvEscape(violation.FilePath)}," +
               $"{violation.LineNumber},{violation.ColumnNumber}," +
               $"{CsvEscape(violation.Code ?? "N/A")}";
    }

    /// <summary>
    /// Escapes a string for CSV output (encloses in quotes and escapes quotes inside).
    /// </summary>
    private static string CsvEscape(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "\"\"";

        // If text contains comma, quote, or newline, enclose in quotes and escape quotes
        if (text.Contains(',') || text.Contains('"') || text.Contains('\n'))
        {
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        return text;
    }
}
