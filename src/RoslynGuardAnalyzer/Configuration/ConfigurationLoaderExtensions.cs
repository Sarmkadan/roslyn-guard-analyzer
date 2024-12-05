#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Utilities;

namespace RoslynGuardAnalyzer.Configuration;

/// <summary>
/// Extension methods for <see cref="ConfigurationLoader"/> to provide additional utility functionality.
/// </summary>
public static class ConfigurationLoaderExtensions
{
    /// <summary>
    /// Loads configuration from a specific file path with validation.
    /// </summary>
    /// <param name="loader">The configuration loader instance.</param>
    /// <param name="filePath">Path to the configuration file.</param>
    /// <param name="validate">Whether to validate the loaded configuration.</param>
    /// <returns>The loaded configuration.</returns>
    /// <exception cref="ArgumentException">Thrown when file path is invalid.</exception>
    /// <exception cref="FileNotFoundException">Thrown when configuration file doesn't exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when configuration parsing fails.</exception>
    public static async Task<AnalysisConfig> LoadFromFileAsync(
        this ConfigurationLoader loader,
        string filePath,
        bool validate = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Configuration file not found: {filePath}");

        var config = await ConfigurationLoader.LoadFromFileAsync(filePath);

        if (validate && !config.Validate(out var errors))
        {
            throw new InvalidOperationException(
                $"Configuration validation failed:\n{string.Join("\n", errors)}");
        }

        return config;
    }

    /// <summary>
    /// Searches for a default configuration file and merges it with provided configuration.
    /// </summary>
    /// <param name="loader">The configuration loader instance.</param>
    /// <param name="baseConfig">The base configuration to merge with defaults.</param>
    /// <param name="projectPath">Path to search for default configuration.</param>
    /// <returns>Merged configuration with defaults applied where not specified.</returns>
    public static async Task<AnalysisConfig> MergeWithDefaultAsync(
        this ConfigurationLoader loader,
        AnalysisConfig baseConfig,
        string projectPath)
    {
        if (baseConfig is null)
            throw new ArgumentNullException(nameof(baseConfig));

        if (string.IsNullOrWhiteSpace(projectPath))
            throw new ArgumentException("Project path cannot be null or empty", nameof(projectPath));

        var defaultConfig = await ConfigurationLoader.TryLoadDefaultAsync(projectPath);

        if (defaultConfig is null)
            return baseConfig;

        // Merge configurations: base config takes precedence over defaults
        var merged = new AnalysisConfig
        {
            MinimumSeverity = baseConfig.MinimumSeverity ?? defaultConfig.MinimumSeverity,
            MaxViolationsToReport = baseConfig.MaxViolationsToReport != 0
                ? baseConfig.MaxViolationsToReport
                : defaultConfig.MaxViolationsToReport,
            EnableCaching = baseConfig.EnableCaching != default
                ? baseConfig.EnableCaching
                : defaultConfig.EnableCaching,
            OutputFormat = !string.IsNullOrWhiteSpace(baseConfig.OutputFormat)
                ? baseConfig.OutputFormat
                : defaultConfig.OutputFormat
        };

        // Merge lists: base config values take precedence
        foreach (var rule in defaultConfig.EnabledRules)
        {
            if (!merged.EnabledRules.Contains(rule))
                merged.EnabledRules.Add(rule);
        }
        foreach (var rule in baseConfig.EnabledRules)
        {
            if (!merged.EnabledRules.Contains(rule))
                merged.EnabledRules.Add(rule);
        }

        foreach (var pattern in defaultConfig.ExcludePatterns)
        {
            if (!merged.ExcludePatterns.Contains(pattern))
                merged.ExcludePatterns.Add(pattern);
        }
        foreach (var pattern in baseConfig.ExcludePatterns)
        {
            if (!merged.ExcludePatterns.Contains(pattern))
                merged.ExcludePatterns.Add(pattern);
        }

        return merged;
    }

    /// <summary>
    /// Checks if a specific rule is enabled in the configuration.
    /// </summary>
    /// <param name="loader">The configuration loader instance.</param>
    /// <param name="config">The configuration to check.</param>
    /// <param name="ruleId">The rule identifier to check.</param>
    /// <returns>True if the rule is enabled; otherwise false.</returns>
    public static bool IsRuleEnabled(
        this ConfigurationLoader loader,
        AnalysisConfig config,
        string ruleId)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        if (string.IsNullOrWhiteSpace(ruleId))
            throw new ArgumentException("Rule ID cannot be null or empty", nameof(ruleId));

        if (config.EnabledRules.Count == 0)
            return true; // All rules enabled if none specified

        return config.EnabledRules.Contains(ruleId, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if a file path matches any of the exclude patterns.
    /// </summary>
    /// <param name="loader">The configuration loader instance.</param>
    /// <param name="config">The configuration containing exclude patterns.</param>
    /// <param name="filePath">The file path to check.</param>
    /// <returns>True if the file should be excluded; otherwise false.</returns>
    public static bool IsPathExcluded(
        this ConfigurationLoader loader,
        AnalysisConfig config,
        string filePath)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (config.ExcludePatterns.Count == 0)
            return false; // No patterns means nothing excluded

        var fileName = Path.GetFileName(filePath);
        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;

        foreach (var pattern in config.ExcludePatterns)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                continue;

            var normalizedPattern = pattern
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            if (normalizedPattern.Contains('*') || normalizedPattern.Contains('?'))
            {
                // Wildcard pattern matching using simple pattern matching
                var fileNameMatch = Path.GetFileName(normalizedPattern);
                if (SimpleMatch(fileNameMatch, fileName))
                    return true;

                if (SimpleMatch(normalizedPattern, filePath))
                    return true;
            }
            else
            {
                // Exact or prefix matching
                if (filePath.StartsWith(normalizedPattern, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (fileName.Equals(normalizedPattern, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Simple wildcard pattern matching implementation.
    /// </summary>
    private static bool SimpleMatch(string pattern, string input)
    {
        var patternIndex = 0;
        var inputIndex = 0;
        var patternLength = pattern.Length;
        var inputLength = input.Length;
        var starIndex = -1;
        var matchIndex = 0;

        while (inputIndex < inputLength)
        {
            if (patternIndex < patternLength && (pattern[patternIndex] == '?' || pattern[patternIndex] == input[inputIndex]))
            {
                patternIndex++;
                inputIndex++;
            }
            else if (patternIndex < patternLength && pattern[patternIndex] == '*')
            {
                starIndex = patternIndex;
                matchIndex = inputIndex;
                patternIndex++;
            }
            else if (starIndex != -1)
            {
                patternIndex = starIndex + 1;
                matchIndex++;
                inputIndex = matchIndex;
            }
            else
            {
                return false;
            }
        }

        while (patternIndex < patternLength && pattern[patternIndex] == '*')
        {
            patternIndex++;
        }

        return patternIndex == patternLength;
    }

    /// <summary>
    /// Creates a deep copy of the configuration.
    /// </summary>
    /// <param name="loader">The configuration loader instance.</param>
    /// <param name="config">The configuration to copy.</param>
    /// <returns>A new independent copy of the configuration.</returns>
    public static AnalysisConfig Clone(this ConfigurationLoader loader, AnalysisConfig config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        var clone = new AnalysisConfig();
        clone.EnabledRules.AddRange(config.EnabledRules);
        clone.ExcludePatterns.AddRange(config.ExcludePatterns);
        clone.MinimumSeverity = config.MinimumSeverity;
        clone.MaxViolationsToReport = config.MaxViolationsToReport;
        clone.EnableCaching = config.EnableCaching;
        clone.OutputFormat = config.OutputFormat;
        return clone;
    }

    /// <summary>
    /// Determines if caching should be enabled based on configuration and environment.
    /// </summary>
    /// <param name="loader">The configuration loader instance.</param>
    /// <param name="config">The configuration to check.</param>
    /// <param name="forceDisable">Whether to force disable caching regardless of config.</param>
    /// <returns>True if caching should be enabled; otherwise false.</returns>
    public static bool ShouldEnableCaching(
        this ConfigurationLoader loader,
        AnalysisConfig config,
        bool forceDisable = false)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        if (forceDisable)
            return false;

        return config.EnableCaching;
    }
}