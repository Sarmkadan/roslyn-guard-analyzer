#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Formatters;

/// <summary>
/// Defines the contract for formatting analysis results into different output formats.
/// Implementations provide specialized output for JSON, CSV, HTML, XML, etc.
/// Formatters are pure transformations: every method returns the rendered text and performs no I/O.
/// Writing the returned string to a file or stdout is the caller's responsibility.
/// </summary>
public interface IOutputFormatter
{
    /// <summary>
    /// Gets the format identifier (e.g., "json", "csv", "html").
    /// The identifier is matched case-insensitively by <see cref="CanFormat"/> and is used as the
    /// key in <c>FormatterRegistry</c>.
    /// </summary>
    string Format { get; }

    /// <summary>
    /// Formats an analysis result into a formatted string.
    /// The output contains the result's violations, and project metadata where the format supports it.
    /// </summary>
    /// <param name="result">The analysis result to format. Must not be <c>null</c>.</param>
    /// <returns>The rendered document in this formatter's format, as a string. Nothing is written to disk.</returns>
    string FormatResult(AnalysisResult result);

    /// <summary>
    /// Formats a collection of violations into formatted output.
    /// No project-level metadata is included beyond what the format requires for a violations-only report.
    /// </summary>
    /// <param name="violations">The violations to format. Must not be <c>null</c>; may be empty.</param>
    /// <returns>The rendered document containing only the given violations, as a string. Nothing is written to disk.</returns>
    string FormatViolations(IEnumerable<RuleViolation> violations);

    /// <summary>
    /// Formats a report object into formatted output.
    /// Violations are taken from the report's violation groups; the report title and project name are included where the format supports them.
    /// </summary>
    /// <param name="report">The report to format. Must not be <c>null</c>.</param>
    /// <returns>The rendered report in this formatter's format, as a string. Nothing is written to disk.</returns>
    string FormatReport(ViolationReport report);

    /// <summary>
    /// Checks if this formatter can handle the given format identifier.
    /// The comparison is case-insensitive. Returns <c>false</c> for a non-matching identifier;
    /// implementations may throw <see cref="ArgumentException"/> for a <c>null</c> or empty identifier.
    /// </summary>
    /// <param name="format">The format identifier to test, such as "json" or "sarif".</param>
    /// <returns><c>true</c> if this formatter produces the given format; otherwise <c>false</c>.</returns>
    bool CanFormat(string format);
}
