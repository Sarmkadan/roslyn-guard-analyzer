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
            v.RuleName,
            v.Severity,
            v.Message,
            v.FilePath,
            v.LineNumber,
            v.ColumnNumber
        }).ToList();

        var output = new
        {
            result.ProjectName,
            result.ProjectPath,
            result.IsCompleted,
            result.ErrorMessage,
            result.TotalFilesAnalyzed,
            result.TotalElementsAnalyzed,
            ViolationCount = result.ViolationCount,
            Violations = violations,
            TimestampUtc = DateTime.UtcNow.ToIso8601String()
        };

        return JsonSerialize(output);
    }

    public string FormatViolations(IEnumerable<RuleViolation> violations)
    {
        var violationList = violations.Select(v => new
        {
            v.RuleName,
            v.Severity,
            v.Message,
            v.FilePath,
            v.LineNumber,
            v.ColumnNumber,
            Code = v.Code ?? "N/A"
        }).ToList();

        var output = new
        {
            Count = violationList.Count,
            Violations = violationList
        };

        return JsonSerialize(output);
    }

    public string FormatReport(ViolationReport report)
    {
        var violationsByRule = report.Violations
            .GroupBy(v => v.RuleName)
            .Select(g => new
            {
                Rule = g.Key,
                Count = g.Count(),
                Severity = g.First().Severity
            })
            .ToList();

        var output = new
        {
            report.Title,
            report.GeneratedAt,
            TotalViolations = report.Violations.Count,
            SeveritySummary = new
            {
                Critical = report.Violations.Count(v => v.Severity == "Critical"),
                High = report.Violations.Count(v => v.Severity == "High"),
                Medium = report.Violations.Count(v => v.Severity == "Medium"),
                Low = report.Violations.Count(v => v.Severity == "Low")
            },
            ViolationsByRule = violationsByRule,
            RecommendedActions = report.RecommendedActions
        };

        return JsonSerialize(output);
    }

    /// <summary>
    /// Manually serializes an object to JSON (since we don't depend on Newtonsoft.Json).
    /// Handles basic types, collections, and anonymous objects.
    /// </summary>
    private static string JsonSerialize(object obj)
    {
        if (obj == null)
            return "null";

        var type = obj.GetType();

        // Handle primitives
        if (type == typeof(string))
            return JsonEscape((string)obj);

        if (type == typeof(int) || type == typeof(long) || type == typeof(double) || type == typeof(float))
            return obj.ToString()!;

        if (type == typeof(bool))
            return ((bool)obj) ? "true" : "false";

        if (type == typeof(DateTime))
            return JsonEscape(((DateTime)obj).ToIso8601String());

        // Handle enumerables (but not strings)
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            var items = new List<string>();
            foreach (var item in (System.Collections.IEnumerable)obj)
                items.Add(JsonSerialize(item));
            return "[" + string.Join(",", items) + "]";
        }

        // Handle objects - use reflection to serialize properties
        var properties = type.GetProperties();
        var pairs = new List<string>();

        foreach (var prop in properties)
        {
            try
            {
                var value = prop.GetValue(obj);
                var serialized = JsonSerialize(value);
                pairs.Add($"{JsonEscape(prop.Name)}:{serialized}");
            }
            catch
            {
                // Skip properties that can't be serialized
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
                default:
                    if (c < 32)
                        sb.Append($"\\u{(int)c:x4}");
                    else
                        sb.Append(c);
                    break;
            }
        }

        sb.Append('"');
        return sb.ToString();
    }
}

/// <summary>
/// Extension methods for date/time formatting to ISO 8601.
/// </summary>
internal static class DateTimeExtensions
{
    public static string ToIso8601String(this DateTime dt)
    {
        return dt.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}
