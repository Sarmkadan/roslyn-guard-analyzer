#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace RoslynGuardAnalyzer.Infrastructure;

/// <summary>
/// Validation helpers for analyzer service collection configuration.
/// </summary>
public static class ServiceCollectionExtensionsValidation
{
    /// <summary>
    /// Validates an analyzer configuration instance.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AnalyzerConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.DataDirectory))
        {
            problems.Add($"DataDirectory cannot be null or whitespace.");
        }
        else if (!Path.IsPathRooted(value.DataDirectory))
        {
            problems.Add($"DataDirectory '{value.DataDirectory}' must be an absolute path.");
        }
        else if (value.DataDirectory.Contains(".."))
        {
            problems.Add($"DataDirectory '{value.DataDirectory}' contains invalid path segments.");
        }

        if (value.MaxViolationsToReport <= 0)
        {
            problems.Add($"MaxViolationsToReport must be greater than 0, but was {value.MaxViolationsToReport}.");
        }

        if (value.AnalysisTimeoutSeconds <= 0)
        {
            problems.Add($"AnalysisTimeoutSeconds must be greater than 0, but was {value.AnalysisTimeoutSeconds}.");
        }
        else if (value.AnalysisTimeoutSeconds > 86400)
        {
            problems.Add($"AnalysisTimeoutSeconds cannot exceed 86400 seconds (24 hours), but was {value.AnalysisTimeoutSeconds}.");
        }

        if (value.LogLevel < 0 || value.LogLevel > 4)
        {
            problems.Add($"LogLevel must be between 0 and 4 inclusive, but was {value.LogLevel}.");
        }

        if (value.MaxParallelThreads <= 0)
        {
            problems.Add($"MaxParallelThreads must be greater than 0, but was {value.MaxParallelThreads}.");
        }
        else if (value.MaxParallelThreads > 1024)
        {
            problems.Add($"MaxParallelThreads cannot exceed 1024, but was {value.MaxParallelThreads}.");
        }

        if (string.IsNullOrWhiteSpace(value.DefaultReportFormat))
        {
            problems.Add("DefaultReportFormat cannot be null or whitespace.");
        }
        else if (value.DefaultReportFormat.Length > 64)
        {
            problems.Add($"DefaultReportFormat cannot exceed 64 characters, but was {value.DefaultReportFormat.Length}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the analyzer configuration is valid.
    /// </summary>
    /// <param name="value">The configuration to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this AnalyzerConfiguration value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the analyzer configuration is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is null or invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this AnalyzerConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Analyzer configuration is invalid. Problems:\n\t- {
                string.Join("\n\t- ", problems)
                }");
        }
    }

    /// <summary>
    /// Validates a data directory path.
    /// </summary>
    /// <param name="dataDirectory">The data directory path to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateDataDirectory(string dataDirectory)
    {
        if (string.IsNullOrWhiteSpace(dataDirectory))
        {
            return new[] { "DataDirectory cannot be null or whitespace." };
        }

        var problems = new List<string>();

        if (!Path.IsPathRooted(dataDirectory))
        {
            problems.Add($"DataDirectory '{dataDirectory}' must be an absolute path.");
        }

        if (dataDirectory.Contains(".."))
        {
            problems.Add($"DataDirectory '{dataDirectory}' contains invalid path segments.");
        }

        if (dataDirectory.Length > 512)
        {
            problems.Add($"DataDirectory cannot exceed 512 characters, but was {dataDirectory.Length}.");
        }

        try
        {
            // Test if we can create a valid path
            Path.GetFullPath(dataDirectory);
        }
        catch (Exception ex)
        {
            problems.Add($"DataDirectory '{dataDirectory}' is not a valid path: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a positive integer value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">The name of the parameter for error messages.</param>
    /// <param name="maxValue">Optional maximum allowed value.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException"><paramref name="paramName"/> is null or empty.</exception>
    public static IReadOnlyList<string> ValidatePositiveInt(
        int value,
        string paramName,
        int? maxValue = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName);

        var problems = new List<string>();

        if (value <= 0)
        {
            problems.Add($"{paramName} must be greater than 0, but was {value}.");
        }
        else if (maxValue.HasValue && value > maxValue.Value)
        {
            problems.Add($"{paramName} cannot exceed {maxValue.Value}, but was {value}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a log level value.
    /// </summary>
    /// <param name="logLevel">The log level to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateLogLevel(int logLevel)
    {
        var problems = new List<string>();

        if (logLevel < 0 || logLevel > 4)
        {
            problems.Add($"LogLevel must be between 0 and 4 inclusive, but was {logLevel}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a report format string.
    /// </summary>
    /// <param name="reportFormat">The report format to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateReportFormat(string reportFormat)
    {
        if (string.IsNullOrWhiteSpace(reportFormat))
        {
            return new[] { "Report format cannot be null or whitespace." };
        }

        var problems = new List<string>();

        if (reportFormat.Length > 64)
        {
            problems.Add($"Report format cannot exceed 64 characters, but was {reportFormat.Length}.");
        }

        // Basic format validation - alphanumeric, underscores, hyphens, dots
        if (!Regex.IsMatch(
            reportFormat,
            "^[a-zA-Z0-9_.-]+$",
            RegexOptions.CultureInvariant))
        {
            problems.Add($"Report format '{reportFormat}' contains invalid characters. Only alphanumeric, underscores, hyphens, and dots are allowed.");
        }

        return problems.AsReadOnly();
    }
}