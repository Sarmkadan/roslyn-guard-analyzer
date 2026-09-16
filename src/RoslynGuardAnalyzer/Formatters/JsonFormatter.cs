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
    private const string JsonFormatValue = "json";
    private const string RoundTripDateTimeFormat = "O";
    private const string NoCodeSnippetPlaceholder = "N/A";
    private const string JsonNullLiteral = "null";
    private const string JsonTrueLiteral = "true";
    private const string JsonFalseLiteral = "false";
    private const string JsonArrayOpen = "[";
    private const string JsonArrayClose = "]";
    private const string JsonObjectOpen = "{";
    private const string JsonObjectClose = "}";
    private const string JsonItemSeparator = ",";
    private const string JsonKeyValueSeparator = ":";
    private const string JsonStringQuote = "\"";
    private const string JsonEscapedQuote = "\\\"";
    private const string JsonEscapedBackslash = "\\\\";
    private const string JsonEscapedBackspace = "\\b";
    private const string JsonEscapedFormFeed = "\\f";
    private const string JsonEscapedNewline = "\\n";
    private const string JsonEscapedCarriageReturn = "\\r";
    private const string JsonEscapedTab = "\\t";

    public string Format => JsonFormatValue;

    public bool CanFormat(string format)
    {
        ArgumentException.ThrowIfNullOrEmpty(format);
        return format.Equals(Format, StringComparison.OrdinalIgnoreCase);
    }

    public string FormatResult(AnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
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
            TimestampUtc = DateTime.UtcNow.ToString(RoundTripDateTimeFormat)
        };

        return JsonSerialize(output);
    }

    public string FormatViolations(IEnumerable<RuleViolation> violations)
    {
        ArgumentNullException.ThrowIfNull(violations);
        var violationList = violations.Select(v => new
        {
            v.RuleId,
            v.RuleName,
            Severity = v.Severity.ToString(),
            v.Message,
            v.FilePath,
            v.LineNumber,
            v.ColumnNumber,
            Code = v.CodeSnippet ?? NoCodeSnippetPlaceholder
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
            return JsonNullLiteral;

        var type = obj.GetType();

        if (type == typeof(string))
            return JsonEscape((string)obj);

        if (type == typeof(int) || type == typeof(long) || type == typeof(double) || type == typeof(float))
            return obj.ToString()!;

        if (type == typeof(bool))
            return (bool)obj ? JsonTrueLiteral : JsonFalseLiteral;

        if (type == typeof(DateTime))
            return JsonEscape(((DateTime)obj).ToString(RoundTripDateTimeFormat));

        if (type.IsEnum)
            return JsonEscape(obj.ToString() ?? string.Empty);

        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            var items = new List<string>();
            foreach (var item in (IEnumerable)obj)
                items.Add(JsonSerialize(item));
            return JsonArrayOpen + string.Join(JsonItemSeparator, items) + JsonArrayClose;
        }

        var pairs = new List<string>();
        foreach (var prop in type.GetProperties())
        {
            try
            {
                pairs.Add($"{JsonEscape(prop.Name)}{JsonKeyValueSeparator}{JsonSerialize(prop.GetValue(obj))}");
            }
            catch
            {
            }
        }

        return JsonObjectOpen + string.Join(JsonItemSeparator, pairs) + JsonObjectClose;
    }

    /// <summary>
    /// Escapes a string for JSON output.
    /// </summary>
    private static string JsonEscape(string text)
    {
        var sb = new StringBuilder(JsonStringQuote);

        foreach (var c in text)
        {
            switch (c)
            {
                case '"': sb.Append(JsonEscapedQuote); break;
                case '\\': sb.Append(JsonEscapedBackslash); break;
                case '\b': sb.Append(JsonEscapedBackspace); break;
                case '\f': sb.Append(JsonEscapedFormFeed); break;
                case '\n': sb.Append(JsonEscapedNewline); break;
                case '\r': sb.Append(JsonEscapedCarriageReturn); break;
                case '\t': sb.Append(JsonEscapedTab); break;
                default: sb.Append(c); break;
            }
        }

        sb.Append(JsonStringQuote);
        return sb.ToString();
    }
}
