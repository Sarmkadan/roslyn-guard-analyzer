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
    // HTML Constants
    private const string Doctype = "<!DOCTYPE html>";
    private const string HtmlOpen = "<html>";
    private const string HtmlClose = "</html>";
    private const string HeadOpen = "<head>";
    private const string HeadClose = "</head>";
    private const string BodyOpen = "<body>";
    private const string BodyClose = "</body>";
    private const string ContainerOpen = "<div class=\"container\">";
    private const string ContainerClose = "</div>";
    private const string H1Open = "<h1>";
    private const string H1Close = "</h1>";
    private const string H2Open = "<h2>";
    private const string H2Close = "</h2>";
    private const string DivOpen = "<div";
    private const string DivClose = "</div>";
    private const string POpen = "<p>";
    private const string PClose = "</p>";
    private const string StrongOpen = "<strong>";
    private const string StrongClose = "</strong>";
    private const string TableOpen = "<table";
    private const string TableClose = "</table>";
    private const string TheadOpen = "<thead>";
    private const string TheadClose = "</thead>";
    private const string TbodyOpen = "<tbody>";
    private const string TbodyClose = "</tbody>";
    private const string TrOpen = "<tr";
    private const string TrClose = "</tr>";
    private const string ThOpen = "<th>";
    private const string ThClose = "</th>";
    private const string TdOpen = "<td>";
    private const string TdClose = "</td>";
    private const string MetaCharset = "<meta charset=\"utf-8\">";
    private const string TitlePrefix = "Analysis Report - ";
    private const string ReportTitle = "Roslyn Guard Analyzer Report";
    private const string ViolationsReportTitle = "Violations Report";
    private const string NoViolationsMessage = "✓ No violations found";
    private const string GeneratedFormat = "{0:yyyy-MM-dd HH:mm:ss} UTC";

    // CSS Class Constants
    private const string ClassContainer = "container";
    private const string ClassHeaderInfo = "header-info";
    private const string ClassSummary = "summary";
    private const string ClassStatBox = "stat-box";
    private const string ClassStatValue = "stat-value";
    private const string ClassStatLabel = "stat-label";
    private const string ClassViolationsTable = "violations-table";
    private const string ClassSuccess = "success";
    private const string ClassSeverityPrefix = "severity-";

    // Other Constants
    private const string ProjectLabel = "Project:";
    private const string PathLabel = "Path:";
    private const string GeneratedLabel = "Generated:";
    private const string RuleHeader = "Rule";
    private const string SeverityHeader = "Severity";
    private const string MessageHeader = "Message";
    private const string FileHeader = "File";
    private const string LineHeader = "Line";

    public string Format => "html";

    public bool CanFormat(string format)
    {
        ArgumentException.ThrowIfNullOrEmpty(format);
        return format.Equals(Format, StringComparison.OrdinalIgnoreCase);
    }

    public string FormatResult(AnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return BuildHtml(result.ProjectName, result.ProjectPath, result.Violations);
    }

    public string FormatViolations(IEnumerable<RuleViolation> violations)
    {
        ArgumentNullException.ThrowIfNull(violations);
        return BuildHtml("Violations Report", string.Empty, violations.ToList());
    }

    public string FormatReport(ViolationReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return BuildHtml(report.Title, report.ProjectName, report.ViolationGroups.SelectMany(g => g.Violations).ToList());
    }

    private static string BuildHtml(string title, string projectPath, IReadOnlyList<RuleViolation> violations)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Doctype);
        sb.AppendLine(HtmlOpen);
        sb.AppendLine(HeadOpen);
        sb.AppendLine(MetaCharset);
        sb.AppendLine($"<title>{HtmlEscape(TitlePrefix + title)}</title>");
        sb.AppendLine(GetStyles());
        sb.AppendLine(HeadClose);
        sb.AppendLine(BodyOpen);
        sb.AppendLine(ContainerOpen);
        sb.AppendLine($"{H1Open}{ReportTitle}{H1Close}");
        sb.AppendLine($"<div class=\"{ClassHeaderInfo}\">");
        sb.AppendLine($"{POpen}{StrongOpen}{ProjectLabel}{StrongClose} {HtmlEscape(title)}{PClose}");
        if (!string.IsNullOrWhiteSpace(projectPath))
            sb.AppendLine($"{POpen}{StrongOpen}{PathLabel}{StrongClose} {HtmlEscape(projectPath)}{PClose}");
        sb.AppendLine($"{POpen}{StrongOpen}{GeneratedLabel}{StrongClose} {DateTime.UtcNow.ToString(GeneratedFormat)}{PClose}");
        sb.AppendLine(DivClose);
        sb.AppendLine($"<div class=\"{ClassSummary}\">");
        sb.AppendLine($"<div class=\"{ClassStatBox}\"><div class=\"{ClassStatValue}\">{violations.Count}</div><div class=\"{ClassStatLabel}\">Total Violations</div></div>");
        sb.AppendLine($"<div class=\"{ClassStatBox}\"><div class=\"{ClassStatValue}\">{violations.Select(v => v.FilePath).Distinct().Count()}</div><div class=\"{ClassStatLabel}\">Affected Files</div></div>");
        sb.AppendLine(DivClose);

        if (violations.Count > 0)
        {
            sb.AppendLine($"{H2Open}Violations{H2Close}");
            sb.AppendLine($"<table class=\"{ClassViolationsTable}\">");
            sb.AppendLine(TheadOpen);
            sb.AppendLine(TrOpen);
            sb.AppendLine(ThOpen + RuleHeader + ThClose);
            sb.AppendLine(ThOpen + SeverityHeader + ThClose);
            sb.AppendLine(ThOpen + MessageHeader + ThClose);
            sb.AppendLine(ThOpen + FileHeader + ThClose);
            sb.AppendLine(ThOpen + LineHeader + ThClose);
            sb.AppendLine(TrClose);
            sb.AppendLine(TheadClose);
            sb.AppendLine(TbodyOpen);

            foreach (var violation in violations.OrderByDescending(v => v.Severity))
            {
                var severityClass = violation.Severity.ToString().ToLowerInvariant();
                sb.AppendLine($"<tr class=\"{ClassSeverityPrefix}{severityClass}\">");
                sb.AppendLine(TdOpen + HtmlEscape(violation.RuleName) + TdClose);
                sb.AppendLine(TdOpen + violation.Severity + TdClose);
                sb.AppendLine(TdOpen + HtmlEscape(violation.Message) + TdClose);
                sb.AppendLine(TdOpen + HtmlEscape(System.IO.Path.GetFileName(violation.FilePath)) + TdClose);
                sb.AppendLine(TdOpen + violation.LineNumber + TdClose);
                sb.AppendLine(TrClose);
            }

            sb.AppendLine(TbodyClose);
            sb.AppendLine(TableClose);
        }
        else
        {
            sb.AppendLine($"<div class=\"{ClassSuccess}\"><p>{NoViolationsMessage}</p></div>");
        }

        sb.AppendLine(ContainerClose);
        sb.AppendLine(BodyClose);
        sb.AppendLine(HtmlClose);
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
