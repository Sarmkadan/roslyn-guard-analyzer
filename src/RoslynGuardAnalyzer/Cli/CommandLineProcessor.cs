#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace RoslynGuardAnalyzer.Cli;

/// <summary>
/// High-level CLI command processor that orchestrates parsing and validation.
/// Acts as a facade for CLI argument parsing and help text generation.
/// </summary>
public sealed class CommandLineProcessor
{
    private readonly string[] _args;
    private CliOptions? _parsedOptions;

    public CommandLineProcessor(string[] args)
    {
        _args = args ?? [];
    }

    /// <summary>
    /// Processes the command-line arguments and validates them.
    /// </summary>
    public (bool Success, CliOptions Options, List<string> Errors) Process()
    {
        var errors = new List<string>();

        try
        {
            _parsedOptions = CliArgumentParser.ParseSafe(_args);

            // Handle help/version first
            if (_parsedOptions.ShowHelp)
            {
                Console.WriteLine(HelpGenerator.GenerateFullHelp());
                return (true, _parsedOptions, errors);
            }

            if (_parsedOptions.ShowVersion)
            {
                Console.WriteLine(HelpGenerator.GenerateVersion());
                return (true, _parsedOptions, errors);
            }

            // Validate the options
            if (!_parsedOptions.Validate(out var validationErrors))
            {
                errors.AddRange(validationErrors);
                PrintErrors(errors);
                return (false, _parsedOptions, errors);
            }

            return (true, _parsedOptions, errors);
        }
        catch (Exception ex)
        {
            errors.Add($"Unexpected error: {ex.Message}");
            PrintErrors(errors);
            return (false, _parsedOptions ?? new CliOptions(), errors);
        }
    }

    /// <summary>
    /// Gets the parsed options (only valid after calling Process).
    /// </summary>
    public CliOptions GetOptions() => _parsedOptions ?? new CliOptions();

    /// <summary>
    /// Prints error messages in a user-friendly format.
    /// </summary>
    private static void PrintErrors(List<string> errors)
    {
        if (errors.Count == 0)
            return;

        Console.Error.WriteLine("Errors:");
        foreach (var error in errors)
        {
            Console.Error.WriteLine($"  - {error}");
        }

        Console.Error.WriteLine();
        Console.Error.WriteLine(HelpGenerator.GenerateUsageSummary());
    }

    /// <summary>
    /// Validates that all required files/paths exist.
    /// </summary>
    public (bool Valid, List<string> Errors) ValidatePaths()
    {
        var errors = new List<string>();

        if (_parsedOptions is null)
        {
            errors.Add("Options not processed yet");
            return (false, errors);
        }

        var targetPath = _parsedOptions.GetTargetPath();
        if (!string.IsNullOrEmpty(targetPath))
        {
            if (!System.IO.File.Exists(targetPath) && !System.IO.Directory.Exists(targetPath))
            {
                errors.Add($"Path not found: {targetPath}");
            }
        }

        if (!string.IsNullOrEmpty(_parsedOptions.ConfigFile))
        {
            if (!System.IO.File.Exists(_parsedOptions.ConfigFile))
            {
                errors.Add($"Config file not found: {_parsedOptions.ConfigFile}");
            }
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Prints a summary of the parsed options.
    /// </summary>
    public void PrintOptionsSummary()
    {
        if (_parsedOptions is null)
            return;

        Console.WriteLine("Configuration:");
        Console.WriteLine($"  Target: {_parsedOptions.GetTargetPath()}");
        Console.WriteLine($"  Format: {_parsedOptions.OutputFormat}");
        Console.WriteLine($"  Timeout: {_parsedOptions.AnalysisTimeoutSeconds}s");
        Console.WriteLine($"  Threads: {_parsedOptions.MaxParallelThreads}");

        if (_parsedOptions.RuleFilter.Count > 0)
        {
            Console.WriteLine($"  Filtered Rules: {string.Join(", ", _parsedOptions.RuleFilter)}");
        }

        Console.WriteLine();
    }
}
