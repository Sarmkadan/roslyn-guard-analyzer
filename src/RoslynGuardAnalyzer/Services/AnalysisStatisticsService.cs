#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Utilities;

namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Calculates detailed statistics about analysis results and violations.
/// Provides aggregated metrics, trends, and summary information.
/// </summary>
public sealed class AnalysisStatisticsService
{
    /// <summary>
    /// Comprehensive statistics about violations.
    /// </summary>
    public sealed class ViolationStatistics
    {
        public int TotalCount { get; set; }
        public int CriticalCount { get; set; }
        public int HighCount { get; set; }
        public int MediumCount { get; set; }
        public int LowCount { get; set; }
        public Dictionary<string, int> ByRule { get; } = [];
        public Dictionary<string, int> ByFile { get; } = [];
        public Dictionary<string, int> BySeverity { get; } = [];
        public int AffectedFiles { get; set; }
        public int AffectedRules { get; set; }
    }

    /// <summary>
    /// Calculates statistics from a list of violations.
    /// </summary>
    public static ViolationStatistics CalculateStatistics(IEnumerable<RuleViolation> violations)
    {
        var violationList = violations?.ToList() ?? [];

        var stats = new ViolationStatistics
        {
            TotalCount = violationList.Count,
            CriticalCount = violationList.Count(v => v.Severity == "Critical"),
            HighCount = violationList.Count(v => v.Severity == "High"),
            MediumCount = violationList.Count(v => v.Severity == "Medium"),
            LowCount = violationList.Count(v => v.Severity == "Low"),
            AffectedFiles = violationList.Select(v => v.FilePath).Distinct().Count(),
            AffectedRules = violationList.Select(v => v.RuleName).Distinct().Count()
        };

        // By rule
        foreach (var group in violationList.GroupBy(v => v.RuleName))
            stats.ByRule[group.Key] = group.Count();

        // By file
        foreach (var group in violationList.GroupBy(v => v.FilePath))
            stats.ByFile[group.Key] = group.Count();

        // By severity
        foreach (var group in violationList.GroupBy(v => v.Severity))
            stats.BySeverity[group.Key] = group.Count();

        return stats;
    }

    /// <summary>
    /// Calculates statistics from an analysis result.
    /// </summary>
    public static ViolationStatistics CalculateStatistics(AnalysisResult result)
    {
        return CalculateStatistics(result?.Violations);
    }

    /// <summary>
    /// Gets the top N rules by violation count.
    /// </summary>
    public static List<(string Rule, int Count)> GetTopRulesByViolations(
        IEnumerable<RuleViolation> violations,
        int count = 10)
    {
        return violations
            .GroupBy(v => v.RuleName)
            .Select(g => (g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Gets the top N files by violation count.
    /// </summary>
    public static List<(string File, int Count)> GetTopFilesByViolations(
        IEnumerable<RuleViolation> violations,
        int count = 10)
    {
        return violations
            .GroupBy(v => System.IO.Path.GetFileName(v.FilePath))
            .Select(g => (g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Calculates severity distribution as percentages.
    /// </summary>
    public static Dictionary<string, double> GetSeverityDistribution(IEnumerable<RuleViolation> violations)
    {
        var stats = CalculateStatistics(violations);

        var result = new Dictionary<string, double>();
        if (stats.TotalCount == 0)
            return result;

        result["Critical"] = (stats.CriticalCount * 100.0) / stats.TotalCount;
        result["High"] = (stats.HighCount * 100.0) / stats.TotalCount;
        result["Medium"] = (stats.MediumCount * 100.0) / stats.TotalCount;
        result["Low"] = (stats.LowCount * 100.0) / stats.TotalCount;

        return result;
    }

    /// <summary>
    /// Generates a summary report of statistics.
    /// </summary>
    public static string GenerateSummaryReport(ViolationStatistics stats)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("=== Violation Statistics ===");
        sb.AppendLine($"Total Violations: {stats.TotalCount}");
        sb.AppendLine();

        sb.AppendLine("By Severity:");
        sb.AppendLine($"  Critical: {stats.CriticalCount} ({PercentOf(stats.CriticalCount, stats.TotalCount)}%)");
        sb.AppendLine($"  High:     {stats.HighCount} ({PercentOf(stats.HighCount, stats.TotalCount)}%)");
        sb.AppendLine($"  Medium:   {stats.MediumCount} ({PercentOf(stats.MediumCount, stats.TotalCount)}%)");
        sb.AppendLine($"  Low:      {stats.LowCount} ({PercentOf(stats.LowCount, stats.TotalCount)}%)");
        sb.AppendLine();

        sb.AppendLine($"Affected Rules: {stats.AffectedRules}");
        sb.AppendLine($"Affected Files: {stats.AffectedFiles}");
        sb.AppendLine();

        if (stats.ByRule.Count > 0)
        {
            sb.AppendLine("Top Rules:");
            foreach (var (rule, count) in stats.ByRule.OrderByDescending(x => x.Value).Take(5))
            {
                sb.AppendLine($"  - {rule}: {count} violations");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Calculates the risk score (0-100) based on violations.
    /// Takes severity into account with heavier weighting for critical issues.
    /// </summary>
    public static int CalculateRiskScore(ViolationStatistics stats)
    {
        // Weighted calculation: Critical=10, High=5, Medium=2, Low=1
        var score = (stats.CriticalCount * 10) + (stats.HighCount * 5) + (stats.MediumCount * 2) + stats.LowCount;

        // Cap at 100
        return Math.Min(100, score / 10);
    }

    /// <summary>
    /// Gets a health assessment based on violation statistics.
    /// </summary>
    public static string GetHealthAssessment(ViolationStatistics stats)
    {
        if (stats.TotalCount == 0)
            return "✓ Excellent - No violations found";

        if (stats.CriticalCount > 0)
            return "✗ Critical - Immediate action required";

        if (stats.HighCount > 5)
            return "⚠ Poor - Multiple high-severity violations";

        if (stats.HighCount > 0)
            return "⚠ Fair - Some high-severity violations";

        if (stats.MediumCount > 10)
            return "◐ Good - Several medium-severity violations";

        return "○ Acceptable - Mostly low-severity violations";
    }

    /// <summary>
    /// Helper to calculate percentage.
    /// </summary>
    private static double PercentOf(int count, int total)
    {
        return total == 0 ? 0 : Math.Round((count * 100.0) / total, 1);
    }
}
