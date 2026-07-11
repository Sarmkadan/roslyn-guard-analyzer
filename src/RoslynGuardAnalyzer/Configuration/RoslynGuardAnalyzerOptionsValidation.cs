#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace RoslynGuardAnalyzer.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="RoslynGuardAnalyzerOptions"/> configuration.
/// </summary>
public static class RoslynGuardAnalyzerOptionsValidation
{
    /// <summary>
    /// Validates the provided configuration options.
    /// </summary>
    /// <param name="value">The configuration options to validate</param>
    /// <returns>An enumerable of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this RoslynGuardAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate ProjectPath
        if (string.IsNullOrWhiteSpace(value.ProjectPath))
        {
            problems.Add("ProjectPath cannot be null or whitespace.");
        }

        // Validate AnalysisTimeoutSeconds
        if (value.AnalysisTimeoutSeconds <= 0)
        {
            problems.Add("AnalysisTimeoutSeconds must be greater than 0.");
        }

        // Validate MaxViolationsToReport
        if (value.MaxViolationsToReport < 1)
        {
            problems.Add("MaxViolationsToReport must be at least 1.");
        }
        else if (value.MaxViolationsToReport > 100000)
        {
            problems.Add("MaxViolationsToReport cannot exceed 100000.");
        }

        // Validate LogLevel
        if (value.LogLevel < 0 || value.LogLevel > 4)
        {
            problems.Add("LogLevel must be between 0 and 4 (inclusive).");
        }

        // Validate OutputFormat
        if (string.IsNullOrWhiteSpace(value.OutputFormat))
        {
            problems.Add("OutputFormat cannot be null or whitespace.");
        }
        else if (value.OutputFormat is not ("text" or "json" or "csv" or "html" or "xml"))
        {
            problems.Add("OutputFormat must be one of: text, json, csv, html, xml.");
        }

        // Validate ReportType
        if (string.IsNullOrWhiteSpace(value.ReportType))
        {
            problems.Add("ReportType cannot be null or whitespace.");
        }
        else if (value.ReportType is not ("summary" or "detailed" or "full"))
        {
            problems.Add("ReportType must be one of: summary, detailed, full.");
        }

        // Validate MinimumSeverity
        if (string.IsNullOrWhiteSpace(value.MinimumSeverity))
        {
            problems.Add("MinimumSeverity cannot be null or whitespace.");
        }
        else if (value.MinimumSeverity is not ("Low" or "Medium" or "High" or "Critical"))
        {
            problems.Add("MinimumSeverity must be one of: Low, Medium, High, Critical.");
        }

        // Validate MaxParallelThreads
        if (value.MaxParallelThreads < 1)
        {
            problems.Add("MaxParallelThreads must be at least 1.");
        }
        else if (value.MaxParallelThreads > 64)
        {
            problems.Add("MaxParallelThreads cannot exceed 64.");
        }

        // Validate RuleFilter
        if (value.RuleFilter is null)
        {
            problems.Add("RuleFilter cannot be null.");
        }

        // Validate ExcludePatterns
        if (value.ExcludePatterns is null)
        {
            problems.Add("ExcludePatterns cannot be null.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the provided configuration options are valid.
    /// </summary>
    /// <param name="value">The configuration options to check</param>
    /// <returns>True if valid; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this RoslynGuardAnalyzerOptions value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the provided configuration options are valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The configuration options to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if the configuration is invalid, with a detailed message</exception>
    public static void EnsureValid(this RoslynGuardAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"RoslynGuardAnalyzerOptions validation failed:{Environment.NewLine}" +
                string.Join(Environment.NewLine, problems)
            );
        }
    }
}
