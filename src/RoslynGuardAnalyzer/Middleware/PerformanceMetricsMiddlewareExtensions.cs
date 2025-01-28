#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace RoslynGuardAnalyzer.Middleware;

/// <summary>
/// Extension methods for <see cref="PerformanceMetricsMiddleware.PerformanceMetrics"/> that provide additional
/// functionality for working with performance metrics.
/// </summary>
public static class PerformanceMetricsMiddlewareExtensions
{
    /// <summary>
    /// Gets the total elapsed time as a formatted string.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <returns>The formatted elapsed time string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static string GetElapsedFormatted(this PerformanceMetricsMiddleware.PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        return metrics.GetElapsed().ToString("hh\\:mm\\:ss\\.fff", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets the peak memory usage as a formatted string in megabytes.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <returns>The formatted memory usage in MB.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static string GetPeakMemoryFormatted(this PerformanceMetricsMiddleware.PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        return $"{(metrics.PeakMemoryBytes / 1024 / 1024)} MB";
    }

    /// <summary>
    /// Gets the component timings as a read-only dictionary.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <returns>A read-only dictionary of component timings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static IReadOnlyDictionary<string, long> GetComponentTimings(
        this PerformanceMetricsMiddleware.PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        return metrics.ComponentTimingsMs;
    }

    /// <summary>
    /// Gets the peak memory usage in bytes.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <returns>The peak memory usage in bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static long GetPeakMemoryBytes(this PerformanceMetricsMiddleware.PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        return metrics.PeakMemoryBytes;
    }

    /// <summary>
    /// Gets the total execution time in milliseconds.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <returns>The total execution time in milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static long GetTotalMilliseconds(this PerformanceMetricsMiddleware.PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        return metrics.TotalMilliseconds;
    }

    /// <summary>
    /// Gets the processor count used during execution.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <returns>The processor count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static int GetProcessorCount(this PerformanceMetricsMiddleware.PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        return metrics.ProcessorCount;
    }

    /// <summary>
    /// Gets the start time of the execution.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <returns>The start time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static DateTime GetStartTime(this PerformanceMetricsMiddleware.PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        return metrics.StartTime;
    }

    /// <summary>
    /// Gets the end time of the execution.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <returns>The end time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static DateTime GetEndTime(this PerformanceMetricsMiddleware.PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        return metrics.EndTime;
    }

    /// <summary>
    /// Gets the component with the highest execution time.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <returns>The component name with the highest execution time, or null if no components recorded.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static string? GetSlowestComponent(this PerformanceMetricsMiddleware.PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        return metrics.ComponentTimingsMs.Count > 0
            ? metrics.ComponentTimingsMs.MaxBy(x => x.Value).Key
            : null;
    }

    /// <summary>
    /// Gets the execution time of a specific component.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <param name="componentName">The name of the component.</param>
    /// <returns>The execution time in milliseconds, or 0 if component not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> or <paramref name="componentName"/> is null.</exception>
    public static long GetComponentTime(
        this PerformanceMetricsMiddleware.PerformanceMetrics metrics,
        string componentName)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(componentName);

        return metrics.ComponentTimingsMs.TryGetValue(componentName, out var time)
            ? time
            : 0;
    }

    /// <summary>
    /// Gets all component names that were timed during execution.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <returns>An enumerable of component names.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static IEnumerable<string> GetTimedComponents(this PerformanceMetricsMiddleware.PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        return metrics.ComponentTimingsMs.Keys;
    }

    /// <summary>
    /// Checks if any components were timed during execution.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <returns>True if components were timed; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static bool HasComponentTimings(this PerformanceMetricsMiddleware.PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        return metrics.ComponentTimingsMs.Count > 0;
    }

    /// <summary>
    /// Gets the percentage of total time spent in a specific component.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <param name="componentName">The name of the component.</param>
    /// <returns>The percentage of total time (0-100), or 0 if component not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> or <paramref name="componentName"/> is null.</exception>
    public static double GetComponentPercentage(
        this PerformanceMetricsMiddleware.PerformanceMetrics metrics,
        string componentName)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(componentName);

        if (metrics.TotalMilliseconds == 0)
            return 0;

        if (metrics.ComponentTimingsMs.TryGetValue(componentName, out var time))
        {
            return (time * 100.0) / metrics.TotalMilliseconds;
        }

        return 0;
    }

    /// <summary>
    /// Gets the top N slowest components by execution time.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <param name="count">The number of components to return.</param>
    /// <returns>An enumerable of component names and their times, ordered by descending time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is less than 1.</exception>
    public static IEnumerable<KeyValuePair<string, long>> GetTopSlowestComponents(
        this PerformanceMetricsMiddleware.PerformanceMetrics metrics,
        int count = 5)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1.");

        return metrics.ComponentTimingsMs
            .OrderByDescending(x => x.Value)
            .Take(count);
    }

    /// <summary>
    /// Gets a summary of the performance metrics as a dictionary.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <returns>A dictionary containing key performance metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static IReadOnlyDictionary<string, string> GetMetricsSummary(
        this PerformanceMetricsMiddleware.PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        var summary = new Dictionary<string, string>(StringComparer.Ordinal);

        summary["TotalMilliseconds"] = metrics.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        summary["PeakMemoryBytes"] = metrics.PeakMemoryBytes.ToString(CultureInfo.InvariantCulture);
        summary["ProcessorCount"] = metrics.ProcessorCount.ToString(CultureInfo.InvariantCulture);
        summary["StartTime"] = metrics.StartTime.ToString("O", CultureInfo.InvariantCulture);
        summary["EndTime"] = metrics.EndTime.ToString("O", CultureInfo.InvariantCulture);
        summary["Elapsed"] = metrics.GetElapsed().ToString("c", CultureInfo.InvariantCulture);

        return summary;
    }

    /// <summary>
    /// Gets the average execution time per component.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <returns>The average execution time in milliseconds, or 0 if no components.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static double GetAverageComponentTime(this PerformanceMetricsMiddleware.PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        return metrics.ComponentTimingsMs.Count == 0
            ? 0
            : metrics.ComponentTimingsMs.Values.Average();
    }

    /// <summary>
    /// Gets the total execution time of all components combined.
    /// </summary>
    /// <param name="metrics">The performance metrics.</param>
    /// <returns>The total component execution time in milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static long GetTotalComponentTime(this PerformanceMetricsMiddleware.PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        return metrics.ComponentTimingsMs.Count > 0
            ? metrics.ComponentTimingsMs.Values.Sum()
            : 0;
    }
}
