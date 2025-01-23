#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <param name="repository">The repository instance.</param>
    /// <param name="projectPath">Path to the project. Must be non-null and non-empty.</param>
    /// <returns>The latest successful analysis result or <see langword="null"/> if none exists.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="projectPath"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static AnalysisResult? GetLatestSuccessfulForProject(this AnalysisResultRepository repository, string projectPath)
    {
        ArgumentNullException.ThrowIfNull(repository);

        ArgumentException.ThrowIfNullOrEmpty(projectPath, nameof(projectPath));

        return repository.GetByProject(projectPath)
            .Where(r => r.AnalysisSucceeded)
            .OrderByDescending(r => r.AnalysisEndTime)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets analysis results for projects that have violations exceeding a specified threshold.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="violationThreshold">Minimum violation count threshold. Must be non-negative.</param>
    /// <returns>List of analysis results with violations above threshold, ordered by violation count descending.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="violationThreshold"/> is negative.</exception>
    public static IReadOnlyList<AnalysisResult> GetWithCriticalViolations(this AnalysisResultRepository repository, int violationThreshold)
    {
        ArgumentNullException.ThrowIfNull(repository);

        ArgumentOutOfRangeException.ThrowIfNegative(violationThreshold);

        return repository.GetWithViolationCountGreaterThan(violationThreshold)
            .OrderByDescending(r => r.ViolationCount)
            .ToList();
    }

    /// <summary>
    /// Gets analysis results grouped by project with their latest analysis status.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <returns>Dictionary mapping project paths to their latest analysis results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    public static IReadOnlyDictionary<string, AnalysisResult?> GetLatestAnalysesByProject(this AnalysisResultRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

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
    /// <param name="repository">The repository instance.</param>
    /// <param name="minCategories">Minimum number of categories with violations. Must be at least 1.</param>
    /// <returns>List of analysis results with violations in multiple categories.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="minCategories"/> is less than 1.</exception>
    public static IReadOnlyList<AnalysisResult> GetWithMultipleViolationCategories(this AnalysisResultRepository repository, int minCategories = 2)
    {
        ArgumentNullException.ThrowIfNull(repository);

        ArgumentOutOfRangeException.ThrowIfNegative(minCategories);

        return repository.GetAll()
            .Where(r => r.ViolationsByCategory.Count >= minCategories)
            .OrderByDescending(r => r.ViolationsByCategory.Count)
            .ThenByDescending(r => r.ViolationCount)
            .ToList();
    }
}