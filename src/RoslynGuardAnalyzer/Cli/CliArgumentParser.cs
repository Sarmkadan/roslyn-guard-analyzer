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
/// Parses command-line arguments into a CliOptions object.
/// Uses a state machine approach to handle flags, options, and positional arguments.
/// Supports both --option=value and --option value formats.
/// </summary>
public sealed class CliArgumentParser
{
    private readonly string[] _args;
    private int _index;

    public CliArgumentParser(string[] args)
    {
        _args = args ?? [];
    }

    /// <summary>
    /// Parses the command-line arguments and returns a CliOptions object.
    /// </summary>
    public CliOptions Parse()
    {
        var options = new CliOptions();
        _index = 0;

        while (_index < _args.Length)
        {
            var arg = _args[_index];

            if (arg == "-h" || arg == "--help")
            {
                options.ShowHelp = true;
                _index++;
            }
            else if (arg == "-v" || arg == "--version")
            {
                options.ShowVersion = true;
                _index++;
            }
            else if (arg == "--verbose")
            {
                options.Verbose = true;
                _index++;
            }
            else if (arg == "--skip-cache")
            {
                options.SkipCache = true;
                _index++;
            }
            else if (arg.StartsWith("--project="))
            {
                options.ProjectPath = arg.Substring(10);
                _index++;
            }
            else if (arg == "--project")
            {
                options.ProjectPath = GetNextValue("--project");
                _index++;
            }
            else if (arg.StartsWith("--file="))
            {
                options.FilePath = arg.Substring(7);
                _index++;
            }
            else if (arg == "--file")
            {
                options.FilePath = GetNextValue("--file");
                _index++;
            }
            else if (arg.StartsWith("--output="))
            {
                options.OutputFile = arg.Substring(9);
                _index++;
            }
            else if (arg == "--output")
            {
                options.OutputFile = GetNextValue("--output");
                _index++;
            }
            else if (arg.StartsWith("--format="))
            {
                options.OutputFormat = arg.Substring(9);
                _index++;
            }
            else if (arg == "--format")
            {
                options.OutputFormat = GetNextValue("--format");
                _index++;
            }
            else if (arg.StartsWith("--config="))
            {
                options.ConfigFile = arg.Substring(9);
                _index++;
            }
            else if (arg == "--config")
            {
                options.ConfigFile = GetNextValue("--config");
                _index++;
            }
            else if (arg.StartsWith("--timeout="))
            {
                if (int.TryParse(arg.Substring(10), out var timeout))
                    options.AnalysisTimeoutSeconds = timeout;
                _index++;
            }
            else if (arg == "--timeout")
            {
                var value = GetNextValue("--timeout");
                if (int.TryParse(value, out var timeout))
                    options.AnalysisTimeoutSeconds = timeout;
                _index++;
            }
            else if (arg.StartsWith("--threads="))
            {
                if (int.TryParse(arg.Substring(10), out var threads))
                    options.MaxParallelThreads = threads;
                _index++;
            }
            else if (arg == "--threads")
            {
                var value = GetNextValue("--threads");
                if (int.TryParse(value, out var threads))
                    options.MaxParallelThreads = threads;
                _index++;
            }
            else if (arg.StartsWith("--log-level="))
            {
                if (int.TryParse(arg.Substring(12), out var level))
                    options.LogLevel = level;
                _index++;
            }
            else if (arg == "--log-level")
            {
                var value = GetNextValue("--log-level");
                if (int.TryParse(value, out var level))
                    options.LogLevel = level;
                _index++;
            }
            else if (arg.StartsWith("--rule-filter="))
            {
                var filters = arg.Substring(14).Split(',');
                options.RuleFilter.AddRange(filters.Select(f => f.Trim()));
                _index++;
            }
            else if (arg == "--rule-filter")
            {
                var value = GetNextValue("--rule-filter");
                var filters = value.Split(',');
                options.RuleFilter.AddRange(filters.Select(f => f.Trim()));
                _index++;
            }
            else if (arg == "--no-fail-on-violations")
            {
                options.FailOnViolations = false;
                _index++;
            }
            else if (arg == "--no-report")
            {
                options.GenerateReport = false;
                _index++;
            }
            else if (arg.StartsWith("--report-type="))
            {
                options.ReportType = arg.Substring(14);
                _index++;
            }
            else if (arg == "--report-type")
            {
                options.ReportType = GetNextValue("--report-type");
                _index++;
            }
            else
            {
                // Try to treat as positional argument
                if (!arg.StartsWith("-") && string.IsNullOrWhiteSpace(options.ProjectPath))
                {
                    options.ProjectPath = arg;
                }
                _index++;
            }
        }

        return options;
    }

    /// <summary>
    /// Gets the next value from arguments, handling the case where option value is separate.
    /// </summary>
    private string GetNextValue(string optionName)
    {
        _index++;
        if (_index >= _args.Length)
            throw new ArgumentException($"Option {optionName} requires a value");

        return _args[_index];
    }

    /// <summary>
    /// Parses arguments with exception handling, useful for CLI entry points.
    /// </summary>
    public static CliOptions ParseSafe(string[] args)
    {
        try
        {
            var parser = new CliArgumentParser(args);
            return parser.Parse();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error parsing arguments: {ex.Message}");
            return new CliOptions { ShowHelp = true };
        }
    }
}
