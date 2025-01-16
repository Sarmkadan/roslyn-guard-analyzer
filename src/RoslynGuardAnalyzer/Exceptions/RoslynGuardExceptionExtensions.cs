#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace RoslynGuardAnalyzer.Exceptions;

/// <summary>
/// Extension methods for <see cref="RoslynGuardException"/> and its derived types.
/// Provides formatting, summarization, and analysis capabilities for exception handling.
/// </summary>
public static class RoslynGuardExceptionExtensions
{
    /// <summary>
    /// Creates a formatted error report string for the exception, including all details.
    /// </summary>
    /// <param name="exception">The exception to format.</param>
    /// <returns>Formatted error report with all available information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static string FormatErrorReport(this RoslynGuardException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var report = new System.Text.StringBuilder();
        report.AppendLine("=== ROSLYN GUARD ANALYZER ERROR REPORT ===");
        report.AppendLine();

        // Basic exception info
        report.AppendLine($"Error Code: {exception.ErrorCode}");
        report.AppendLine($"Occurred At: {exception.OccurredAt:yyyy-MM-dd HH:mm:ss UTC}");
        report.AppendLine($"Message: {exception.Message}");
        report.AppendLine();

        // Type-specific information
        switch (exception)
        {
            case RuleNotFoundException ruleEx:
                report.AppendLine("Exception Type: RuleNotFoundException");
                report.AppendLine($"Rule ID: {ruleEx.RuleId}");
                break;

            case AnalysisException analysisEx:
                report.AppendLine("Exception Type: AnalysisException");
                if (!string.IsNullOrEmpty(analysisEx.ProjectPath))
                    report.AppendLine($"Project Path: {analysisEx.ProjectPath}");

                if (analysisEx.Details?.Count > 0)
                {
                    report.AppendLine($"Details ({analysisEx.Details.Count} items):");
                    foreach (var detail in analysisEx.Details)
                    {
                        report.AppendLine($" - {detail}");
                    }
                }
                break;

            case ConfigurationException configEx:
                report.AppendLine("Exception Type: ConfigurationException");
                if (!string.IsNullOrEmpty(configEx.ConfigKey))
                    report.AppendLine($"Config Key: {configEx.ConfigKey}");
                break;

            case FileAccessException fileEx:
                report.AppendLine("Exception Type: FileAccessException");
                report.AppendLine($"File Path: {fileEx.FilePath}");
                break;

            case ParseException parseEx:
                report.AppendLine("Exception Type: ParseException");
                report.AppendLine($"File Path: {parseEx.FilePath}");
                break;

            case AnalysisTimeoutException timeoutEx:
                report.AppendLine("Exception Type: AnalysisTimeoutException");
                report.AppendLine($"Timeout: {timeoutEx.TimeoutSeconds} seconds");
                break;

            default:
                report.AppendLine("Exception Type: RoslynGuardException");
                break;
        }

        // Inner exception if present
        if (exception.InnerException is not null)
        {
            report.AppendLine();
            report.AppendLine("=== INNER EXCEPTION ===");
            report.AppendLine(exception.InnerException.ToString());
        }

        report.AppendLine();
        report.AppendLine("=== END OF ERROR REPORT ===");

        return report.ToString();
    }

    /// <summary>
    /// Creates a concise error summary suitable for logging or user display.
    /// </summary>
    /// <param name="exception">The exception to summarize.</param>
    /// <param name="includeDetails">Whether to include exception details in the summary.</param>
    /// <returns>Concise error summary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static string ToErrorSummary(this RoslynGuardException exception, bool includeDetails = true)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var summary = new System.Text.StringBuilder();
        summary.Append($"[{exception.ErrorCode}] {exception.Message}");

        switch (exception)
        {
            case RuleNotFoundException ruleEx:
                summary.Append($" | Rule: {ruleEx.RuleId}");
                break;

            case AnalysisException analysisEx:
                if (!string.IsNullOrEmpty(analysisEx.ProjectPath))
                    summary.Append($" | Project: {analysisEx.ProjectPath}");

                if (includeDetails && analysisEx.Details?.Count > 0)
                {
                    var firstDetail = analysisEx.Details.FirstOrDefault() ?? "Unknown error";
                    summary.Append($" | Details: {firstDetail}");
                }
                break;

            case ConfigurationException configEx:
                if (!string.IsNullOrEmpty(configEx.ConfigKey))
                    summary.Append($" | Config: {configEx.ConfigKey}");
                break;

            case FileAccessException fileEx:
                summary.Append($" | File: {fileEx.FilePath}");
                break;

            case ParseException parseEx:
                summary.Append($" | File: {parseEx.FilePath}");
                break;

            case AnalysisTimeoutException timeoutEx:
                summary.Append($" | Timeout: {timeoutEx.TimeoutSeconds}s");
                break;
        }

        summary.Append($" | Occurred: {exception.OccurredAt:yyyy-MM-dd HH:mm:ss}");

        return summary.ToString();
    }

    /// <summary>
    /// Determines if the exception represents a critical failure that should stop analysis.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is critical.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static bool IsCritical(this RoslynGuardException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            AnalysisException => true,
            ConfigurationException => true,
            FileAccessException => true,
            ParseException => true,
            AnalysisTimeoutException => true,
            _ => false
        };
    }

    /// <summary>
    /// Creates a dictionary containing all exception properties for serialization or analysis.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <returns>Dictionary with exception properties.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static Dictionary<string, object?> ToPropertyDictionary(this RoslynGuardException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["ErrorCode"] = exception.ErrorCode,
            ["OccurredAt"] = exception.OccurredAt,
            ["Message"] = exception.Message,
            ["ExceptionType"] = exception.GetType().Name,
            ["IsCritical"] = exception.IsCritical()
        };

        switch (exception)
        {
            case RuleNotFoundException ruleEx:
                dict["RuleId"] = ruleEx.RuleId;
                break;

            case AnalysisException analysisEx:
                dict["ProjectPath"] = analysisEx.ProjectPath;
                dict["Details"] = analysisEx.Details?.ToArray() ?? Array.Empty<string>();
                break;

            case ConfigurationException configEx:
                dict["ConfigKey"] = configEx.ConfigKey;
                break;

            case FileAccessException fileEx:
                dict["FilePath"] = fileEx.FilePath;
                break;

            case ParseException parseEx:
                dict["FilePath"] = parseEx.FilePath;
                break;

            case AnalysisTimeoutException timeoutEx:
                dict["TimeoutSeconds"] = timeoutEx.TimeoutSeconds;
                break;
        }

        return dict;
    }
}