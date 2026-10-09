#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RoslynGuardAnalyzer.Configuration;

/// <summary>
/// Validates configuration objects for consistency and correctness.
/// Checks rules, paths, and parameter values.
/// </summary>
public sealed class ConfigurationValidator
{
    // Validation message constants
    private const string ConfigNullError = "Configuration cannot be null";
    private const string OptionsNullError = "Options cannot be null";
    private const string RuleNamesNullError = "Rule names and supported rules cannot be null";
    private const string InvalidMinSeverityFormat = "Invalid minimum severity: {0}";
    private const string MaxViolationsMustBePositive = "Max violations must be greater than 0";
    private const string MaxViolationsLowWarning = "Max violations is very low, may limit report completeness";
    private const string InvalidOutputFormat = "Invalid output format: {0}";
    private const string NoRulesEnabledWarning = "No rules are explicitly enabled";
    private const string DuplicateRulesWarning = "Duplicate rules found: {0}";
    private const string EmptyExcludePatternError = "Exclude patterns cannot contain empty values";
    private const string ProjectPathNotFound = "Project path not found: {0}";
    private const string FileNotFound = "File not found: {0}";
    private const string ConfigFileNotFound = "Config file not found: {0}";
    private const string AnalysisTimeoutMustBePositive = "Analysis timeout must be positive";
    private const string MaxParallelThreadsMustBePositive = "Max parallel threads must be positive";
    private const string MaxParallelThreadsExceedsReasonable = "Max parallel threads ({0}) exceeds reasonable count";
    private const string UnknownRuleError = "Unknown rule: {0}";

    // Validation value constants
    private static readonly string[] ValidSeverities = { "Low", "Medium", "High", "Critical" };
    private static readonly string[] ValidFormats = { "text", "json", "csv", "html", "xml" };
    /// <summary>
    /// Validation result containing success status and error messages.
    /// </summary>
    public sealed class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; } = [];
        public List<string> Warnings { get; } = [];

        public void AddError(string message)
        {
            Errors.Add(message);
            IsValid = false;
        }

        public void AddWarning(string message)
        {
            Warnings.Add(message);
        }

        public override string ToString()
        {
            var lines = new List<string>();

            if (IsValid)
                lines.Add("✓ Configuration is valid");
            else
                lines.Add("✗ Configuration has errors");

            if (Errors.Count > 0)
            {
                lines.Add($"Errors ({Errors.Count}):");
                lines.AddRange(Errors.Select(e => $"  - {e}"));
            }

            if (Warnings.Count > 0)
            {
                lines.Add($"Warnings ({Warnings.Count}):");
                lines.AddRange(Warnings.Select(w => $"  ! {w}"));
            }

            return string.Join(Environment.NewLine, lines);
        }
    }

    /// <summary>
    /// Validates an analysis configuration.
    /// </>
    public static ValidationResult ValidateAnalysisConfig(AnalysisConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        var result = new ValidationResult { IsValid = true };

        if (config is null)
        {
            result.AddError(ConfigNullError);
            return result;
        }

        // Validate severity
        if (!ValidSeverities.Contains(config.MinimumSeverity, StringComparer.OrdinalIgnoreCase))
            result.AddError(string.Format(InvalidMinSeverityFormat, config.MinimumSeverity));

        // Validate max violations
        if (config.MaxViolationsToReport <= 0)
            result.AddError(MaxViolationsMustBePositive);

        if (config.MaxViolationsToReport < 10)
            result.AddWarning(MaxViolationsLowWarning);

        // Validate output format
        if (!ValidFormats.Contains(config.OutputFormat, StringComparer.OrdinalIgnoreCase))
            result.AddError(string.Format(InvalidOutputFormat, config.OutputFormat));

        // Validate rule names
        if (config.EnabledRules.Count == 0)
            result.AddWarning(NoRulesEnabledWarning);

        // Check for duplicate rules
        var duplicates = config.EnabledRules
            .GroupBy(r => r, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
            result.AddWarning(string.Format(DuplicateRulesWarning, string.Join(", ", duplicates)));

        // Validate patterns
        if (config.ExcludePatterns.Count > 0)
        {
            foreach (var pattern in config.ExcludePatterns)
            {
                if (string.IsNullOrWhiteSpace(pattern))
                    result.AddError(EmptyExcludePatternError);
            }
        }

        return result;
    }

    /// <summary>
    /// Validates CLI options.
    /// </summary>
    public static ValidationResult ValidateCliOptions(Cli.CliOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var result = new ValidationResult { IsValid = true };

        if (options is null)
        {
            result.AddError(OptionsNullError);
            return result;
        }

        // Check paths exist if specified
        if (!string.IsNullOrEmpty(options.ProjectPath))
        {
            if (!File.Exists(options.ProjectPath) && !Directory.Exists(options.ProjectPath))
                result.AddError(string.Format(ProjectPathNotFound, options.ProjectPath));
        }

        if (!string.IsNullOrEmpty(options.FilePath))
        {
            if (!File.Exists(options.FilePath))
                result.AddError(string.Format(FileNotFound, options.FilePath));
        }

        if (!string.IsNullOrEmpty(options.ConfigFile))
        {
            if (!File.Exists(options.ConfigFile))
                result.AddError(string.Format(ConfigFileNotFound, options.ConfigFile));
        }

        // Validate numeric options
        if (options.AnalysisTimeoutSeconds <= 0)
            result.AddError(AnalysisTimeoutMustBePositive);

        if (options.MaxParallelThreads <= 0)
            result.AddError(MaxParallelThreadsMustBePositive);

        if (options.MaxParallelThreads > Environment.ProcessorCount * 2)
            result.AddWarning(string.Format(MaxParallelThreadsExceedsReasonable, options.MaxParallelThreads));

        return result;
    }

    /// <summary>
    /// Validates that all rules are known/supported.
    /// </summary>
    public static ValidationResult ValidateRuleNames(
        IEnumerable<string> ruleNames,
        IEnumerable<string> supportedRules)
    {
        ArgumentNullException.ThrowIfNull(ruleNames);
        ArgumentNullException.ThrowIfNull(supportedRules);
        var result = new ValidationResult { IsValid = true };

        if (ruleNames is null || supportedRules is null)
        {
            result.AddError(RuleNamesNullError);
            return result;
        }

        var supported = new HashSet<string>(supportedRules, StringComparer.OrdinalIgnoreCase);

        foreach (var rule in ruleNames)
        {
            if (!supported.Contains(rule))
                result.AddError(string.Format(UnknownRuleError, rule));
        }

        return result;
    }

    /// <summary>
    /// Performs a comprehensive validation of all configuration components.
    /// </summary>
    public static ValidationResult ValidateComprehensive(
        AnalysisConfig? analysisConfig,
        Cli.CliOptions? cliOptions)
    {
        var results = new List<ValidationResult>();

        if (analysisConfig is not null)
            results.Add(ValidateAnalysisConfig(analysisConfig));

        if (cliOptions is not null)
            results.Add(ValidateCliOptions(cliOptions));

        var combined = new ValidationResult { IsValid = true };

        foreach (var result in results)
        {
            combined.Errors.AddRange(result.Errors);
            combined.Warnings.AddRange(result.Warnings);
            if (!result.IsValid)
                combined.IsValid = false;
        }

        return combined;
    }
}
