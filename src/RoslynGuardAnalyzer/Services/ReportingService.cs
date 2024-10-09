// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;

using RoslynGuardAnalyzer.Domain.Models;
namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Generates formatted reports from analysis results.
/// </summary>
public sealed class ReportingService : IReportingService
{
    /// <summary>
    /// Generates a human-readable text report from analysis results.
    /// </summary>
    public string GenerateReport(AnalysisResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        var sb = new StringBuilder();

        sb.AppendLine("╔═══════════════════════════════════════════════════════════╗");
        sb.AppendLine("║           ROSLYN GUARD ANALYZER - ANALYSIS REPORT          ║");
        sb.AppendLine("╚═══════════════════════════════════════════════════════════╝");
        sb.AppendLine();

        sb.AppendLine($"Project: {result.ProjectName}");
        sb.AppendLine($"Path: {result.ProjectPath}");
        sb.AppendLine($"Analysis Started: {result.AnalysisStartTime:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Analysis Ended: {result.AnalysisEndTime:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Duration: {result.GetDuration().TotalSeconds:F2}s");
        sb.AppendLine();

        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine("SUMMARY");
        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine($"Total Violations: {result.ViolationCount}");
        sb.AppendLine($"Files Analyzed: {result.TotalFilesAnalyzed}");
        sb.AppendLine($"Code Elements Analyzed: {result.TotalElementsAnalyzed}");
        sb.AppendLine($"Success Percentage: {result.GetSuccessPercentage():F1}%");
        sb.AppendLine();

        if (result.ViolationsBySeverity.Any())
        {
            sb.AppendLine("Violations by Severity:");
            foreach (var kvp in result.ViolationsBySeverity.OrderByDescending(k => k.Key))
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
            sb.AppendLine();
        }

        if (result.ViolationsByCategory.Any())
        {
            sb.AppendLine("Violations by Category:");
            foreach (var kvp in result.ViolationsByCategory.OrderByDescending(k => k.Value))
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine("DETAILED VIOLATIONS");
        sb.AppendLine("═══════════════════════════════════════════════════════════");

        if (result.ViolationCount == 0)
        {
            sb.AppendLine("✓ No violations found!");
        }
        else
        {
            var groupedByFile = result.Violations.GroupBy(v => v.FilePath);

            foreach (var fileGroup in groupedByFile)
            {
                sb.AppendLine();
                sb.AppendLine($"📄 {Path.GetFileName(fileGroup.Key)}");
                sb.AppendLine(new string('─', 60));

                foreach (var violation in fileGroup.OrderBy(v => v.LineNumber))
                {
                    var severityIcon = violation.Severity switch
                    {
                        SeverityLevel.Critical => "🔴",
                        SeverityLevel.Error => "❌",
                        SeverityLevel.Warning => "⚠️",
                        _ => "ℹ️"
                    };

                    sb.AppendLine($"{severityIcon} [{violation.RuleId}] Line {violation.LineNumber}: {violation.Message}");

                    if (!string.IsNullOrEmpty(violation.SuggestedFix))
                    {
                        sb.AppendLine($"   💡 Suggestion: {violation.SuggestedFix}");
                    }
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine($"Report generated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("═══════════════════════════════════════════════════════════");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a report in the specified format (JSON, XML, CSV, etc.).
    /// </summary>
    public string GenerateFormattedReport(AnalysisResult result, string format)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        return format?.ToUpperInvariant() switch
        {
            "JSON" => GenerateJsonReport(result),
            "CSV" => GenerateCsvReport(result),
            "XML" => GenerateXmlReport(result),
            _ => GenerateReport(result)
        };
    }

    /// <summary>
    /// Saves a violation report to a file asynchronously.
    /// </summary>
    public async Task SaveReportAsync(ViolationReport report, string filePath)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var content = report.Format switch
        {
            ReportFormat.Json => SerializeToJson(report),
            ReportFormat.Xml => SerializeToXml(report),
            ReportFormat.Csv => SerializeToCsv(report),
            _ => report.DetailedContent
        };

        await File.WriteAllTextAsync(filePath, content);
    }

    /// <summary>
    /// Generates a JSON formatted report.
    /// </summary>
    private string GenerateJsonReport(AnalysisResult result)
    {
        var reportData = new
        {
            result.ProjectName,
            result.ProjectPath,
            result.AnalysisStartTime,
            result.AnalysisEndTime,
            result.AnalysisSucceeded,
            TotalViolations = result.ViolationCount,
            FilesAnalyzed = result.TotalFilesAnalyzed,
            ElementsAnalyzed = result.TotalElementsAnalyzed,
            Violations = result.Violations.Select(v => new
            {
                v.RuleId,
                v.RuleName,
                v.Message,
                v.Severity,
                v.FilePath,
                v.LineNumber,
                v.ColumnNumber,
                v.Category
            })
        };

        return JsonSerializer.Serialize(reportData, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Generates a CSV formatted report.
    /// </summary>
    private string GenerateCsvReport(AnalysisResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("RuleId,RuleName,Severity,Category,FilePath,LineNumber,ColumnNumber,Message");

        foreach (var violation in result.Violations)
        {
            var message = violation.Message.Replace("\"", "\"\"");
            sb.AppendLine(
                $"\"{violation.RuleId}\",\"{violation.RuleName}\",\"{violation.Severity}\"," +
                $"\"{violation.Category}\",\"{violation.FilePath}\",{violation.LineNumber}," +
                $"{violation.ColumnNumber},\"{message}\"");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates an XML formatted report.
    /// </summary>
    private string GenerateXmlReport(AnalysisResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<AnalysisReport>");
        sb.AppendLine($"  <Project>{XmlEscape(result.ProjectName)}</Project>");
        sb.AppendLine($"  <Path>{XmlEscape(result.ProjectPath)}</Path>");
        sb.AppendLine($"  <AnalysisTime>{result.AnalysisStartTime:O}</AnalysisTime>");
        sb.AppendLine($"  <TotalViolations>{result.ViolationCount}</TotalViolations>");
        sb.AppendLine("  <Violations>");

        foreach (var violation in result.Violations)
        {
            sb.AppendLine("    <Violation>");
            sb.AppendLine($"      <RuleId>{XmlEscape(violation.RuleId)}</RuleId>");
            sb.AppendLine($"      <Message>{XmlEscape(violation.Message)}</Message>");
            sb.AppendLine($"      <Severity>{violation.Severity}</Severity>");
            sb.AppendLine($"      <File>{XmlEscape(violation.FilePath)}</File>");
            sb.AppendLine($"      <Line>{violation.LineNumber}</Line>");
            sb.AppendLine("    </Violation>");
        }

        sb.AppendLine("  </Violations>");
        sb.AppendLine("</AnalysisReport>");

        return sb.ToString();
    }

    /// <summary>
    /// Serializes report to JSON.
    /// </summary>
    private string SerializeToJson(ViolationReport report)
    {
        return JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Serializes report to CSV.
    /// </summary>
    private string SerializeToCsv(ViolationReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("RuleId,Category,Severity,Message,FilePath,LineNumber");

        foreach (var group in report.ViolationGroups)
        {
            foreach (var violation in group.Violations)
            {
                var message = violation.Message.Replace("\"", "\"\"");
                sb.AppendLine(
                    $"\"{violation.RuleId}\",\"{violation.Category}\",\"{violation.Severity}\"," +
                    $"\"{message}\",\"{violation.FilePath}\",{violation.LineNumber}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Serializes report to XML.
    /// </summary>
    private string SerializeToXml(ViolationReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine($"<Report Title=\"{XmlEscape(report.Title)}\" Generated=\"{report.GeneratedAt:O}\">");

        foreach (var group in report.ViolationGroups)
        {
            sb.AppendLine($"  <ViolationGroup Name=\"{XmlEscape(group.Name)}\">");

            foreach (var violation in group.Violations)
            {
                sb.AppendLine("    <Violation>");
                sb.AppendLine($"      <RuleId>{XmlEscape(violation.RuleId)}</RuleId>");
                sb.AppendLine($"      <Message>{XmlEscape(violation.Message)}</Message>");
                sb.AppendLine($"      <Severity>{violation.Severity}</Severity>");
                sb.AppendLine("    </Violation>");
            }

            sb.AppendLine("  </ViolationGroup>");
        }

        sb.AppendLine("</Report>");

        return sb.ToString();
    }

    /// <summary>
    /// Escapes XML special characters.
    /// </summary>
    private static string XmlEscape(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
