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
/// Exit codes returned by the analyzer.
/// </summary>
public enum ExitCode : int
{
    /// <summary>Clean execution with no violations.</summary>
    Success = 0,

    /// <summary>Violations found (when --strict is enabled).</summary>
    ViolationsFound = 1,

    /// <summary>Bad arguments or validation errors.</summary>
    BadArguments = 2,

    /// <summary>Internal error or unhandled exception.</summary>
    InternalError = 3
}

/// <summary>
/// High-level CLI command processor that orchestrates parsing and validation.
/// Acts as a facade for CLI argument parsing and help text generation.
/// </summary>
public sealed class CommandLineProcessor
{
    private const string UnexpectedErrorPrefix = "Unexpected error: ";
    private const string ErrorsHeader = "Errors:";
    private const string ErrorItemPrefix = " - ";
    private const string OptionsNotProcessedError = "Options not processed yet";
    private const string PathNotFoundPrefix = "Path not found: ";
    private const string ConfigFileNotFoundPrefix = "Config file not found: ";
    private const string ConfigurationHeader = "Configuration:";
    private const string TargetLabel = " Target: ";
    private const string FormatLabel = " Format: ";
    private const string TimeoutLabel = " Timeout: ";
    private const string SecondsSuffix = "s";
    private const string ThreadsLabel = " Threads: ";
    private const string FilteredRulesLabel = " Filtered Rules: ";
    private const string RuleSeparator = ", ";

    private readonly string[] _args;
    private CliOptions? _parsedOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineProcessor"/> class.
    /// </summary>
    public CommandLineProcessor(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        _args = args;
    }

    /// <summary>
    /// Processes the command-line arguments and validates them.
    /// </summary>
    public (bool Success, ExitCode ExitCode, CliOptions Options, List<string> Errors) Process()
    {
        var errors = new List<string>();

        try
        {
            _parsedOptions = CliArgumentParser.ParseSafe(_args);

            // Handle help/version first
            if (_parsedOptions.ShowHelp)
            {
                Console.WriteLine(HelpGenerator.GenerateFullHelp());
                return (true, ExitCode.Success, _parsedOptions, errors);
            }

            if (_parsedOptions.ShowVersion)
            {
                Console.WriteLine(HelpGenerator.GenerateVersion());
                return (true, ExitCode.Success, _parsedOptions, errors);
            }

            // Validate the options
            if (!_parsedOptions.Validate(out var validationErrors))
            {
                errors.AddRange(validationErrors);
                PrintErrors(errors);
                return (false, ExitCode.BadArguments, _parsedOptions, errors);
            }

            return (true, ExitCode.Success, _parsedOptions, errors);
        }
        catch (Exception ex)
        {
            errors.Add(UnexpectedErrorPrefix + ex.Message);
            PrintErrors(errors);
            return (false, ExitCode.InternalError, _parsedOptions ?? new CliOptions(), errors);
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
        {
            return;
        }

        Console.Error.WriteLine(ErrorsHeader);
        foreach (var error in errors)
        {
            Console.Error.WriteLine(ErrorItemPrefix + error);
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
            errors.Add(OptionsNotProcessedError);
            return (false, errors);
        }

        var targetPath = _parsedOptions.GetTargetPath();
        if (!string.IsNullOrEmpty(targetPath))
        {
            if (!System.IO.File.Exists(targetPath) && !System.IO.Directory.Exists(targetPath))
            {
                errors.Add(PathNotFoundPrefix + targetPath);
            }
        }

        if (!string.IsNullOrEmpty(_parsedOptions.ConfigFile))
        {
            if (!System.IO.File.Exists(_parsedOptions.ConfigFile))
            {
                errors.Add(ConfigFileNotFoundPrefix + _parsedOptions.ConfigFile);
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
        {
            return;
        }

        Console.WriteLine(ConfigurationHeader);
        Console.WriteLine(TargetLabel + _parsedOptions.GetTargetPath());
        Console.WriteLine(FormatLabel + _parsedOptions.OutputFormat);
        Console.WriteLine(TimeoutLabel + _parsedOptions.AnalysisTimeoutSeconds + SecondsSuffix);
        Console.WriteLine(ThreadsLabel + _parsedOptions.MaxParallelThreads);

        if (_parsedOptions.RuleFilter.Count > 0)
        {
            Console.WriteLine(FilteredRulesLabel + string.Join(RuleSeparator, _parsedOptions.RuleFilter));
        }

        Console.WriteLine();
    }
}
