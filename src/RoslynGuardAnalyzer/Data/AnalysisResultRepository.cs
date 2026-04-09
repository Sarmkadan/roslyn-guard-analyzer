#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Data;

/// <summary>
/// Repository for managing persistence of analysis results.
/// </summary>
public sealed class AnalysisResultRepository : RepositoryBase<AnalysisResult>
{
    private const string ResultsDirectory = "results";
    private readonly string _dataDirectory;

    public AnalysisResultRepository(string? dataDirectory = null)
    {
        _dataDirectory = dataDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RoslynGuardAnalyzer",
            "Data");

        EnsureDirectoryExists();
    }

    /// <summary>
    /// Gets results for a specific project.
    /// </summary>
    public IReadOnlyList<AnalysisResult> GetByProject(string projectPath)
    {
        return Find(r => r.ProjectPath.Equals(projectPath, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets results analyzed after a specific date.
    /// </summary>
    public IReadOnlyList<AnalysisResult> GetAnalyzedAfter(DateTime date)
    {
        return Find(r => r.AnalysisEndTime > date);
    }

    /// <summary>
    /// Gets failed analysis results.
    /// </summary>
    public IReadOnlyList<AnalysisResult> GetFailedAnalyses()
    {
        return Find(r => !r.AnalysisSucceeded);
    }

    /// <summary>
    /// Gets successful analysis results.
    /// </summary>
    public IReadOnlyList<AnalysisResult> GetSuccessfulAnalyses()
    {
        return Find(r => r.AnalysisSucceeded);
    }

    /// <summary>
    /// Gets results with violations in a specific category.
    /// </summary>
    public IReadOnlyList<AnalysisResult> GetWithViolationsInCategory(string category)
    {
        return Find(r => r.ViolationsByCategory.ContainsKey(category) && r.ViolationsByCategory[category] > 0);
    }

    /// <summary>
    /// Gets the latest result for a project.
    /// </summary>
    public AnalysisResult? GetLatestForProject(string projectPath)
    {
        return Find(r => r.ProjectPath.Equals(projectPath, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.AnalysisEndTime)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets results with more than a specified number of violations.
    /// </summary>
    public IReadOnlyList<AnalysisResult> GetWithViolationCountGreaterThan(int violationCount)
    {
        return Find(r => r.ViolationCount > violationCount);
    }

    /// <summary>
    /// Saves an analysis result to disk.
    /// </summary>
    public async Task SaveAsync(AnalysisResult result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        var fileName = $"{result.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        var filePath = Path.Combine(_dataDirectory, ResultsDirectory, fileName);

        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(
                result,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save analysis result: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads all analysis results from disk.
    /// </summary>
    public async Task LoadAllAsync()
    {
        var resultsPath = Path.Combine(_dataDirectory, ResultsDirectory);

        if (!Directory.Exists(resultsPath))
            return;

        try
        {
            var files = Directory.GetFiles(resultsPath, "*.json");

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var result = System.Text.Json.JsonSerializer.Deserialize<AnalysisResult>(json);

                    if (result is not null)
                    {
                        Add(result.Id, result);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to load result from {file}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load analysis results: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Exports results to a CSV file.
    /// </summary>
    public async Task ExportToCsvAsync(string filePath)
    {
        try
        {
            var results = GetAll();
            var csv = new System.Text.StringBuilder();

            csv.AppendLine("ProjectName,ProjectPath,TotalFiles,TotalElements,ViolationCount,Success,StartTime,EndTime,Duration");

            foreach (var result in results)
            {
                var duration = (result.AnalysisEndTime - result.AnalysisStartTime).TotalSeconds;
                csv.AppendLine(
                    $"\"{result.ProjectName}\",\"{result.ProjectPath}\",{result.TotalFilesAnalyzed}," +
                    $"{result.TotalElementsAnalyzed},{result.ViolationCount},{result.AnalysisSucceeded}," +
                    $"\"{result.AnalysisStartTime:O}\",\"{result.AnalysisEndTime:O}\",{duration}");
            }

            await File.WriteAllTextAsync(filePath, csv.ToString());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to export results: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets statistics about stored results.
    /// </summary>
    public AnalysisResultStatistics GetStatistics()
    {
        var allResults = GetAll();

        if (!allResults.Any())
        {
            return new AnalysisResultStatistics();
        }

        return new AnalysisResultStatistics
        {
            TotalAnalyses = allResults.Count,
            SuccessfulAnalyses = allResults.Count(r => r.AnalysisSucceeded),
            FailedAnalyses = allResults.Count(r => !r.AnalysisSucceeded),
            AverageViolationCount = allResults.Average(r => r.ViolationCount),
            TotalViolations = allResults.Sum(r => r.ViolationCount),
            AverageAnalysisDurationSeconds = allResults.Average(r => (r.AnalysisEndTime - r.AnalysisStartTime).TotalSeconds),
            ProjectsAnalyzed = allResults.Select(r => r.ProjectName).Distinct().Count()
        };
    }

    /// <summary>
    /// Clears old results (older than specified days).
    /// </summary>
    public async Task ClearOldResultsAsync(int daysOld)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var oldResults = Find(r => r.AnalysisEndTime < cutoffDate);

        foreach (var result in oldResults)
        {
            Remove(result.Id);

            var fileName = $"{result.Id}_*.json";
            var resultsPath = Path.Combine(_dataDirectory, ResultsDirectory);
            var files = Directory.GetFiles(resultsPath, fileName);

            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to delete old result file: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Ensures the data directory exists.
    /// </summary>
    private void EnsureDirectoryExists()
    {
        var resultsPath = Path.Combine(_dataDirectory, ResultsDirectory);

        if (!Directory.Exists(_dataDirectory))
            Directory.CreateDirectory(_dataDirectory);

        if (!Directory.Exists(resultsPath))
            Directory.CreateDirectory(resultsPath);
    }
}

/// <summary>
/// Statistics about the analysis results repository.
/// </summary>
public sealed class AnalysisResultStatistics
{
    public int TotalAnalyses { get; set; }
    public int SuccessfulAnalyses { get; set; }
    public int FailedAnalyses { get; set; }
    public double AverageViolationCount { get; set; }
    public int TotalViolations { get; set; }
    public double AverageAnalysisDurationSeconds { get; set; }
    public int ProjectsAnalyzed { get; set; }

    public double GetSuccessRate()
    {
        if (TotalAnalyses == 0)
            return 0;

        return (SuccessfulAnalyses / (double)TotalAnalyses) * 100;
    }
}
