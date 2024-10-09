// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

using RoslynGuardAnalyzer.Core;
namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Represents a formatted report of analysis violations and statistics.
/// </summary>
public sealed class ViolationReport
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string ProjectName { get; set; }
    public DateTime GeneratedAt { get; set; }
    public List<ViolationGroup> ViolationGroups { get; set; }
    public ReportStatistics Statistics { get; set; }
    public string Summary { get; set; }
    public string DetailedContent { get; set; }
    public ReportFormat Format { get; set; }

    public ViolationReport()
    {
        Id = Guid.NewGuid().ToString();
        Title = string.Empty;
        ProjectName = string.Empty;
        GeneratedAt = DateTime.UtcNow;
        ViolationGroups = new List<ViolationGroup>();
        Statistics = new ReportStatistics();
        Summary = string.Empty;
        DetailedContent = string.Empty;
        Format = ReportFormat.Text;
    }

    public ViolationReport(string title, string projectName)
        : this()
    {
        Title = title;
        ProjectName = projectName;
    }

    /// <summary>
    /// Adds a violation group to the report.
    /// </summary>
    public void AddViolationGroup(ViolationGroup group)
    {
        if (group != null)
        {
            ViolationGroups.Add(group);
            UpdateStatistics();
        }
    }

    /// <summary>
    /// Gets violations grouped by severity.
    /// </summary>
    public Dictionary<SeverityLevel, List<RuleViolation>> GetViolationsBySeverity()
    {
        var groups = new Dictionary<SeverityLevel, List<RuleViolation>>();

        foreach (var group in ViolationGroups)
        {
            foreach (var violation in group.Violations)
            {
                if (!groups.ContainsKey(violation.Severity))
                    groups[violation.Severity] = new List<RuleViolation>();

                groups[violation.Severity].Add(violation);
            }
        }

        return groups;
    }

    /// <summary>
    /// Gets the total violation count.
    /// </summary>
    public int GetTotalViolationCount()
    {
        return ViolationGroups.Sum(g => g.Violations.Count);
    }

    /// <summary>
    /// Gets violations from a specific file.
    /// </summary>
    public List<RuleViolation> GetViolationsFromFile(string filePath)
    {
        var violations = new List<RuleViolation>();

        foreach (var group in ViolationGroups)
        {
            violations.AddRange(
                group.Violations.Where(v =>
                    v.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)));
        }

        return violations;
    }

    /// <summary>
    /// Updates report statistics based on contained violations.
    /// </summary>
    private void UpdateStatistics()
    {
        var allViolations = ViolationGroups.SelectMany(g => g.Violations).ToList();

        Statistics.TotalViolations = allViolations.Count;
        Statistics.CriticalCount = allViolations.Count(v => v.Severity == SeverityLevel.Critical);
        Statistics.ErrorCount = allViolations.Count(v => v.Severity == SeverityLevel.Error);
        Statistics.WarningCount = allViolations.Count(v => v.Severity == SeverityLevel.Warning);
        Statistics.InfoCount = allViolations.Count(v => v.Severity == SeverityLevel.Info);
        Statistics.AffectedFileCount = allViolations.Select(v => v.FilePath).Distinct().Count();
    }

    /// <summary>
    /// Generates a summary text for the report.
    /// </summary>
    public string GenerateSummary()
    {
        var summary = new System.Text.StringBuilder();
        summary.AppendLine($"Analysis Report: {Title}");
        summary.AppendLine($"Project: {ProjectName}");
        summary.AppendLine($"Generated: {GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        summary.AppendLine();
        summary.AppendLine($"Total Violations: {GetTotalViolationCount()}");
        summary.AppendLine($"Critical: {Statistics.CriticalCount}");
        summary.AppendLine($"Errors: {Statistics.ErrorCount}");
        summary.AppendLine($"Warnings: {Statistics.WarningCount}");
        summary.AppendLine($"Affected Files: {Statistics.AffectedFileCount}");

        return summary.ToString();
    }
}

/// <summary>
/// Groups violations by a specific category or rule.
/// </summary>
public sealed class ViolationGroup
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<RuleViolation> Violations { get; set; }
    public RuleCategory? Category { get; set; }

    public ViolationGroup()
    {
        Id = Guid.NewGuid().ToString();
        Name = string.Empty;
        Description = string.Empty;
        Violations = new List<RuleViolation>();
    }

    public ViolationGroup(string name, string description)
        : this()
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Adds a violation to the group.
    /// </summary>
    public void AddViolation(RuleViolation violation)
    {
        if (violation != null && !Violations.Any(v => v.Id == violation.Id))
        {
            Violations.Add(violation);
        }
    }

    /// <summary>
    /// Gets the most severe violation in the group.
    /// </summary>
    public RuleViolation? GetMostSevere()
    {
        return Violations.OrderByDescending(v => v.Severity).FirstOrDefault();
    }

    /// <summary>
    /// Gets violations count by severity.
    /// </summary>
    public Dictionary<SeverityLevel, int> GetSeverityDistribution()
    {
        return Violations
            .GroupBy(v => v.Severity)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}

/// <summary>
/// Contains statistical information about a violation report.
/// </summary>
public sealed class ReportStatistics
{
    public int TotalViolations { get; set; }
    public int CriticalCount { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
    public int AffectedFileCount { get; set; }
    public int AffectedNamespaceCount { get; set; }
    public int RuleCount { get; set; }
    public double AverageViolationsPerFile { get; set; }

    /// <summary>
    /// Calculates if the report indicates a passing analysis.
    /// </summary>
    public bool IsPassing()
    {
        return CriticalCount == 0 && ErrorCount == 0;
    }

    /// <summary>
    /// Calculates the severity score (0-100).
    /// </summary>
    public int CalculateSeverityScore()
    {
        const int criticalWeight = 10;
        const int errorWeight = 5;
        const int warningWeight = 1;

        var score = 100 - (CriticalCount * criticalWeight + ErrorCount * errorWeight + WarningCount * warningWeight);
        return Math.Max(0, Math.Min(100, score));
    }
}
