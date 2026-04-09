#nullable enable
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
/// Formats analysis results as HTML output for viewing in browsers.
/// Produces styled, readable HTML with summary statistics and violation details.
/// </summary>
public sealed class HtmlFormatter : IOutputFormatter
{
    public string Format => "html";

    public bool CanFormat(string format)
    {
        return format.Equals(Format, StringComparison.OrdinalIgnoreCase);
    }

    public string FormatResult(AnalysisResult result)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\">");
        sb.AppendLine("<title>Analysis Report - " + HtmlEscape(result.ProjectName) + "</title>");
        sb.AppendLine(GetStyles());
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // Header
        sb.AppendLine("<div class=\"container\">");
        sb.AppendLine("<h1>Roslyn Guard Analyzer Report</h1>");
        sb.AppendLine("<div class=\"header-info\">");
        sb.AppendLine($"<p><strong>Project:</strong> {HtmlEscape(result.ProjectName)}</p>");
        sb.AppendLine($"<p><strong>Path:</strong> {HtmlEscape(result.ProjectPath)}</p>");
        sb.AppendLine($"<p><strong>Generated:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");
        sb.AppendLine("</div>");

        // Summary
        sb.AppendLine("<div class=\"summary\">");
        sb.AppendLine($"<div class=\"stat-box\"><div class=\"stat-value\">{result.ViolationCount}</div><div class=\"stat-label\">Total Violations</div></div>");
        sb.AppendLine($"<div class=\"stat-box\"><div class=\"stat-value\">{result.TotalFilesAnalyzed}</div><div class=\"stat-label\">Files Analyzed</div></div>");
        sb.AppendLine($"<div class=\"stat-box\"><div class=\"stat-value\">{result.TotalElementsAnalyzed}</div><div class=\"stat-label\">Elements Analyzed</div></div>");
        sb.AppendLine("</div>");

        // Violations table
        if (result.Violations.Count > 0)
        {
            sb.AppendLine("<h2>Violations</h2>");
            sb.AppendLine("<table class=\"violations-table\">");
            sb.AppendLine("<thead><tr><th>Rule</th><th>Severity</th><th>Message</th><th>File</th><th>Line</th></tr></thead>");
            sb.AppendLine("<tbody>");

            foreach (var violation in result.Violations.OrderByDescending(v => v.Severity))
            {
                var severityClass = violation.Severity.ToLowerInvariant();
                sb.AppendLine($"<tr class=\"severity-{severityClass}\">");
                sb.AppendLine($"<td>{HtmlEscape(violation.RuleName)}</td>");
                sb.AppendLine($"<td>{violation.Severity}</td>");
                sb.AppendLine($"<td>{HtmlEscape(violation.Message)}</td>");
                sb.AppendLine($"<td>{HtmlEscape(System.IO.Path.GetFileName(violation.FilePath))}</td>");
                sb.AppendLine($"<td>{violation.LineNumber}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
        }
        else
        {
            sb.AppendLine("<div class=\"success\"><p>✓ No violations found</p></div>");
        }

        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    public string FormatViolations(IEnumerable<RuleViolation> violations)
    {
        var violationList = violations.ToList();

        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\">");
        sb.AppendLine("<title>Violations Report</title>");
        sb.AppendLine(GetStyles());
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        sb.AppendLine("<div class=\"container\">");
        sb.AppendLine($"<h1>Violations Report ({violationList.Count} violations)</h1>");

        sb.AppendLine("<table class=\"violations-table\">");
        sb.AppendLine("<thead><tr><th>Rule</th><th>Severity</th><th>Message</th><th>File</th><th>Line:Column</th></tr></thead>");
        sb.AppendLine("<tbody>");

        foreach (var violation in violationList.OrderByDescending(v => v.Severity))
        {
            var severityClass = violation.Severity.ToLowerInvariant();
            sb.AppendLine($"<tr class=\"severity-{severityClass}\">");
            sb.AppendLine($"<td>{HtmlEscape(violation.RuleName)}</td>");
            sb.AppendLine($"<td>{violation.Severity}</td>");
            sb.AppendLine($"<td>{HtmlEscape(violation.Message)}</td>");
            sb.AppendLine($"<td>{HtmlEscape(System.IO.Path.GetFileName(violation.FilePath))}</td>");
            sb.AppendLine($"<td>{violation.LineNumber}:{violation.ColumnNumber}</td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");

        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    public string FormatReport(ViolationReport report)
    {
        return FormatResult(null!); // Simplified for now
    }

    /// <summary>
    /// Returns embedded CSS styles for the HTML report.
    /// </summary>
    private static string GetStyles()
    {
        return @"<style>
body {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
    line-height: 1.6;
    color: #333;
    background: #f5f5f5;
}
.container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}
h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }
h2 { color: #34495e; margin-top: 30px; }
.header-info { background: #ecf0f1; padding: 15px; border-radius: 4px; }
.summary {
    display: flex;
    gap: 20px;
    margin: 20px 0;
    flex-wrap: wrap;
}
.stat-box {
    flex: 1;
    min-width: 150px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 20px;
    border-radius: 8px;
    text-align: center;
}
.stat-value { font-size: 32px; font-weight: bold; }
.stat-label { font-size: 14px; opacity: 0.9; }
.violations-table {
    width: 100%;
    border-collapse: collapse;
    margin: 20px 0;
}
.violations-table th {
    background: #34495e;
    color: white;
    padding: 12px;
    text-align: left;
    font-weight: 600;
}
.violations-table td {
    padding: 10px 12px;
    border-bottom: 1px solid #ecf0f1;
}
.violations-table tr:hover {
    background: #f9f9f9;
}
.severity-critical { background: #fee; border-left: 4px solid #e74c3c; }
.severity-high { background: #ffeaa7; border-left: 4px solid #f39c12; }
.severity-medium { background: #e8f4f8; border-left: 4px solid #3498db; }
.severity-low { background: #eafaf1; border-left: 4px solid #27ae60; }
.success {
    background: #d4edda;
    border: 1px solid #c3e6cb;
    color: #155724;
    padding: 15px;
    border-radius: 4px;
    text-align: center;
    font-weight: 500;
}
</style>";
    }

    /// <summary>
    /// Escapes special HTML characters to prevent XSS.
    /// </summary>
    private static string HtmlEscape(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}
