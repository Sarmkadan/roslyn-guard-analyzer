#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Data;

/// <summary>
/// Extension methods for <see cref="AnalysisResultRepository"/> providing enhanced querying and analysis capabilities.
/// </summary>
public static class AnalysisResultRepositoryExtensions
{
    /// <summary>
    /// Gets the most recent successful analysis for a project.
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="projectPath">Path to the project</param>
    /// <returns>The latest successful analysis result or null if none exists</returns>
    public static AnalysisResult? GetLatestSuccessfulForProject(this AnalysisResultRepository repository, string projectPath)
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (string.IsNullOrWhiteSpace(projectPath))
            throw new ArgumentException("Project path cannot be null or empty", nameof(projectPath));

        return repository.GetByProject(projectPath)
            .Where(r => r.AnalysisSucceeded)
            .OrderByDescending(r => r.AnalysisEndTime)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets analysis results for projects that have violations exceeding a specified threshold.
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="violationThreshold">Minimum violation count threshold</param>
    /// <returns>List of analysis results with violations above threshold</returns>
    public static IReadOnlyList<AnalysisResult> GetWithCriticalViolations(this AnalysisResultRepository repository, int violationThreshold)
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        return repository.GetWithViolationCountGreaterThan(violationThreshold)
            .OrderByDescending(r => r.ViolationCount)
            .ToList();
    }

    /// <summary>
    /// Gets analysis results grouped by project with their latest analysis status.
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <returns>Dictionary mapping project paths to their latest analysis results</returns>
    public static IReadOnlyDictionary<string, AnalysisResult?> GetLatestAnalysesByProject(this AnalysisResultRepository repository)
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        var allResults = repository.GetAll();
        var latestByProject = allResults
            .GroupBy(r => r.ProjectPath, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(r => r.AnalysisEndTime).FirstOrDefault(),
                StringComparer.OrdinalIgnoreCase
            );

        return latestByProject;
    }

    /// <summary>
    /// Gets analysis results that have violations in multiple categories, indicating complex issues.
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="minCategories">Minimum number of categories with violations</param>
    /// <returns>List of analysis results with violations in multiple categories</returns>
    public static IReadOnlyList<AnalysisResult> GetWithMultipleViolationCategories(this AnalysisResultRepository repository, int minCategories = 2)
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (minCategories < 1)
            throw new ArgumentOutOfRangeException(nameof(minCategories), "Minimum categories must be at least 1");

        return repository.GetAll()
            .Where(r => r.ViolationsByCategory.Count >= minCategories)
            .OrderByDescending(r => r.ViolationsByCategory.Count)
            .ThenByDescending(r => r.ViolationCount)
            .ToList();
    }
}