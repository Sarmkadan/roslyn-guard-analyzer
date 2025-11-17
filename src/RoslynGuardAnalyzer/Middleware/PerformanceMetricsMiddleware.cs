// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Middleware;

/// <summary>
/// Captures performance metrics during analysis and stores them in the pipeline context.
/// Useful for identifying bottlenecks and performance regressions.
/// </summary>
public sealed class PerformanceMetricsMiddleware : IMiddleware
{
    public sealed class PerformanceMetrics
    {
        public long TotalMilliseconds { get; set; }
        public long PeakMemoryBytes { get; set; }
        public int ProcessorCount { get; set; }
        public Dictionary<string, long> ComponentTimingsMs { get; } = [];
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public TimeSpan GetElapsed() => EndTime - StartTime;
    }

    private const string MetricsKey = "PerformanceMetrics";
    public string Name => "PerformanceMetrics";

    public async Task InvokeAsync(PipelineContext context, MiddlewareDelegate next)
    {
        var metrics = new PerformanceMetrics
        {
            StartTime = DateTime.UtcNow,
            ProcessorCount = Environment.ProcessorCount
        };

        var stopwatch = Stopwatch.StartNew();
        var initialMemory = GC.GetTotalMemory(false);

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            metrics.EndTime = DateTime.UtcNow;
            metrics.TotalMilliseconds = stopwatch.ElapsedMilliseconds;

            var finalMemory = GC.GetTotalMemory(false);
            metrics.PeakMemoryBytes = Math.Max(finalMemory - initialMemory, 0);

            context.SetItem(MetricsKey, metrics);
        }
    }

    /// <summary>
    /// Records timing for a specific component.
    /// </summary>
    public static void RecordComponentTiming(PipelineContext context, string componentName, long milliseconds)
    {
        var metrics = context.GetItem<PerformanceMetrics>(MetricsKey);
        if (metrics != null)
        {
            if (!metrics.ComponentTimingsMs.ContainsKey(componentName))
                metrics.ComponentTimingsMs[componentName] = 0;

            metrics.ComponentTimingsMs[componentName] += milliseconds;
        }
    }

    /// <summary>
    /// Gets the metrics from a pipeline context.
    /// </summary>
    public static PerformanceMetrics? GetMetrics(PipelineContext context)
    {
        return context.GetItem<PerformanceMetrics>(MetricsKey);
    }

    /// <summary>
    /// Generates a human-readable performance report.
    /// </summary>
    public static string GenerateReport(PerformanceMetrics metrics)
    {
        var report = new System.Text.StringBuilder();

        report.AppendLine("=== Performance Report ===");
        report.AppendLine($"Total Time: {metrics.TotalMilliseconds}ms");
        report.AppendLine($"Duration: {metrics.GetElapsed():hh\\:mm\\:ss\\.fff}");
        report.AppendLine($"Memory Delta: {(metrics.PeakMemoryBytes / 1024 / 1024)}MB");
        report.AppendLine($"Processors: {metrics.ProcessorCount}");

        if (metrics.ComponentTimingsMs.Count > 0)
        {
            report.AppendLine();
            report.AppendLine("Component Timings:");

            foreach (var (component, ms) in metrics.ComponentTimingsMs.OrderByDescending(x => x.Value))
            {
                var percentage = (ms * 100.0) / metrics.TotalMilliseconds;
                report.AppendLine($"  {component}: {ms}ms ({percentage:F1}%)");
            }
        }

        return report.ToString();
    }
}
