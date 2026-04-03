// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Formatters;

namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Handles writing formatted analysis results to files or console.
/// Supports multiple output formats and destinations.
/// </summary>
public sealed class OutputWriter
{
    private readonly FormatterRegistry _formatterRegistry;

    public OutputWriter(FormatterRegistry? formatterRegistry = null)
    {
        _formatterRegistry = formatterRegistry ?? FormatterRegistry.CreateWithDefaults();
    }

    /// <summary>
    /// Writes analysis result to the specified output.
    /// </summary>
    public async Task WriteResultAsync(AnalysisResult result, string format, string? outputFilePath = null)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        if (string.IsNullOrWhiteSpace(format))
            throw new ArgumentException("Format cannot be null or empty", nameof(format));

        var formatter = _formatterRegistry.GetFormatterOrThrow(format);
        var formattedOutput = formatter.FormatResult(result);

        if (string.IsNullOrWhiteSpace(outputFilePath))
        {
            // Write to console
            Console.Out.WriteLine(formattedOutput);
        }
        else
        {
            // Write to file
            await WriteToFileAsync(outputFilePath, formattedOutput);
        }
    }

    /// <summary>
    /// Writes violations to the specified output.
    /// </summary>
    public async Task WriteViolationsAsync(
        IEnumerable<RuleViolation> violations,
        string format,
        string? outputFilePath = null)
    {
        if (violations == null)
            throw new ArgumentNullException(nameof(violations));

        var formatter = _formatterRegistry.GetFormatterOrThrow(format);
        var formattedOutput = formatter.FormatViolations(violations);

        if (string.IsNullOrWhiteSpace(outputFilePath))
        {
            Console.Out.WriteLine(formattedOutput);
        }
        else
        {
            await WriteToFileAsync(outputFilePath, formattedOutput);
        }
    }

    /// <summary>
    /// Writes a report to the specified output.
    /// </summary>
    public async Task WriteReportAsync(
        ViolationReport report,
        string format,
        string? outputFilePath = null)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        var formatter = _formatterRegistry.GetFormatterOrThrow(format);
        var formattedOutput = formatter.FormatReport(report);

        if (string.IsNullOrWhiteSpace(outputFilePath))
        {
            Console.Out.WriteLine(formattedOutput);
        }
        else
        {
            await WriteToFileAsync(outputFilePath, formattedOutput);
        }
    }

    /// <summary>
    /// Writes plain text output.
    /// </summary>
    public async Task WriteAsync(string content, string? outputFilePath = null)
    {
        if (string.IsNullOrWhiteSpace(outputFilePath))
        {
            Console.Out.WriteLine(content);
        }
        else
        {
            await WriteToFileAsync(outputFilePath, content);
        }
    }

    /// <summary>
    /// Writes to a file, creating directories as needed.
    /// </summary>
    private static async Task WriteToFileAsync(string filePath, string content)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(filePath, content);
            Console.WriteLine($"Output written to: {Path.GetFullPath(filePath)}");
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to write output to {filePath}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets the supported output formats.
    /// </summary>
    public IEnumerable<string> GetSupportedFormats() => _formatterRegistry.GetSupportedFormats();

    /// <summary>
    /// Checks if a format is supported.
    /// </summary>
    public bool IsFormatSupported(string format) => _formatterRegistry.IsFormatSupported(format);
}
