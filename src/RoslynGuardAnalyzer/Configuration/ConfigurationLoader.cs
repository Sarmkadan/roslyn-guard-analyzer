// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Utilities;

namespace RoslynGuardAnalyzer.Configuration;

/// <summary>
/// Loads and parses configuration files for the analyzer.
/// Supports JSON format with fallback to defaults.
/// </summary>
public sealed class ConfigurationLoader
{
    private const string DefaultConfigFileName = ".roslyn-guard.json";

    /// <summary>
    /// Loads configuration from a specific file path.
    /// </summary>
    public static async Task<AnalysisConfig> LoadFromFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Configuration file not found: {filePath}");

        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            return ParseConfigurationJson(content);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load configuration from {filePath}", ex);
        }
    }

    /// <summary>
    /// Searches for a default configuration file in the project directory and parents.
    /// </summary>
    public static async Task<AnalysisConfig?> TryLoadDefaultAsync(string projectPath)
    {
        var directory = new DirectoryInfo(projectPath);

        while (directory != null)
        {
            var configPath = Path.Combine(directory.FullName, DefaultConfigFileName);

            if (File.Exists(configPath))
            {
                try
                {
                    return await LoadFromFileAsync(configPath);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Warning: Failed to load default config from {configPath}: {ex.Message}");
                    return null;
                }
            }

            directory = directory.Parent;
        }

        return null;
    }

    /// <summary>
    /// Parses a JSON configuration string into an AnalysisConfig object.
    /// </summary>
    private static AnalysisConfig ParseConfigurationJson(string json)
    {
        var config = new AnalysisConfig();

        // Simple JSON parsing (without external dependencies)
        if (json.Contains("\"enabledRules\""))
        {
            var rulesSection = ExtractJsonArray(json, "enabledRules");
            foreach (var rule in rulesSection)
            {
                var trimmed = rule.Trim().Trim('"');
                if (!string.IsNullOrEmpty(trimmed))
                    config.EnabledRules.Add(trimmed);
            }
        }

        if (json.Contains("\"excludePatterns\""))
        {
            var patternsSection = ExtractJsonArray(json, "excludePatterns");
            foreach (var pattern in patternsSection)
            {
                var trimmed = pattern.Trim().Trim('"');
                if (!string.IsNullOrEmpty(trimmed))
                    config.ExcludePatterns.Add(trimmed);
            }
        }

        if (json.Contains("\"severity\""))
        {
            config.MinimumSeverity = ExtractJsonValue(json, "severity").Trim('"');
        }

        if (json.Contains("\"maxViolations\""))
        {
            var value = ExtractJsonValue(json, "maxViolations");
            if (int.TryParse(value, out var max))
                config.MaxViolationsToReport = max;
        }

        if (json.Contains("\"enableCaching\""))
        {
            var value = ExtractJsonValue(json, "enableCaching").ToLower();
            config.EnableCaching = value == "true";
        }

        if (json.Contains("\"outputFormat\""))
        {
            config.OutputFormat = ExtractJsonValue(json, "outputFormat").Trim('"');
        }

        return config;
    }

    /// <summary>
    /// Extracts a JSON array value.
    /// </summary>
    private static List<string> ExtractJsonArray(string json, string key)
    {
        var result = new List<string>();
        var pattern = $"\"{key}\"\\s*:\\s*\\[";

        var startIndex = json.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
        if (startIndex == -1)
            return result;

        startIndex = json.IndexOf('[', startIndex);
        var endIndex = json.IndexOf(']', startIndex);

        if (endIndex == -1)
            return result;

        var arrayContent = json.Substring(startIndex + 1, endIndex - startIndex - 1);
        var items = arrayContent.Split(',');

        foreach (var item in items)
        {
            var trimmed = item.Trim();
            if (!string.IsNullOrEmpty(trimmed))
                result.Add(trimmed);
        }

        return result;
    }

    /// <summary>
    /// Extracts a JSON value.
    /// </summary>
    private static string ExtractJsonValue(string json, string key)
    {
        var pattern = $"\"{key}\"\\s*:\\s*";
        var startIndex = json.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);

        if (startIndex == -1)
            return string.Empty;

        startIndex = json.IndexOf(':', startIndex) + 1;

        // Skip whitespace
        while (startIndex < json.Length && char.IsWhiteSpace(json[startIndex]))
            startIndex++;

        var endIndex = startIndex;

        if (json[startIndex] == '"')
        {
            endIndex = json.IndexOf('"', startIndex + 1);
            if (endIndex == -1)
                endIndex = json.Length;

            return json.Substring(startIndex, endIndex - startIndex + 1);
        }

        // For numbers and booleans
        while (endIndex < json.Length && !char.IsWhiteSpace(json[endIndex]) && json[endIndex] != ',' && json[endIndex] != '}')
            endIndex++;

        return json.Substring(startIndex, endIndex - startIndex);
    }
}

/// <summary>
/// Configuration data for analysis rules and execution.
/// </summary>
public sealed class AnalysisConfig
{
    public List<string> EnabledRules { get; } = [];
    public List<string> ExcludePatterns { get; } = [];
    public string MinimumSeverity { get; set; } = "Low";
    public int MaxViolationsToReport { get; set; } = 1000;
    public bool EnableCaching { get; set; } = true;
    public string OutputFormat { get; set; } = "text";

    /// <summary>
    /// Validates the configuration is internally consistent.
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = [];

        var validSeverities = new[] { "Low", "Medium", "High", "Critical" };
        if (!validSeverities.Contains(MinimumSeverity, StringComparer.OrdinalIgnoreCase))
            errors.Add($"Invalid minimum severity: {MinimumSeverity}");

        if (MaxViolationsToReport <= 0)
            errors.Add("Max violations to report must be greater than 0");

        var validFormats = new[] { "text", "json", "csv", "html", "xml" };
        if (!validFormats.Contains(OutputFormat, StringComparer.OrdinalIgnoreCase))
            errors.Add($"Invalid output format: {OutputFormat}");

        return errors.Count == 0;
    }
}
