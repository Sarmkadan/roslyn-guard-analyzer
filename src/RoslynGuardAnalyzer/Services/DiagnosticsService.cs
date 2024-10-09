// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Provides diagnostic information about the analyzer and analysis runs.
/// Tracks performance, errors, and usage statistics.
/// </summary>
public sealed class DiagnosticsService
{
    private sealed class RunStatistics
    {
        public int AnalysisCount { get; set; }
        public int TotalViolationsFound { get; set; }
        public long TotalAnalysisTimeMs { get; set; }
        public int ErrorCount { get; set; }
        public List<string> RecentErrors { get; } = [];
    }

    private readonly RunStatistics _stats = new();
    private readonly object _lockObject = new();
    private readonly DateTime _startTime = DateTime.UtcNow;

    /// <summary>
    /// Records a successful analysis run.
    /// </summary>
    public void RecordAnalysis(long durationMs, int violationsFound)
    {
        lock (_lockObject)
        {
            _stats.AnalysisCount++;
            _stats.TotalAnalysisTimeMs += durationMs;
            _stats.TotalViolationsFound += violationsFound;
        }
    }

    /// <summary>
    /// Records an error that occurred during analysis.
    /// </summary>
    public void RecordError(string errorMessage)
    {
        lock (_lockObject)
        {
            _stats.ErrorCount++;
            _stats.RecentErrors.Add(errorMessage);

            // Keep only the last 10 errors
            if (_stats.RecentErrors.Count > 10)
                _stats.RecentErrors.RemoveAt(0);
        }
    }

    /// <summary>
    /// Gets the average analysis time in milliseconds.
    /// </summary>
    public long GetAverageAnalysisTime()
    {
        lock (_lockObject)
        {
            if (_stats.AnalysisCount == 0)
                return 0;

            return _stats.TotalAnalysisTimeMs / _stats.AnalysisCount;
        }
    }

    /// <summary>
    /// Gets the total number of analyses performed.
    /// </summary>
    public int GetAnalysisCount()
    {
        lock (_lockObject)
        {
            return _stats.AnalysisCount;
        }
    }

    /// <summary>
    /// Gets the total violations found across all analyses.
    /// </summary>
    public int GetTotalViolationsFound()
    {
        lock (_lockObject)
        {
            return _stats.TotalViolationsFound;
        }
    }

    /// <summary>
    /// Gets the error count.
    /// </summary>
    public int GetErrorCount()
    {
        lock (_lockObject)
        {
            return _stats.ErrorCount;
        }
    }

    /// <summary>
    /// Gets the last N errors.
    /// </summary>
    public IReadOnlyList<string> GetRecentErrors(int count = 5)
    {
        lock (_lockObject)
        {
            return _stats.RecentErrors
                .Skip(Math.Max(0, _stats.RecentErrors.Count - count))
                .ToList()
                .AsReadOnly();
        }
    }

    /// <summary>
    /// Gets system information for diagnostics.
    /// </summary>
    public Dictionary<string, object> GetSystemInfo()
    {
        var process = Process.GetCurrentProcess();
        var uptime = DateTime.UtcNow - _startTime;

        return new Dictionary<string, object>
        {
            ["OS"] = Environment.OSVersion.ToString(),
            ["ProcessorCount"] = Environment.ProcessorCount,
            ["DotNetVersion"] = Environment.Version.ToString(),
            ["CurrentMemoryMB"] = process.WorkingSet64 / 1024 / 1024,
            ["PeakMemoryMB"] = process.PeakWorkingSet64 / 1024 / 1024,
            ["UptimeHours"] = uptime.TotalHours,
            ["AnalysesCompleted"] = GetAnalysisCount(),
            ["ErrorsEncountered"] = GetErrorCount()
        };
    }

    /// <summary>
    /// Generates a diagnostic report as a string.
    /// </summary>
    public string GenerateDiagnosticReport()
    {
        lock (_lockObject)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("=== Diagnostics Report ===");
            sb.AppendLine($"Uptime: {(DateTime.UtcNow - _startTime):hh\\:mm\\:ss}");
            sb.AppendLine();

            sb.AppendLine("Analysis Statistics:");
            sb.AppendLine($"  Total Runs: {_stats.AnalysisCount}");
            sb.AppendLine($"  Total Time: {_stats.TotalAnalysisTimeMs}ms");
            sb.AppendLine($"  Avg Time: {(_stats.AnalysisCount > 0 ? _stats.TotalAnalysisTimeMs / _stats.AnalysisCount : 0)}ms");
            sb.AppendLine($"  Total Violations: {_stats.TotalViolationsFound}");
            sb.AppendLine();

            sb.AppendLine("Error Statistics:");
            sb.AppendLine($"  Total Errors: {_stats.ErrorCount}");

            if (_stats.RecentErrors.Count > 0)
            {
                sb.AppendLine("  Recent Errors:");
                foreach (var error in _stats.RecentErrors.TakeLast(5))
                {
                    sb.AppendLine($"    - {error}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("System Information:");
            var sysInfo = GetSystemInfo();
            foreach (var (key, value) in sysInfo)
            {
                sb.AppendLine($"  {key}: {value}");
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Resets all statistics (useful for testing).
    /// </summary>
    public void Reset()
    {
        lock (_lockObject)
        {
            _stats.AnalysisCount = 0;
            _stats.TotalViolationsFound = 0;
            _stats.TotalAnalysisTimeMs = 0;
            _stats.ErrorCount = 0;
            _stats.RecentErrors.Clear();
        }
    }
}
