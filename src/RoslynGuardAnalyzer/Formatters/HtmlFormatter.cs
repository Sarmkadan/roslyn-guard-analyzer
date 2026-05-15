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
        return BuildHtml(result.ProjectName, result.ProjectPath, result.Violations);
    }

    public string FormatViolations(IEnumerable<RuleViolation> violations)
    {
        return BuildHtml("Violations Report", string.Empty, violations.ToList());
    }

    public string FormatReport(ViolationReport report)
    {
        return BuildHtml(report.Title, report.ProjectName, report.ViolationGroups.SelectMany(g => g.Violations).ToList());
    }

    private static string BuildHtml(string title, string projectPath, IReadOnlyList<RuleViolation> violations)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\">");
        sb.AppendLine("<title>Analysis Report - " + HtmlEscape(title) + "</title>");
        sb.AppendLine(GetStyles());
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"container\">");
        sb.AppendLine("<h1>Roslyn Guard Analyzer Report</h1>");
        sb.AppendLine("<div class=\"header-info\">");
        sb.AppendLine($"<p><strong>Project:</strong> {HtmlEscape(title)}</p>");
        if (!string.IsNullOrWhiteSpace(projectPath))
            sb.AppendLine($"<p><strong>Path:</strong> {HtmlEscape(projectPath)}</p>");
        sb.AppendLine($"<p><strong>Generated:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");
        sb.AppendLine("</div>");
        sb.AppendLine("<div class=\"summary\">");
        sb.AppendLine($"<div class=\"stat-box\"><div class=\"stat-value\">{violations.Count}</div><div class=\"stat-label\">Total Violations</div></div>");
        sb.AppendLine($"<div class=\"stat-box\"><div class=\"stat-value\">{violations.Select(v => v.FilePath).Distinct().Count()}</div><div class=\"stat-label\">Affected Files</div></div>");
        sb.AppendLine("</div>");

        if (violations.Count > 0)
        {
            sb.AppendLine("<h2>Violations</h2>");
            sb.AppendLine("<table class=\"violations-table\">");
            sb.AppendLine("<thead><tr><th>Rule</th><th>Severity</th><th>Message</th><th>File</th><th>Line</th></tr></thead>");
            sb.AppendLine("<tbody>");

            foreach (var violation in violations.OrderByDescending(v => v.Severity))
            {
                var severityClass = violation.Severity.ToString().ToLowerInvariant();
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
.summary { display: flex; gap: 20px; margin: 20px 0; flex-wrap: wrap; }
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
.violations-table { width: 100%; border-collapse: collapse; }
.violations-table th, .violations-table td { padding: 12px; border-bottom: 1px solid #ddd; text-align: left; }
.severity-critical { background: #fee2e2; }
.severity-error { background: #ffedd5; }
.severity-warning { background: #fef3c7; }
.severity-info { background: #dbeafe; }
.success { background: #dcfce7; padding: 16px; border-radius: 4px; }
</style>";
    }

    private static string HtmlEscape(string text)
    {
        return text
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal);
    }
}
