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
    /// </summary>
    public static ValidationResult ValidateAnalysisConfig(AnalysisConfig config)
    {
        var result = new ValidationResult { IsValid = true };

        if (config == null)
        {
            result.AddError("Configuration cannot be null");
            return result;
        }

        // Validate severity
        var validSeverities = new[] { "Low", "Medium", "High", "Critical" };
        if (!validSeverities.Contains(config.MinimumSeverity, StringComparer.OrdinalIgnoreCase))
            result.AddError($"Invalid minimum severity: {config.MinimumSeverity}");

        // Validate max violations
        if (config.MaxViolationsToReport <= 0)
            result.AddError("Max violations must be greater than 0");

        if (config.MaxViolationsToReport < 10)
            result.AddWarning("Max violations is very low, may limit report completeness");

        // Validate output format
        var validFormats = new[] { "text", "json", "csv", "html", "xml" };
        if (!validFormats.Contains(config.OutputFormat, StringComparer.OrdinalIgnoreCase))
            result.AddError($"Invalid output format: {config.OutputFormat}");

        // Validate rule names
        if (config.EnabledRules.Count == 0)
            result.AddWarning("No rules are explicitly enabled");

        // Check for duplicate rules
        var duplicates = config.EnabledRules
            .GroupBy(r => r, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
            result.AddWarning($"Duplicate rules found: {string.Join(", ", duplicates)}");

        // Validate patterns
        if (config.ExcludePatterns.Count > 0)
        {
            foreach (var pattern in config.ExcludePatterns)
            {
                if (string.IsNullOrWhiteSpace(pattern))
                    result.AddError("Exclude patterns cannot contain empty values");
            }
        }

        return result;
    }

    /// <summary>
    /// Validates CLI options.
    /// </summary>
    public static ValidationResult ValidateCliOptions(Cli.CliOptions options)
    {
        var result = new ValidationResult { IsValid = true };

        if (options == null)
        {
            result.AddError("Options cannot be null");
            return result;
        }

        // Check paths exist if specified
        if (!string.IsNullOrEmpty(options.ProjectPath))
        {
            if (!File.Exists(options.ProjectPath) && !Directory.Exists(options.ProjectPath))
                result.AddError($"Project path not found: {options.ProjectPath}");
        }

        if (!string.IsNullOrEmpty(options.FilePath))
        {
            if (!File.Exists(options.FilePath))
                result.AddError($"File not found: {options.FilePath}");
        }

        if (!string.IsNullOrEmpty(options.ConfigFile))
        {
            if (!File.Exists(options.ConfigFile))
                result.AddError($"Config file not found: {options.ConfigFile}");
        }

        // Validate numeric options
        if (options.AnalysisTimeoutSeconds <= 0)
            result.AddError("Analysis timeout must be positive");

        if (options.MaxParallelThreads <= 0)
            result.AddError("Max parallel threads must be positive");

        if (options.MaxParallelThreads > Environment.ProcessorCount * 2)
            result.AddWarning($"Max parallel threads ({options.MaxParallelThreads}) exceeds reasonable count");

        return result;
    }

    /// <summary>
    /// Validates that all rules are known/supported.
    /// </summary>
    public static ValidationResult ValidateRuleNames(
        IEnumerable<string> ruleNames,
        IEnumerable<string> supportedRules)
    {
        var result = new ValidationResult { IsValid = true };

        if (ruleNames == null || supportedRules == null)
        {
            result.AddError("Rule names and supported rules cannot be null");
            return result;
        }

        var supported = new HashSet<string>(supportedRules, StringComparer.OrdinalIgnoreCase);

        foreach (var rule in ruleNames)
        {
            if (!supported.Contains(rule))
                result.AddError($"Unknown rule: {rule}");
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

        if (analysisConfig != null)
            results.Add(ValidateAnalysisConfig(analysisConfig));

        if (cliOptions != null)
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
