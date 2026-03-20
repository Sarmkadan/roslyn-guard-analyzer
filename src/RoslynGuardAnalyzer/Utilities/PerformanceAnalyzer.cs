// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RoslynGuardAnalyzer.Utilities;

/// <summary>
/// Analyzes performance data from analysis runs to identify bottlenecks and trends.
/// Provides statistical analysis and recommendations for optimization.
/// </summary>
public sealed class PerformanceAnalyzer
{
    public sealed class PerformanceMetrics
    {
        public required string ComponentName { get; init; }
        public required long TotalTimeMs { get; init; }
        public required long MinTimeMs { get; init; }
        public required long MaxTimeMs { get; init; }
        public required long AverageTimeMs { get; init; }
        public required int ExecutionCount { get; init; }
        public double PercentageOfTotal { get; set; }
    }

    private readonly Dictionary<string, List<long>> _timings = [];
    private readonly object _lockObject = new();

    /// <summary>
    /// Records a performance measurement.
    /// </summary>
    public void RecordTiming(string componentName, long milliseconds)
    {
        if (string.IsNullOrWhiteSpace(componentName))
            throw new ArgumentException("Component name cannot be null or empty", nameof(componentName));

        if (milliseconds < 0)
            throw new ArgumentException("Milliseconds cannot be negative", nameof(milliseconds));

        lock (_lockObject)
        {
            if (!_timings.ContainsKey(componentName))
                _timings[componentName] = [];

            _timings[componentName].Add(milliseconds);
        }
    }

    /// <summary>
    /// Gets metrics for a specific component.
    /// </summary>
    public PerformanceMetrics? GetMetricsForComponent(string componentName)
    {
        lock (_lockObject)
        {
            if (!_timings.TryGetValue(componentName, out var measurements) || measurements.Count == 0)
                return null;

            var total = measurements.Sum();
            return new PerformanceMetrics
            {
                ComponentName = componentName,
                TotalTimeMs = total,
                MinTimeMs = measurements.Min(),
                MaxTimeMs = measurements.Max(),
                AverageTimeMs = total / measurements.Count,
                ExecutionCount = measurements.Count
            };
        }
    }

    /// <summary>
    /// Gets metrics for all components.
    /// </summary>
    public List<PerformanceMetrics> GetAllMetrics()
    {
        lock (_lockObject)
        {
            var allMetrics = new List<PerformanceMetrics>();
            var totalTime = _timings.Values.SelectMany(x => x).Sum();

            foreach (var (componentName, measurements) in _timings)
            {
                if (measurements.Count == 0)
                    continue;

                var total = measurements.Sum();
                var metrics = new PerformanceMetrics
                {
                    ComponentName = componentName,
                    TotalTimeMs = total,
                    MinTimeMs = measurements.Min(),
                    MaxTimeMs = measurements.Max(),
                    AverageTimeMs = total / measurements.Count,
                    ExecutionCount = measurements.Count,
                    PercentageOfTotal = totalTime > 0 ? (total * 100.0) / totalTime : 0
                };

                allMetrics.Add(metrics);
            }

            return allMetrics.OrderByDescending(m => m.TotalTimeMs).ToList();
        }
    }

    /// <summary>
    /// Identifies bottleneck components (slowest ones).
    /// </summary>
    public List<PerformanceMetrics> GetBottlenecks(int count = 5)
    {
        return GetAllMetrics().Take(count).ToList();
    }

    /// <summary>
    /// Calculates the total analysis time.
    /// </summary>
    public long GetTotalTimeMs()
    {
        lock (_lockObject)
        {
            return _timings.Values.SelectMany(x => x).Sum();
        }
    }

    /// <summary>
    /// Generates a performance report.
    /// </summary>
    public string GenerateReport()
    {
        var metrics = GetAllMetrics();
        var totalTime = GetTotalTimeMs();

        var sb = new System.Text.StringBuilder();

        sb.AppendLine("=== Performance Analysis Report ===");
        sb.AppendLine($"Total Analysis Time: {totalTime}ms");
        sb.AppendLine($"Components Measured: {metrics.Count}");
        sb.AppendLine();

        sb.AppendLine("Component Breakdown (sorted by total time):");
        sb.AppendLine();

        foreach (var metric in metrics)
        {
            sb.AppendLine($"{metric.ComponentName}:");
            sb.AppendLine($"  Total:     {metric.TotalTimeMs}ms ({metric.PercentageOfTotal:F1}%)");
            sb.AppendLine($"  Average:   {metric.AverageTimeMs}ms");
            sb.AppendLine($"  Min/Max:   {metric.MinTimeMs}ms / {metric.MaxTimeMs}ms");
            sb.AppendLine($"  Calls:     {metric.ExecutionCount}");
            sb.AppendLine();
        }

        // Recommendations
        sb.AppendLine("Recommendations:");

        var bottlenecks = GetBottlenecks(3);
        foreach (var bottleneck in bottlenecks)
        {
            sb.AppendLine($"  - Optimize {bottleneck.ComponentName} ({bottleneck.PercentageOfTotal:F1}% of total time)");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Clears all recorded timings.
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            _timings.Clear();
        }
    }

    /// <summary>
    /// Gets the count of measured components.
    /// </summary>
    public int ComponentCount
    {
        get
        {
            lock (_lockObject)
            {
                return _timings.Count;
            }
        }
    }

    /// <summary>
    /// Checks if a component has been measured.
    /// </summary>
    public bool HasComponent(string componentName)
    {
        lock (_lockObject)
        {
            return _timings.ContainsKey(componentName);
        }
    }
}
