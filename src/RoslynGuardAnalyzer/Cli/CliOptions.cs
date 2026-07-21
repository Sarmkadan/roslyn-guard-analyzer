#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using RoslynGuardAnalyzer.Core;

namespace RoslynGuardAnalyzer.Cli;

/// <summary>
/// Represents parsed command-line options for the analyzer.
/// Uses validation to ensure options are internally consistent before use.
/// </summary>
public sealed class CliOptions
{
    public string? ProjectPath { get; set; }
    public string? FilePath { get; set; }
    public string OutputFormat { get; set; } = "text";
    public string? OutputFile { get; set; }
    public bool Verbose { get; set; }
    public bool ShowHelp { get; set; }
    public bool ShowVersion { get; set; }
    public int MaxParallelThreads { get; set; } = Environment.ProcessorCount;
    public int AnalysisTimeoutSeconds { get; set; } = 300;
    public List<string> RuleFilter { get; set; } = [];
    public bool FailOnViolations { get; set; } = true;
    public string? ConfigFile { get; set; }
    public bool GenerateReport { get; set; } = true;
    public string ReportType { get; set; } = "summary";
    public bool SkipCache { get; set; }
    public int LogLevel { get; set; } = 2;
    public string? BaselineFile { get; set; }
    public bool CreateBaseline { get; set; }

    /// <summary>
    /// New option: specifies the minimum severity that will cause a non‑zero exit code.
    /// If null, the original behavior (any violation) is used.
    /// </summary>
    public SeverityLevel? FailOnSeverity { get; set; }

    /// <summary>
    /// Validates that the options are mutually consistent and required fields are set.
    /// Project/file paths are mutually exclusive; at least one must be specified for analysis.
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = [];

        // Show help/version requires no other arguments
        if (ShowHelp || ShowVersion)
            return true;

        // Exactly one of project or file path must be specified
        var hasProject = !string.IsNullOrWhiteSpace(ProjectPath);
        var hasFile = !string.IsNullOrWhiteSpace(FilePath);

        if (!hasProject && !hasFile)
            errors.Add("Either --project or --file must be specified");

        if (hasProject && hasFile)
            errors.Add("Cannot specify both --project and --file");

        // Validate timeout
        if (AnalysisTimeoutSeconds <= 0)
            errors.Add("Analysis timeout must be greater than 0");

        // Validate parallel threads
        if (MaxParallelThreads <= 0)
            errors.Add("Parallel threads must be greater than 0");

        // Validate log level
        if (LogLevel < 0 || LogLevel > 4)
            errors.Add("Log level must be between 0 (silent) and 4 (debug)");

        // Validate output format
        var validFormats = new[] { "text", "json", "csv", "html", "xml", "sarif" };
        if (!validFormats.Contains(OutputFormat.ToLowerInvariant()))
            errors.Add($"Invalid output format. Supported: {string.Join(", ", validFormats)}");

        // Validate baseline file if specified
        if (!string.IsNullOrWhiteSpace(BaselineFile))
        {
            var fileName = Path.GetFileName(BaselineFile);
            if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                errors.Add("Baseline file name contains invalid characters");
        }

        // Validate --fail-on severity if provided
        if (FailOnSeverity.HasValue)
        {
            // The enum parsing is already done by the CLI binder; just ensure the value is defined.
            // No additional checks needed because any defined enum value is acceptable.
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// Determines if the options are for analysis mode (requires project or file).
    /// </summary>
    public bool IsAnalysisMode => !ShowHelp && !ShowVersion &&
        (!string.IsNullOrWhiteSpace(ProjectPath) ||
         !string.IsNullOrWhiteSpace(FilePath));

    /// <summary>
    /// Gets the target path for analysis (project or file).
    /// </summary>
    public string? GetTargetPath() => !string.IsNullOrWhiteSpace(ProjectPath) ? ProjectPath : FilePath;

    /// <summary>
    /// Returns true if a violation with the given severity should cause a failure
    /// based on the current --fail-on setting.
    /// </summary>
    public bool ShouldFailForSeverity(SeverityLevel violationSeverity)
    {
        // If no explicit severity threshold is set, fall back to the original behavior.
        if (!FailOnSeverity.HasValue)
            return FailOnViolations;

        // Fail only when the violation severity is greater than or equal to the threshold.
        return violationSeverity >= FailOnSeverity.Value;
    }

    /// <summary>
    /// Creates a summary string of the options for logging.
    /// </summary>
    public override string ToString()
    {
        return $"CliOptions {{ " +
        $"ProjectPath={ProjectPath}, " +
        $"FilePath={FilePath}, " +
        $"OutputFormat={OutputFormat}, " +
        $"Verbose={Verbose}, " +
        $"MaxParallelThreads={MaxParallelThreads}, " +
        $"AnalysisTimeoutSeconds={AnalysisTimeoutSeconds}, " +
        $"RuleFilterCount={RuleFilter.Count}, " +
        $"BaselineFile={BaselineFile}, " +
        $"CreateBaseline={CreateBaseline}, " +
        $"FailOnSeverity={FailOnSeverity?.ToString() ?? \"<none>\"} " +
        $"}};";
    }
}
