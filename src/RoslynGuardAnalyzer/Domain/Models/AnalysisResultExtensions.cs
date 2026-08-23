using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RoslynGuardAnalyzer.Core;

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

    /// <summary>
    /// Gets the highest severity level present among the result's violations.
    /// </summary>
    /// <param name="result">The analysis result.</param>
    /// <returns>The worst severity level, or null if the result has no violations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static SeverityLevel? WorstSeverity(this AnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Violations.Count == 0
            ? null
            : result.Violations.Max(v => v.Severity);
    }

    /// <summary>
    /// Groups the result's violations by file path.
    /// </summary>
    /// <param name="result">The analysis result.</param>
    /// <returns>An enumerable of groupings of violations by file path.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static IEnumerable<IGrouping<string, RuleViolation>> GroupByFile(this AnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Violations.GroupBy(v => v.FilePath);
    }

    /// <summary>
    /// Converts the analysis result to a process exit code.
    /// </summary>
    /// <param name="result">The analysis result.</param>
    /// <returns>0 when clean, 1 when warnings are present, 2 when errors or critical violations are present.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static int ToExitCode(this AnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Violations.Any(v => v.Severity == SeverityLevel.Error || v.Severity == SeverityLevel.Critical))
        {
            return 2;
        }

        if (result.Violations.Any(v => v.Severity == SeverityLevel.Warning))
        {
            return 1;
        }

        return 0;
    }
}
