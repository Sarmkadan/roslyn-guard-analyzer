#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace RoslynGuardAnalyzer.Cli;

/// <summary>
/// Provides extension methods for <see cref="CommandLineProcessor"/> to enhance CLI functionality
/// with common operations and convenience methods.
/// </summary>
public static class CommandLineProcessorExtensions
{
    /// <summary>
    /// Attempts to process the command-line arguments with automatic error handling.
    /// Returns a tuple indicating success and providing the parsed options if successful.
    /// </summary>
    /// <param name="processor">The command line processor instance.</param>
    /// <param name="throwOnError">Whether to throw an exception on processing errors.</param>
    /// <returns>A tuple with success status, parsed options, and any errors encountered.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="processor"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="throwOnError"/> is <see langword="true"/> and processing fails.</exception>
    public static (bool Success, CliOptions Options, IReadOnlyList<string> Errors) TryProcess(
        this CommandLineProcessor processor,
        bool throwOnError = false)
    {
        ArgumentNullException.ThrowIfNull(processor);

        var result = processor.Process();

        if (throwOnError && result is { Success: false, Errors.Count: > 0 })
        {
            throw new InvalidOperationException(
                $"Command line processing failed: {string.Join(", ", result.Errors)}");
        }

        return (result.Success, result.Options, result.Errors.AsReadOnly());
    }

    /// <summary>
    /// Validates the parsed paths and returns a detailed validation result.
    /// Includes information about which specific paths failed validation.
    /// </summary>
    /// <param name="processor">The command line processor instance.</param>
    /// <returns>A tuple with validation status and detailed error information.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="processor"/> is <see langword="null"/>.</exception>
    public static (bool Valid, IReadOnlyList<string> Errors, IReadOnlyList<string> FailedPaths) ValidatePathsDetailed(
        this CommandLineProcessor processor)
    {
        ArgumentNullException.ThrowIfNull(processor);

        var (valid, errors) = processor.ValidatePaths();

        var failedPaths = new List<string>();
        if (!valid && errors.Count > 0)
        {
            foreach (var error in errors)
            {
                if (error.StartsWith("Path not found:", StringComparison.Ordinal))
                {
                    var path = error["Path not found:".Length..].Trim();
                    failedPaths.Add(path);
                }
                else if (error.StartsWith("Config file not found:", StringComparison.Ordinal))
                {
                    var path = error["Config file not found:".Length..].Trim();
                    failedPaths.Add(path);
                }
            }
        }

        return (valid, errors.AsReadOnly(), failedPaths.AsReadOnly());
    }

    /// <summary>
    /// Gets a formatted summary of the parsed options suitable for display in reports.
    /// </summary>
    /// <param name="processor">The command line processor instance.</param>
    /// <returns>A formatted string containing the options summary.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="processor"/> is <see langword="null"/>.</exception>
    public static string GetOptionsSummaryFormatted(this CommandLineProcessor processor)
    {
        ArgumentNullException.ThrowIfNull(processor);

        var options = processor.GetOptions();

        var summary = new System.Text.StringBuilder();
        summary.AppendLine("=== Analyzer Configuration ===");
        summary.AppendLine($"Target: {options.GetTargetPath() ?? "Not specified"}");
        summary.AppendLine($"Output Format: {options.OutputFormat}");
        summary.AppendLine($"Timeout: {options.AnalysisTimeoutSeconds}s");
        summary.AppendLine($"Threads: {options.MaxParallelThreads}");
        summary.AppendLine($"Verbose: {options.Verbose}");
        summary.AppendLine($"Fail on Violations: {options.FailOnViolations}");
        summary.AppendLine($"Generate Report: {options.GenerateReport}");
        summary.AppendLine($"Report Type: {options.ReportType}");

        if (options.RuleFilter.Count > 0)
        {
            summary.AppendLine($"Filtered Rules: {options.RuleFilter.Count}");
        }

        if (!string.IsNullOrEmpty(options.ConfigFile))
        {
            summary.AppendLine($"Config File: {options.ConfigFile}");
        }

        summary.AppendLine("===========================");
        return summary.ToString();
    }

    /// <summary>
    /// Determines if the parsed options indicate analysis should be performed.
    /// </summary>
    /// <param name="processor">The command line processor instance.</param>
    /// <returns>True if analysis mode is active; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="processor"/> is <see langword="null"/>.</exception>
    public static bool IsAnalysisMode(this CommandLineProcessor processor)
    {
        ArgumentNullException.ThrowIfNull(processor);

        var options = processor.GetOptions();
        return options.IsAnalysisMode;
    }
}
