#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Aggregates multiple analysis results into a unified report.
/// Used when analyzing multiple projects or files to produce a consolidated view.
/// </summary>
public sealed class ResultAggregator
{
    private readonly List<AnalysisResult> _results = [];

    /// <summary>
    /// Adds an analysis result to the aggregation.
    /// </summary>
    public void Add(AnalysisResult result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        _results.Add(result);
    }

    /// <summary>
    /// Adds multiple results at once.
    /// </summary>
    public void AddRange(IEnumerable<AnalysisResult> results)
    {
        foreach (var result in results)
            Add(result);
    }

    /// <summary>
    /// Gets the total number of violations across all results.
    /// </summary>
    public int GetTotalViolations() => _results.Sum(r => r.ViolationCount);

    /// <summary>
    /// Gets all violations from all results.
    /// </summary>
    public IEnumerable<RuleViolation> GetAllViolations() =>
        _results.SelectMany(r => r.Violations);

    /// <summary>
    /// Gets violations grouped by rule name.
    /// </summary>
    public Dictionary<string, List<RuleViolation>> GetViolationsByRule()
    {
        return GetAllViolations()
            .GroupBy(v => v.RuleName)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Gets violations grouped by severity.
    /// </summary>
    public Dictionary<string, List<RuleViolation>> GetViolationsBySeverity()
    {
        return GetAllViolations()
            .GroupBy(v => v.Severity.ToString())
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Gets violations grouped by file.
    /// </summary>
    public Dictionary<string, List<RuleViolation>> GetViolationsByFile()
    {
        return GetAllViolations()
            .GroupBy(v => v.FilePath)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Gets the total number of files analyzed.
    /// </summary>
    public int GetTotalFilesAnalyzed() => _results.Sum(r => r.TotalFilesAnalyzed);

    /// <summary>
    /// Gets the total number of code elements analyzed.
    /// </summary>
    public int GetTotalElementsAnalyzed() => _results.Sum(r => r.TotalElementsAnalyzed);

    /// <summary>
    /// Generates a summary report across all results.
    /// </summary>
    public ViolationReport GenerateSummaryReport()
    {
        var allViolations = GetAllViolations().ToList();
        var report = new ViolationReport($"Aggregated Analysis Report ({_results.Count} projects)", "Multiple Projects");

        var group = new Domain.Models.ViolationGroup("All Violations", "Combined violations from all projects");
        foreach (var violation in allViolations)
            group.AddViolation(violation);

        report.AddViolationGroup(group);

        return report;
    }

    /// <summary>
    /// Gets statistics about all results.
    /// </summary>
    public Dictionary<string, object> GetStatistics()
    {
        var allViolations = GetAllViolations().ToList();

        return new Dictionary<string, object>
        {
            ["TotalProjects"] = _results.Count,
            ["TotalViolations"] = allViolations.Count,
            ["TotalFilesAnalyzed"] = GetTotalFilesAnalyzed(),
            ["TotalElementsAnalyzed"] = GetTotalElementsAnalyzed(),
            ["AverageViolationsPerProject"] = _results.Count > 0 ? allViolations.Count / (double)_results.Count : 0,
            ["CriticalCount"] = allViolations.Count(v => v.Severity.ToString() == "Critical"),
            ["ErrorCount"] = allViolations.Count(v => v.Severity.ToString() == "Error"),
            ["WarningCount"] = allViolations.Count(v => v.Severity.ToString() == "Warning"),
            ["InfoCount"] = allViolations.Count(v => v.Severity.ToString() == "Info"),
            ["FailedAnalyses"] = _results.Count(r => !r.AnalysisSucceeded),
            ["SuccessfulAnalyses"] = _results.Count(r => r.AnalysisSucceeded)
        };
    }

    /// <summary>
    /// Gets the count of results.
    /// </summary>
    public int Count => _results.Count;

    /// <summary>
    /// Clears all results.
    /// </summary>
    public void Clear() => _results.Clear();

    /// <summary>
    /// Gets a specific result by index.
    /// </summary>
    public AnalysisResult? GetResult(int index) => index >= 0 && index < _results.Count ? _results[index] : null;

    /// <summary>
    /// Gets all results.
    /// </summary>
    public IReadOnlyList<AnalysisResult> GetAllResults() => _results.AsReadOnly();
}
