#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Formatters;

/// <summary>
/// Formats analysis results as JSON output.
/// Produces valid, minified JSON suitable for programmatic consumption.
/// </summary>
public sealed class JsonFormatter : IOutputFormatter
{
    public string Format => "json";

    public bool CanFormat(string format)
    {
        return format.Equals(Format, StringComparison.OrdinalIgnoreCase);
    }

    public string FormatResult(AnalysisResult result)
    {
        var violations = result.Violations.Select(v => new
        {
            v.RuleId,
            v.RuleName,
            Severity = v.Severity.ToString(),
            v.Message,
            v.FilePath,
            v.LineNumber,
            v.ColumnNumber,
            v.CodeSnippet
        }).ToList();

        var output = new
        {
            result.ProjectName,
            result.ProjectPath,
            result.AnalysisSucceeded,
            result.ErrorMessage,
            result.TotalFilesAnalyzed,
            result.TotalElementsAnalyzed,
            ViolationCount = result.ViolationCount,
            Violations = violations,
            TimestampUtc = DateTime.UtcNow.ToString("O")
        };

        return JsonSerialize(output);
    }

    public string FormatViolations(IEnumerable<RuleViolation> violations)
    {
        var violationList = violations.Select(v => new
        {
            v.RuleId,
            v.RuleName,
            Severity = v.Severity.ToString(),
            v.Message,
            v.FilePath,
            v.LineNumber,
            v.ColumnNumber,
            Code = v.CodeSnippet ?? "N/A"
        }).ToList();

        return JsonSerialize(new
        {
            Count = violationList.Count,
            Violations = violationList
        });
    }

    public string FormatReport(ViolationReport report)
    {
        var violations = report.ViolationGroups.SelectMany(g => g.Violations).ToList();
        var output = new
        {
            report.Title,
            report.ProjectName,
            report.GeneratedAt,
            report.Summary,
            report.DetailedContent,
            TotalViolations = violations.Count,
            SeveritySummary = new
            {
                Critical = violations.Count(v => v.Severity == SeverityLevel.Critical),
                High = violations.Count(v => v.Severity == SeverityLevel.Error),
                Medium = violations.Count(v => v.Severity == SeverityLevel.Warning),
                Low = violations.Count(v => v.Severity == SeverityLevel.Info)
            },
            ViolationsByRule = violations
                .GroupBy(v => v.RuleName)
                .Select(g => new
                {
                    Rule = g.Key,
                    Count = g.Count(),
                    Severity = g.Max(v => v.Severity).ToString()
                })
                .ToList()
        };

        return JsonSerialize(output);
    }

    /// <summary>
    /// Manually serializes an object to JSON.
    /// </summary>
    private static string JsonSerialize(object? obj)
    {
        if (obj is null)
            return "null";

        var type = obj.GetType();

        if (type == typeof(string))
            return JsonEscape((string)obj);

        if (type == typeof(int) || type == typeof(long) || type == typeof(double) || type == typeof(float))
            return obj.ToString()!;

        if (type == typeof(bool))
            return (bool)obj ? "true" : "false";

        if (type == typeof(DateTime))
            return JsonEscape(((DateTime)obj).ToString("O"));

        if (type.IsEnum)
            return JsonEscape(obj.ToString() ?? string.Empty);

        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            var items = new List<string>();
            foreach (var item in (IEnumerable)obj)
                items.Add(JsonSerialize(item));
            return "[" + string.Join(",", items) + "]";
        }

        var pairs = new List<string>();
        foreach (var prop in type.GetProperties())
        {
            try
            {
                pairs.Add($"{JsonEscape(prop.Name)}:{JsonSerialize(prop.GetValue(obj))}");
            }
            catch
            {
            }
        }

        return "{" + string.Join(",", pairs) + "}";
    }

    /// <summary>
    /// Escapes a string for JSON output.
    /// </summary>
    private static string JsonEscape(string text)
    {
        var sb = new StringBuilder("\"");

        foreach (var c in text)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default: sb.Append(c); break;
            }
        }

        sb.Append('"');
        return sb.ToString();
    }
}
