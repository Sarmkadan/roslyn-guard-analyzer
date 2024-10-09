// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Core;

/// <summary>
/// Extension methods for domain models and common types.
/// </summary>
public static class DomainExtensions
{
    /// <summary>
    /// Gets a human-readable display name for a severity level.
    /// </summary>
    public static string GetDisplayName(this SeverityLevel severity)
    {
        return severity switch
        {
            SeverityLevel.Critical => "🔴 Critical",
            SeverityLevel.Error => "❌ Error",
            SeverityLevel.Warning => "⚠️ Warning",
            SeverityLevel.Info => "ℹ️ Info",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets a color code for console output.
    /// </summary>
    public static ConsoleColor GetConsoleColor(this SeverityLevel severity)
    {
        return severity switch
        {
            SeverityLevel.Critical => ConsoleColor.Magenta,
            SeverityLevel.Error => ConsoleColor.Red,
            SeverityLevel.Warning => ConsoleColor.Yellow,
            SeverityLevel.Info => ConsoleColor.Cyan,
            _ => ConsoleColor.Gray
        };
    }

    /// <summary>
    /// Determines if a violation should block analysis success.
    /// </summary>
    public static bool IsBlockingViolation(this RuleViolation violation)
    {
        return violation.Severity == SeverityLevel.Critical
            || violation.Severity == SeverityLevel.Error;
    }

    /// <summary>
    /// Gets violations grouped by file with line-based sorting.
    /// </summary>
    public static Dictionary<string, List<RuleViolation>> GroupByFileAndSort(this IEnumerable<RuleViolation> violations)
    {
        return violations
            .GroupBy(v => v.FilePath)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(v => v.LineNumber).ToList());
    }

    /// <summary>
    /// Filters violations by severity threshold and above.
    /// </summary>
    public static List<RuleViolation> FilterBySeverity(
        this IEnumerable<RuleViolation> violations,
        SeverityLevel minimumSeverity)
    {
        return violations.Where(v => v.Severity >= minimumSeverity).ToList();
    }

    /// <summary>
    /// Creates a summary of violations by category.
    /// </summary>
    public static Dictionary<RuleCategory, int> SummarizeByCategory(this IEnumerable<RuleViolation> violations)
    {
        return violations
            .GroupBy(v => v.Category)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Calculates the percentage of violations by severity.
    /// </summary>
    public static Dictionary<SeverityLevel, double> CalculateSeverityPercentages(this IEnumerable<RuleViolation> violations)
    {
        var violationList = violations.ToList();
        var total = violationList.Count;

        if (total == 0)
            return new Dictionary<SeverityLevel, double>();

        return violationList
            .GroupBy(v => v.Severity)
            .ToDictionary(g => g.Key, g => (g.Count() / (double)total) * 100);
    }

    /// <summary>
    /// Gets the most common rule violated.
    /// </summary>
    public static string? GetMostCommonRule(this IEnumerable<RuleViolation> violations)
    {
        return violations
            .GroupBy(v => v.RuleId)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()
            ?.Key;
    }

    /// <summary>
    /// Gets the file with the most violations.
    /// </summary>
    public static string? GetMostProblematicFile(this IEnumerable<RuleViolation> violations)
    {
        return violations
            .GroupBy(v => v.FilePath)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()
            ?.Key;
    }

    /// <summary>
    /// Exports violations to a formatted string.
    /// </summary>
    public static string ExportToText(this IEnumerable<RuleViolation> violations, string title = "Violations Export")
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"═══ {title} ═══");
        sb.AppendLine($"Exported: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        foreach (var violation in violations)
        {
            sb.AppendLine($"{violation.GetFullDescription()}");
            if (!string.IsNullOrEmpty(violation.CodeSnippet))
            {
                sb.AppendLine($"  Code: {violation.CodeSnippet}");
            }
        }

        return sb.ToString();
    }
}

/// <summary>
/// Extension methods for AnalysisResult.
/// </summary>
public static class AnalysisResultExtensions
{
    /// <summary>
    /// Gets violations above a severity threshold.
    /// </summary>
    public static List<RuleViolation> GetViolationsAboveSeverity(
        this AnalysisResult result,
        SeverityLevel minimumSeverity)
    {
        return result.Violations.FilterBySeverity(minimumSeverity);
    }

    /// <summary>
    /// Gets all violations for a specific rule.
    /// </summary>
    public static List<RuleViolation> GetViolationsForRule(this AnalysisResult result, string ruleId)
    {
        return result.Violations.Where(v => v.RuleId == ruleId).ToList();
    }

    /// <summary>
    /// Gets a summary of the analysis in a string format.
    /// </summary>
    public static string GetSummary(this AnalysisResult result)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Project: {result.ProjectName}");
        sb.AppendLine($"Status: {(result.AnalysisSucceeded ? "✓ Success" : "✗ Failed")}");
        sb.AppendLine($"Duration: {result.GetDuration().TotalSeconds:F2}s");
        sb.AppendLine($"Total Violations: {result.ViolationCount}");
        sb.AppendLine($"Success Rate: {result.GetSuccessPercentage():F1}%");

        return sb.ToString();
    }

    /// <summary>
    /// Determines if the analysis results are acceptable.
    /// </summary>
    public static bool IsAcceptable(this AnalysisResult result, int maxAllowedErrors = 0, int maxAllowedWarnings = 10)
    {
        var errors = result.GetViolationCountBySeverity(SeverityLevel.Error)
            + result.GetViolationCountBySeverity(SeverityLevel.Critical);

        var warnings = result.GetViolationCountBySeverity(SeverityLevel.Warning);

        return errors <= maxAllowedErrors && warnings <= maxAllowedWarnings;
    }
}

/// <summary>
/// Extension methods for collections.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Safe enumeration with index.
    /// </summary>
    public static IEnumerable<(int Index, T Item)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (index, item));
    }

    /// <summary>
    /// Batches items into groups.
    /// </summary>
    public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        var batch = new List<T>(batchSize);

        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count == batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }

        if (batch.Count > 0)
            yield return batch;
    }

    /// <summary>
    /// Checks if a collection is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    /// <summary>
    /// Gets a distinct collection based on a key selector.
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        var seen = new HashSet<TKey>();

        foreach (var item in source)
        {
            var key = keySelector(item);
            if (seen.Add(key))
                yield return item;
        }
    }
}

/// <summary>
/// Extension methods for strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Truncates a string to a maximum length.
    /// </summary>
    public static string Truncate(this string text, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength - suffix.Length) + suffix;
    }

    /// <summary>
    /// Determines if a string is a valid file path.
    /// </summary>
    public static bool IsValidFilePath(this string path)
    {
        try
        {
            _ = Path.GetFullPath(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Converts a string to a safe identifier (removes invalid characters).
    /// </summary>
    public static string ToSafeIdentifier(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "Unknown";

        var safeName = System.Text.RegularExpressions.Regex.Replace(text, @"[^\w]", "_");
        return char.IsLetter(safeName[0]) ? safeName : "_" + safeName;
    }

    /// <summary>
    /// Gets the relative path from one path to another.
    /// </summary>
    public static string GetRelativePath(this string fullPath, string basePath)
    {
        if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(basePath))
            return fullPath;

        try
        {
            return Path.GetRelativePath(basePath, fullPath);
        }
        catch
        {
            return fullPath;
        }
    }
}
