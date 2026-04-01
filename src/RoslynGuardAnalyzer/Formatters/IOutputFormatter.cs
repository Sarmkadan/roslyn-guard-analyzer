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
/// </summary>
public interface IOutputFormatter
{
    /// <summary>
    /// Gets the format identifier (e.g., "json", "csv", "html").
    /// </summary>
    string Format { get; }

    /// <summary>
    /// Formats an analysis result into a formatted string.
    /// </summary>
    string FormatResult(AnalysisResult result);

    /// <summary>
    /// Formats a collection of violations into formatted output.
    /// </summary>
    string FormatViolations(IEnumerable<RuleViolation> violations);

    /// <summary>
    /// Formats a report object into formatted output.
    /// </summary>
    string FormatReport(ViolationReport report);

    /// <summary>
    /// Checks if this formatter can handle the given format identifier.
    /// </summary>
    bool CanFormat(string format);
}
