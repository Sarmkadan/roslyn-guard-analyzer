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
using System.Text.Json;
using System.Text.Json.Serialization;

using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Core;

namespace RoslynGuardAnalyzer.Formatters;

/// <summary>
/// Formats analysis results as SARIF 2.1.0 (Static Analysis Results Interchange Format).
/// SARIF is a JSON-based standard format for the output of static analysis tools.
/// </summary>
public sealed class SarifFormatter : IOutputFormatter, IEquatable<SarifFormatter>
{
    private const string SarifVersion = "2.1.0";
    private const string SarifSchema = "https://raw.githubusercontent.com/oasis-tcs/sarif-spec/master/Schemata/sarif-schema-2.1.0.json";

    /// <summary>
    /// Gets the format identifier for SARIF output.
    /// </summary>
    public string Format => "sarif";

    /// <summary>
    /// Formats an analysis result into SARIF 2.1.0 format.
    /// </summary>
    public string FormatResult(AnalysisResult result)
    {
        var sarifReport = CreateSarifReport(result);
        return JsonSerializer.Serialize(sarifReport, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Formats a collection of violations into SARIF format.
    /// </summary>
    public string FormatViolations(IEnumerable<RuleViolation> violations)
    {
        var analysisResult = new AnalysisResult
        {
            ProjectName = "Violations Report",
            Violations = violations.ToList(),
            AnalysisStartTime = DateTime.UtcNow,
            AnalysisEndTime = DateTime.UtcNow
        };
        return FormatResult(analysisResult);
    }

    /// <summary>
    /// Formats a violation report into SARIF format.
    /// </summary>
    public string FormatReport(ViolationReport report)
    {
        // Convert ViolationReport to AnalysisResult for SARIF formatting
        var analysisResult = new AnalysisResult
        {
            ProjectName = report.ProjectName,
            Violations = report.ViolationGroups
                .SelectMany(g => g.Violations)
                .ToList(),
            AnalysisStartTime = report.GeneratedAt,
            AnalysisEndTime = DateTime.UtcNow
        };
        return FormatResult(analysisResult);
    }

    /// <summary>
    /// Checks if this formatter can handle the given format identifier.
    /// </summary>
    public bool CanFormat(string format)
    {
        return string.Equals(format, Format, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a minimal valid SARIF 2.1.0 report from an analysis result.
    /// </summary>
    private static SarifReport CreateSarifReport(AnalysisResult result)
    {
        var violations = result.Violations ?? new List<RuleViolation>();

        // Create rules collection
        var rules = new List<SarifRule>();
        var ruleIds = violations
            .Select(v => v.RuleId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var ruleId in ruleIds)
        {
            rules.Add(new SarifRule
            {
                Id = ruleId,
                Name = violations.FirstOrDefault(v => v.RuleId.Equals(ruleId, StringComparison.OrdinalIgnoreCase))?.RuleName ?? ruleId,
                ShortDescription = new SarifMessage
                {
                    Text = ruleId
                },
                HelpUri = GetRuleHelpUri(ruleId)
            });
        }

        // Create results collection
        var results = new List<SarifResult>();
        var ruleIndex = 0;

        foreach (var violation in violations)
        {
            var sarifLevel = MapSeverityToSarifLevel(violation.Severity);
            var ruleId = violation.RuleId;

            // Find rule index
            var rule = rules.FirstOrDefault(r => r.Id.Equals(ruleId, StringComparison.OrdinalIgnoreCase));
            var ruleIndexValue = rule != null ? rules.IndexOf(rule) : ruleIndex++;

            var resultObj = new SarifResult
            {
                RuleId = ruleId,
                RuleIndex = ruleIndexValue,
                Level = sarifLevel,
                Message = new SarifMessage
                {
                    Text = violation.Message
                },
                Locations = new List<SarifLocation>
                {
                    new SarifLocation
                    {
                        PhysicalLocation = new SarifPhysicalLocation
                        {
                            ArtifactLocation = new SarifArtifactLocation
                            {
                                Uri = violation.FilePath.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                    ? new Uri(violation.FilePath)
                                    : new Uri(violation.FilePath, UriKind.RelativeOrAbsolute),
                                UriBaseId = "%SRCROOT%"
                            },
                            Region = new SarifRegion
                            {
                                StartLine = violation.LineNumber,
                                StartColumn = violation.ColumnNumber > 0 ? violation.ColumnNumber : 1,
                                EndLine = violation.LineNumber,
                                EndColumn = violation.ColumnNumber > 0 ? violation.ColumnNumber + 1 : 1
                            }
                        }
                    }
                },
                Properties = new Dictionary<string, object>
                {
                    { "severity", violation.Severity.ToString() },
                    { "category", violation.Category.ToString() },
                    { "project", violation.ProjectName ?? result.ProjectName ?? string.Empty },
                    { "detectedAt", violation.DetectedAt.ToString("o") }
                },
                Guid = violation.Id
            };

            // Add code snippet if available
            if (!string.IsNullOrWhiteSpace(violation.CodeSnippet))
            {
                resultObj.Locations[0].PhysicalLocation.Region.Snippet = new SarifArtifactContent
                {
                    Text = violation.CodeSnippet
                };
            }

            results.Add(resultObj);
        }

        // Create the full SARIF report
        var sarifReport = new SarifReport
        {
            Version = SarifVersion,
            Schema = SarifSchema,
            Runs = new List<SarifRun>
            {
                new SarifRun
                {
                    Tool = new SarifTool
                    {
                        Driver = new SarifToolDriver
                        {
                            Name = "Roslyn Guard Analyzer",
                            InformationUri = new Uri("https://github.com/sarmkadan/roslyn-guard-analyzer"),
                            Version = "1.0.0",
                            Rules = rules
                        }
                    },
                    Invocations = new List<SarifInvocation>
                    {
                        new SarifInvocation
                        {
                            ExecutionSuccessful = result.ViolationCount == 0,
                            StartTimeUtc = result.AnalysisStartTime,
                            EndTimeUtc = result.AnalysisEndTime,
                            WorkingDirectory = new SarifArtifactLocation
                            {
                                Uri = new Uri(Environment.CurrentDirectory)
                            }
                        }
                    },
                    Results = results,
                    ColumnKind = "utf16CodeUnits"
                }
            }
        };

        return sarifReport;
    }

    /// <summary>
    /// Maps RoslynGuardAnalyzer severity to SARIF level.
    /// </summary>
    private static string MapSeverityToSarifLevel(SeverityLevel severity)
    {
        return severity switch
        {
            SeverityLevel.Critical => "error",
            SeverityLevel.Error => "error",
            SeverityLevel.Warning => "warning",
            _ => "note" // Info and default
        };
    }

    /// <summary>
    /// Gets a help URI for a rule ID.
    /// </summary>
    private static string GetRuleHelpUri(string ruleId)
    {
        return $"https://github.com/sarmkadan/roslyn-guard-analyzer/wiki/Rules#{ruleId.ToLowerInvariant()}";
    }

    #region IEquatable<SarifFormatter>
    public bool Equals(SarifFormatter? other)
    {
        return other is SarifFormatter otherFormatter &&
               Format == otherFormatter.Format;
    }

    public override bool Equals(object? obj)
    {
        return obj is SarifFormatter other &&
               Format == other.Format;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Format);
    }

    public static bool operator ==(SarifFormatter? left, SarifFormatter? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
        return left.Format == right.Format;
    }

    public static bool operator !=(SarifFormatter? left, SarifFormatter? right)
    {
        return !(left == right);
    }
    #endregion
}

/// <summary>
/// SARIF 2.1.0 report structure.
/// </summary>
internal sealed class SarifReport
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("schema")]
    public string Schema { get; set; } = string.Empty;

    [JsonPropertyName("$schema")]
    public string SchemaAlias
    {
        get => Schema;
        set => Schema = value;
    }

    [JsonPropertyName("runs")]
    public List<SarifRun> Runs { get; set; } = new List<SarifRun>();
}

/// <summary>
/// SARIF run structure.
/// </summary>
internal sealed class SarifRun
{
    [JsonPropertyName("tool")]
    public SarifTool Tool { get; set; } = new SarifTool();

    [JsonPropertyName("invocations")]
    public List<SarifInvocation> Invocations { get; set; } = new List<SarifInvocation>();

    [JsonPropertyName("results")]
    public List<SarifResult> Results { get; set; } = new List<SarifResult>();

    [JsonPropertyName("columnKind")]
    public string ColumnKind { get; set; } = "utf16CodeUnits";
}

/// <summary>
/// SARIF tool structure.
/// </summary>
internal sealed class SarifTool
{
    [JsonPropertyName("driver")]
    public SarifToolDriver Driver { get; set; } = new SarifToolDriver();
}

/// <summary>
/// SARIF tool driver structure.
/// </summary>
internal sealed class SarifToolDriver
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Roslyn Guard Analyzer";

    [JsonPropertyName("informationUri")]
    public Uri? InformationUri { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("rules")]
    public List<SarifRule> Rules { get; set; } = new List<SarifRule>();
}

/// <summary>
/// SARIF rule structure.
/// </summary>
internal sealed class SarifRule
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("shortDescription")]
    public SarifMessage ShortDescription { get; set; } = new SarifMessage();

    [JsonPropertyName("helpUri")]
    public string? HelpUri { get; set; }
}

/// <summary>
/// SARIF message structure.
/// </summary>
internal sealed class SarifMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// SARIF result structure.
/// </summary>
internal sealed class SarifResult
{
    [JsonPropertyName("ruleId")]
    public string RuleId { get; set; } = string.Empty;

    [JsonPropertyName("ruleIndex")]
    public int RuleIndex { get; set; }

    [JsonPropertyName("level")]
    public string Level { get; set; } = "warning";

    [JsonPropertyName("message")]
    public SarifMessage Message { get; set; } = new SarifMessage();

    [JsonPropertyName("locations")]
    public List<SarifLocation> Locations { get; set; } = new List<SarifLocation>();

    [JsonPropertyName("guid")]
    public string? Guid { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// SARIF location structure.
/// </summary>
internal sealed class SarifLocation
{
    [JsonPropertyName("physicalLocation")]
    public SarifPhysicalLocation PhysicalLocation { get; set; } = new SarifPhysicalLocation();
}

/// <summary>
/// SARIF physical location structure.
/// </summary>
internal sealed class SarifPhysicalLocation
{
    [JsonPropertyName("artifactLocation")]
    public SarifArtifactLocation ArtifactLocation { get; set; } = new SarifArtifactLocation();

    [JsonPropertyName("region")]
    public SarifRegion Region { get; set; } = new SarifRegion();
}

/// <summary>
/// SARIF artifact location structure.
/// </summary>
internal sealed class SarifArtifactLocation
{
    [JsonPropertyName("uri")]
    public Uri Uri { get; set; } = new Uri("file:///unknown");

    [JsonPropertyName("uriBaseId")]
    public string? UriBaseId { get; set; }
}

/// <summary>
/// SARIF region structure.
/// </summary>
internal sealed class SarifRegion
{
    [JsonPropertyName("startLine")]
    public int StartLine { get; set; }

    [JsonPropertyName("startColumn")]
    public int StartColumn { get; set; } = 1;

    [JsonPropertyName("endLine")]
    public int EndLine { get; set; }

    [JsonPropertyName("endColumn")]
    public int EndColumn { get; set; } = 1;

    [JsonPropertyName("snippet")]
    public SarifArtifactContent? Snippet { get; set; }
}

/// <summary>
/// SARIF artifact content structure.
/// </summary>
internal sealed class SarifArtifactContent
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// SARIF invocation structure.
/// </summary>
internal sealed class SarifInvocation
{
    [JsonPropertyName("executionSuccessful")]
    public bool ExecutionSuccessful { get; set; }

    [JsonPropertyName("startTimeUtc")]
    public DateTime StartTimeUtc { get; set; }

    [JsonPropertyName("endTimeUtc")]
    public DateTime EndTimeUtc { get; set; }

    [JsonPropertyName("workingDirectory")]
    public SarifArtifactLocation WorkingDirectory { get; set; } = new SarifArtifactLocation();
}