#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using RoslynGuardAnalyzer.Cli;

namespace RoslynGuardAnalyzer.Configuration;

/// <summary>
/// Configuration options for Roslyn Guard Analyzer using IOptions pattern.
/// Provides strongly-typed configuration with validation via DataAnnotations.
/// </summary>
public sealed class RoslynGuardAnalyzerOptions
{
    /// <summary>
    /// Gets or sets the root project path for analysis.
    /// Default: current directory (./)
    /// </summary>
    [Display(Name = "Project Path")]
    public string ProjectPath { get; set; } = "./";

    /// <summary>
    /// Gets or sets the analysis timeout in seconds.
    /// Default: 600 seconds (10 minutes)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Timeout must be greater than 0")]
    [Display(Name = "Analysis Timeout (seconds)")]
    public int AnalysisTimeoutSeconds { get; set; } = 600;

    /// <summary>
    /// Gets or sets the maximum number of violations to report.
    /// Default: 1000
    /// </summary>
    [Range(1, 100000, ErrorMessage = "Max violations must be between 1 and 100000")]
    [Display(Name = "Max Violations to Report")]
    public int MaxViolationsToReport { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the log level (0=none, 1=errors, 2=warnings, 3=info, 4=debug).
    /// Default: 2 (warnings)
    /// </summary>
    [Range(0, 4, ErrorMessage = "Log level must be between 0 and 4")]
    [Display(Name = "Log Level")]
    public int LogLevel { get; set; } = 2;

    /// <summary>
    /// Gets or sets the output format (text, json, csv, html, xml).
    /// Default: text
    /// </summary>
    [RegularExpression(
        "^(text|json|csv|html|xml)$",
        ErrorMessage = "Output format must be one of: text, json, csv, html, xml")]
    [Display(Name = "Output Format")]
    public string OutputFormat { get; set; } = "text";

    /// <summary>
    /// Gets or sets the output file path.
    /// Default: null (console output)
    /// </summary>
    [Display(Name = "Output File Path")]
    public string? OutputFile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate a report.
    /// Default: true
    /// </summary>
    [Display(Name = "Generate Report")]
    public bool GenerateReport { get; set; } = true;

    /// <summary>
    /// Gets or sets the report type (summary, detailed, full).
    /// Default: summary
    /// </summary>
    [RegularExpression(
        "^(summary|detailed|full)$",
        ErrorMessage = "Report type must be one of: summary, detailed, full")]
    [Display(Name = "Report Type")]
    public string ReportType { get; set; } = "summary";

    /// <summary>
    /// Gets or sets a value indicating whether to fail on violations.
    /// Default: true
    /// </summary>
    [Display(Name = "Fail on Violations")]
    public bool FailOnViolations { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to skip caching.
    /// Default: false
    /// </summary>
    [Display(Name = "Skip Cache")]
    public bool SkipCache { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of parallel threads for analysis.
    /// Default: Environment.ProcessorCount
    /// </summary>
    [Range(1, 64, ErrorMessage = "Parallel threads must be between 1 and 64")]
    [Display(Name = "Max Parallel Threads")]
    public int MaxParallelThreads { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Gets or sets the list of rule IDs to execute (comma-separated or array).
    /// Default: empty (all rules)
    /// </summary>
    [Display(Name = "Rule Filter")]
    public List<string> RuleFilter { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of glob patterns to exclude from analysis.
    /// Default: ["**/bin/**", "**/obj/**", "**/*.Generated.cs", "**/*.Designer.cs"]
    /// </summary>
    [Display(Name = "Exclude Patterns")]
    public List<string> ExcludePatterns { get; set; } =
    [
        "**/bin/**",
        "**/obj/**",
        "**/*.Generated.cs",
        "**/*.Designer.cs"
    ];

    /// <summary>
    /// Gets or sets the minimum severity level to report (Low, Medium, High, Critical).
    /// Default: Low
    /// </summary>
    [RegularExpression(
        "^(Low|Medium|High|Critical)$",
        ErrorMessage = "Minimum severity must be one of: Low, Medium, High, Critical")]
    [Display(Name = "Minimum Severity")]
    public string MinimumSeverity { get; set; } = "Low";

    /// <summary>
    /// Gets or sets the configuration file path.
    /// Default: null (auto-discover .roslyn-guard.json)
    /// </summary>
    [Display(Name = "Configuration File")]
    public string? ConfigFile { get; set; }

    /// <summary>
    /// Validates the configuration using DataAnnotations validation.
    /// </summary>
    /// <returns>List of validation errors, empty if valid</returns>
    public List<string> Validate()
    {
        var validationContext = new ValidationContext(this);
        var validationResults = new List<ValidationResult>();
        var errors = new List<string>();

        if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
        {
            foreach (var validationResult in validationResults)
            {
                if (validationResult.ErrorMessage is not null)
                {
                    errors.Add(validationResult.ErrorMessage);
                }
            }
        }

        // Additional custom validation
        if (MaxViolationsToReport < 10)
        {
            errors.Add("Max violations is very low, may limit report completeness");
        }

        if (MaxParallelThreads > Environment.ProcessorCount * 2)
        {
            errors.Add(
                $"Max parallel threads ({MaxParallelThreads}) exceeds reasonable count " +
                $"(recommended: {Environment.ProcessorCount})"
            );
        }

        return errors;
    }

    /// <summary>
    /// Creates a summary string of the options for logging.
    /// </summary>
    public override string ToString()
    {
        return $"RoslynGuardAnalyzerOptions {{" +
               $" ProjectPath={ProjectPath}, " +
               $" AnalysisTimeoutSeconds={AnalysisTimeoutSeconds}, " +
               $" MaxViolationsToReport={MaxViolationsToReport}, " +
               $" LogLevel={LogLevel}, " +
               $" OutputFormat={OutputFormat}, " +
               $" GenerateReport={GenerateReport}, " +
               $" MaxParallelThreads={MaxParallelThreads}, " +
               $" RuleFilterCount={RuleFilter.Count}, " +
               $" ExcludePatternsCount={ExcludePatterns.Count}" +
               $"}}";
    }

    /// <summary>
    /// Merges this configuration with CLI options, giving CLI options priority.
    /// </summary>
    public void MergeWithCliOptions(Cli.CliOptions cliOptions)
    {
        if (cliOptions is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(cliOptions.ProjectPath))
        {
            ProjectPath = cliOptions.ProjectPath;
        }

        if (cliOptions.AnalysisTimeoutSeconds != 300) // Different default than CLI
        {
            AnalysisTimeoutSeconds = cliOptions.AnalysisTimeoutSeconds;
        }

        if (cliOptions.MaxParallelThreads != Environment.ProcessorCount)
        {
            MaxParallelThreads = cliOptions.MaxParallelThreads;
        }

        if (!string.IsNullOrWhiteSpace(cliOptions.OutputFormat) &&
            cliOptions.OutputFormat != "text")
        {
            OutputFormat = cliOptions.OutputFormat;
        }

        if (!string.IsNullOrWhiteSpace(cliOptions.OutputFile))
        {
            OutputFile = cliOptions.OutputFile;
        }

        if (cliOptions.GenerateReport != true) // Different default than CLI
        {
            GenerateReport = cliOptions.GenerateReport;
        }

        if (!string.IsNullOrWhiteSpace(cliOptions.ReportType) &&
            cliOptions.ReportType != "summary")
        {
            ReportType = cliOptions.ReportType;
        }

        if (cliOptions.FailOnViolations != true) // Different default than CLI
        {
            FailOnViolations = cliOptions.FailOnViolations;
        }

        if (cliOptions.SkipCache != false) // Different default than CLI
        {
            SkipCache = cliOptions.SkipCache;
        }

        if (cliOptions.MaxParallelThreads != Environment.ProcessorCount)
        {
            MaxParallelThreads = cliOptions.MaxParallelThreads;
        }

        if (cliOptions.RuleFilter.Count > 0)
        {
            RuleFilter = cliOptions.RuleFilter;
        }

        if (cliOptions.ConfigFile is not null)
        {
            ConfigFile = cliOptions.ConfigFile;
        }
    }
}
