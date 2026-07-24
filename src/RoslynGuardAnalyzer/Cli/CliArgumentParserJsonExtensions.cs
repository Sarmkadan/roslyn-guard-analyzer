#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using RoslynGuardAnalyzer.Utilities;

namespace RoslynGuardAnalyzer.Cli;

/// <summary>
/// Provides System.Text.Json serialization extensions for CliArgumentParser.
/// </summary>
public static class CliArgumentParserJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = JsonIoHelper.CreateOptions(false);

    /// <summary>
    /// Serializes the CliArgumentParser to a JSON string.
    /// </summary>
    /// <param name="value">The CliArgumentParser instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the CliArgumentParser.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this CliArgumentParser value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value.Parse(), options);
    }

    /// <summary>
    /// Deserializes a JSON string to a CliArgumentParser instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A CliArgumentParser instance if successful; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the JSON input exceeds size limits or contains invalid data.</exception>
    public static CliArgumentParser? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        JsonIoHelper.ValidateJsonSize(json, nameof(json));

        var options = JsonSerializer.Deserialize<CliOptions>(json, _jsonOptions);
        return options is not null ? new CliArgumentParser(ToArgs(options)) : null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a CliArgumentParser instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized CliArgumentParser if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the JSON input exceeds size limits.</exception>
    public static bool TryFromJson(string json, out CliArgumentParser? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        try
        {
            JsonIoHelper.ValidateJsonSize(json, nameof(json));

            var options = JsonSerializer.Deserialize<CliOptions>(json, _jsonOptions);
            if (options is not null)
            {
                value = new CliArgumentParser(ToArgs(options));
                return true;
            }

            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Reconstructs an equivalent argument array from a <see cref="CliOptions"/> instance so that
    /// re-parsing it via <see cref="CliArgumentParser"/> yields the same effective options.
    /// </summary>
    /// <param name="options">The CliOptions instance to convert to command-line arguments.</param>
    /// <returns>An array of command-line arguments representing the provided options.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    private static string[] ToArgs(CliOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var args = new List<string>();

        if (options.ShowHelp)
            args.Add("--help");
        if (options.ShowVersion)
            args.Add("--version");
        if (options.Verbose)
            args.Add("--verbose");
        if (options.SkipCache)
            args.Add("--skip-cache");
        if (!string.IsNullOrEmpty(options.ProjectPath))
            args.Add($"--project={options.ProjectPath}");
        if (!string.IsNullOrEmpty(options.FilePath))
            args.Add($"--file={options.FilePath}");
        if (!string.IsNullOrEmpty(options.OutputFile))
            args.Add($"--output={options.OutputFile}");
        if (!string.IsNullOrEmpty(options.OutputFormat))
            args.Add($"--format={options.OutputFormat}");
        if (!string.IsNullOrEmpty(options.ConfigFile))
            args.Add($"--config={options.ConfigFile}");

        args.Add($"--timeout={options.AnalysisTimeoutSeconds.ToString(CultureInfo.InvariantCulture)}");
        args.Add($"--threads={options.MaxParallelThreads.ToString(CultureInfo.InvariantCulture)}");
        args.Add($"--log-level={options.LogLevel.ToString(CultureInfo.InvariantCulture)}");

        if (options.RuleFilter is not null && options.RuleFilter.Count > 0)
            args.Add($"--rule-filter={string.Join(",", options.RuleFilter)}");
        if (!options.FailOnViolations)
            args.Add("--no-fail-on-violations");
        if (!options.GenerateReport)
            args.Add("--no-report");
        if (!string.IsNullOrEmpty(options.ReportType))
            args.Add($"--report-type={options.ReportType}");

        return args.ToArray();
    }
}