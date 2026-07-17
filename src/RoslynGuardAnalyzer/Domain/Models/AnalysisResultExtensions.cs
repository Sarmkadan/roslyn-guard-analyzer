using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Extension methods for <see cref="AnalysisResult"/>.
/// </summary>
public static class AnalysisResultExtensions
{
    /// <summary>
    /// Gets a summary of the analysis result.
    /// </summary>
    /// <param name="result">The analysis result.</param>
    /// <returns>A summary of the analysis result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static string GetSummary(this AnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"Analysis of {result.ProjectName} ({result.ProjectPath}): {result.TotalFilesAnalyzed} files, {result.TotalElementsAnalyzed} elements, {(result.AnalysisSucceeded ? "succeeded" : "failed")}");
    }

    /// <summary>
    /// Gets the total number of violations by severity.
    /// </summary>
    /// <param name="result">The analysis result.</param>
    /// <returns>A dictionary containing the total number of violations by severity.
    /// Returns an empty dictionary if <see cref="AnalysisResult.ViolationsBySeverity"/> is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static IReadOnlyDictionary<string, int> GetTotalViolationsBySeverity(this AnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.ViolationsBySeverity ?? new Dictionary<string, int>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets the elapsed time of the analysis.
    /// </summary>
    /// <param name="result">The analysis result.</param>
    /// <returns>The elapsed time of the analysis.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="result.AnalysisStartTime"/> is after <paramref name="result.AnalysisEndTime"/>.</exception>
    public static TimeSpan GetElapsedTime(this AnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var elapsed = result.AnalysisEndTime - result.AnalysisStartTime;
        if (elapsed < TimeSpan.Zero)
        {
            throw new ArgumentException(
                "AnalysisEndTime cannot be earlier than AnalysisStartTime.",
                nameof(result));
        }

        return elapsed;
    }

    /// <summary>
    /// Formats the analysis result as a CSV string.
    /// </summary>
    /// <param name="result">The analysis result.</param>
    /// <returns>A CSV string representing the analysis result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static string ToCsv(this AnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("ProjectName,ProjectPath,AnalysisSucceeded,TotalFilesAnalyzed,TotalElementsAnalyzed,Violations");
        csv.AppendLine(string.Create(
            CultureInfo.InvariantCulture,
            $"{result.ProjectName ?? string.Empty},{result.ProjectPath ?? string.Empty},{result.AnalysisSucceeded},{result.TotalFilesAnalyzed},{result.TotalElementsAnalyzed},{result.Violations?.Count ?? 0}"));

        return csv.ToString();
    }
}
