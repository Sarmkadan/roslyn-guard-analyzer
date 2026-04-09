#nullable enable
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
/// Contains the complete results of a code analysis execution.
/// Includes violations found, analysis statistics, and metadata.
/// </summary>
public sealed class AnalysisResult
{
    public string Id { get; set; }
    public string ProjectName { get; set; }
    public string ProjectPath { get; set; }
    public List<RuleViolation> Violations { get; set; }
    public List<CodeElement> AnalyzedElements { get; set; }
    public DateTime AnalysisStartTime { get; set; }
    public DateTime AnalysisEndTime { get; set; }
    public bool AnalysisSucceeded { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalFilesAnalyzed { get; set; }
    public int TotalElementsAnalyzed { get; set; }
    public Dictionary<string, int> ViolationsByCategory { get; set; }
    public Dictionary<string, int> ViolationsBySeverity { get; set; }

    public AnalysisResult()
    {
        Id = Guid.NewGuid().ToString();
        ProjectName = string.Empty;
        ProjectPath = string.Empty;
        Violations = new List<RuleViolation>();
        AnalyzedElements = new List<CodeElement>();
        AnalysisStartTime = DateTime.UtcNow;
        AnalysisSucceeded = true;
        ViolationsByCategory = new Dictionary<string, int>();
        ViolationsBySeverity = new Dictionary<string, int>();
    }

    public AnalysisResult(string projectName, string projectPath)
        : this()
    {
        ProjectName = projectName;
        ProjectPath = projectPath;
    }

    /// <summary>
    /// Adds a violation to the result and updates statistics.
    /// </summary>
    public void AddViolation(RuleViolation violation)
    {
        if (violation is null)
            throw new ArgumentNullException(nameof(violation));

        Violations.Add(violation);
        UpdateViolationStatistics(violation);
    }

    /// <summary>
    /// Adds multiple violations at once.
    /// </summary>
    public void AddViolations(IEnumerable<RuleViolation> violations)
    {
        foreach (var violation in violations ?? Enumerable.Empty<RuleViolation>())
        {
            AddViolation(violation);
        }
    }

    /// <summary>
    /// Adds an analyzed code element to the result.
    /// </summary>
    public void AddAnalyzedElement(CodeElement element)
    {
        if (element is not null)
        {
            AnalyzedElements.Add(element);
            TotalElementsAnalyzed = AnalyzedElements.Count;
        }
    }

    /// <summary>
    /// Gets the total count of violations.
    /// </summary>
    public int ViolationCount => Violations.Count;

    /// <summary>
    /// Gets the count of violations with a specific severity.
    /// </summary>
    public int GetViolationCountBySeverity(SeverityLevel severity)
    {
        return Violations.Count(v => v.Severity == severity);
    }

    /// <summary>
    /// Gets violations grouped by rule ID.
    /// </summary>
    public Dictionary<string, List<RuleViolation>> GetViolationsByRule()
    {
        return Violations
            .GroupBy(v => v.RuleId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Gets critical violations that must be fixed.
    /// </summary>
    public List<RuleViolation> GetCriticalViolations()
    {
        return Violations
            .Where(v => v.Severity == SeverityLevel.Critical || v.Severity == SeverityLevel.Error)
            .ToList();
    }

    /// <summary>
    /// Gets analysis duration.
    /// </summary>
    public TimeSpan GetDuration()
    {
        return AnalysisEndTime - AnalysisStartTime;
    }

    /// <summary>
    /// Gets success percentage (violations that were not found relative to total rules checked).
    /// </summary>
    public double GetSuccessPercentage()
    {
        if (TotalElementsAnalyzed == 0)
            return 100;

        var violationRatio = (double)ViolationCount / TotalElementsAnalyzed;
        return Math.Max(0, (1 - violationRatio) * 100);
    }

    /// <summary>
    /// Marks the analysis as completed and sets end time.
    /// </summary>
    public void MarkAsCompleted()
    {
        AnalysisEndTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the analysis as failed with an error message.
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        AnalysisSucceeded = false;
        ErrorMessage = errorMessage;
        AnalysisEndTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates internal statistics after adding a violation.
    /// </summary>
    private void UpdateViolationStatistics(RuleViolation violation)
    {
        var categoryKey = violation.Category.ToString();
        if (!ViolationsByCategory.ContainsKey(categoryKey))
            ViolationsByCategory[categoryKey] = 0;
        ViolationsByCategory[categoryKey]++;

        var severityKey = violation.Severity.ToString();
        if (!ViolationsBySeverity.ContainsKey(severityKey))
            ViolationsBySeverity[severityKey] = 0;
        ViolationsBySeverity[severityKey]++;
    }

    /// <summary>
    /// Gets violations in a file.
    /// </summary>
    public List<RuleViolation> GetViolationsInFile(string filePath)
    {
        return Violations
            .Where(v => v.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
