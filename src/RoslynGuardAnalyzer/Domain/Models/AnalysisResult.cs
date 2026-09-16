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
    /// <summary>
    /// Unique identifier for the analysis result.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Name of the analyzed project.
    /// </summary>
    public string ProjectName { get; set; }

    /// <summary>
    /// File system path to the analyzed project.
    /// </summary>
    public string ProjectPath { get; set; }

    /// <summary>
    /// Collection of rule violations found during analysis.
    /// </summary>
    public List<RuleViolation> Violations { get; set; }

    /// <summary>
    /// Collection of code elements that were analyzed.
    /// </summary>
    public List<CodeElement> AnalyzedElements { get; set; }

    /// <summary>
    /// Timestamp when the analysis started.
    /// </summary>
    public DateTime AnalysisStartTime { get; set; }

    /// <summary>
    /// Timestamp when the analysis ended.
    /// </summary>
    public DateTime AnalysisEndTime { get; set; }

    /// <summary>
    /// Indicates whether the analysis completed successfully.
    /// </summary>
    public bool AnalysisSucceeded { get; set; }

    /// <summary>
    /// Error message if the analysis failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Total number of files analyzed.
    /// </summary>
    public int TotalFilesAnalyzed { get; set; }

    /// <summary>
    /// Total number of code elements analyzed.
    /// </summary>
    public int TotalElementsAnalyzed { get; set; }

    /// <summary>
    /// Count of violations grouped by category.
    /// </summary>
    public Dictionary<string, int> ViolationsByCategory { get; set; }

    /// <summary>
    /// Count of violations grouped by severity level.
    /// </summary>
    public Dictionary<string, int> ViolationsBySeverity { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisResult"/> class with default values.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisResult"/> class with the specified project name and path.
    /// </summary>
    /// <param name="projectName">The name of the project being analyzed.</param>
    /// <param name="projectPath">The file system path to the project being analyzed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectName"/> or <paramref name="projectPath"/> is null.</exception>
    public AnalysisResult(string projectName, string projectPath)
        : this()
    {
        if (projectName is null)
            throw new ArgumentNullException(nameof(projectName));
        if (projectPath is null)
            throw new ArgumentNullException(nameof(projectPath));

        ProjectName = projectName;
        ProjectPath = projectPath;
    }

    /// <summary>
    /// Adds a violation to the result and updates statistics.
    /// </summary>
    /// <param name="violation">The violation to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="violation"/> is null.</exception>
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
    /// <param name="violations">The violations to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="violations"/> is null.</exception>
    public void AddViolations(IEnumerable<RuleViolation> violations)
    {
        if (violations is null)
            throw new ArgumentNullException(nameof(violations));

        foreach (var violation in violations)
        {
            AddViolation(violation);
        }
    }

    /// <summary>
    /// Adds an analyzed code element to the result.
    /// </summary>
    /// <param name="element">The code element that was analyzed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> is null.</exception>
    public void AddAnalyzedElement(CodeElement element)
    {
        if (element is null)
            throw new ArgumentNullException(nameof(element));

        AnalyzedElements.Add(element);
        TotalElementsAnalyzed = AnalyzedElements.Count;
    }

    /// <summary>
    /// Gets the total count of violations.
    /// </summary>
    public int ViolationCount => Violations.Count;

    /// <summary>
    /// Gets the count of violations with a specific severity.
    /// </summary>
    /// <param name="severity">The severity level to count.</param>
    /// <returns>The number of violations with the specified severity.</returns>
    public int GetViolationCountBySeverity(SeverityLevel severity)
    {
        return Violations.Count(v => v.Severity == severity);
    }

    /// <summary>
    /// Gets violations grouped by rule ID.
    /// </summary>
    /// <returns>A dictionary mapping rule IDs to their violations.</returns>
    public Dictionary<string, List<RuleViolation>> GetViolationsByRule()
    {
        return Violations
            .GroupBy(v => v.RuleId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Gets critical violations that must be fixed.
    /// </summary>
    /// <returns>A list of violations with Critical or Error severity.</returns>
    public List<RuleViolation> GetCriticalViolations()
    {
        return Violations
            .Where(v => v.Severity == SeverityLevel.Critical || v.Severity == SeverityLevel.Error)
            .ToList();
    }

    /// <summary>
    /// Gets analysis duration.
    /// </summary>
    /// <returns>The time span between analysis start and end times.</returns>
    public TimeSpan GetDuration()
    {
        return AnalysisEndTime - AnalysisStartTime;
    }

    /// <summary>
    /// Gets success percentage (violations that were not found relative to total rules checked).
    /// </summary>
    /// <returns>The success percentage as a value between 0 and 100.</returns>
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
    /// <param name="errorMessage">The error message describing the failure.</param>
    public void MarkAsFailed(string errorMessage)
    {
        AnalysisSucceeded = false;
        ErrorMessage = errorMessage;
        AnalysisEndTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns a string representation of the analysis result.
    /// </summary>
    /// <returns>A string containing the project name and violation count.</returns>
    public override string ToString() => $"{ProjectName}: {ViolationCount} violations";

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
