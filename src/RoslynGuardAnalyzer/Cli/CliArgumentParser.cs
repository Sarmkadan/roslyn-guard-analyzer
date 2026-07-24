#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace RoslynGuardAnalyzer.Cli;

/// <summary>
/// Parses command-line arguments into a CliOptions object.
/// Uses a state machine approach to handle flags, options, and positional arguments.
/// Supports both --option=value and --option value formats.
/// Supports response file expansion (@filename) with recursion depth and file size limits.
/// </summary>
public sealed class CliArgumentParser
{
    private readonly string[] _args;
    private int _index;

    /// <summary>
    /// Maximum recursion depth for response file expansion to prevent infinite loops.
    /// Prevents DoS attacks via circular @file references (e.g., @args.txt containing @args.txt).
    /// </summary>
    private const int MaxResponseFileRecursionDepth = 50;

    /// <summary>
    /// Maximum file size in bytes for response files to prevent memory exhaustion.
    /// Prevents DoS attacks via extremely large response files.
    /// </summary>
    private const int MaxResponseFileSizeBytes = 1_000_000; // 1MB

    /// <summary>
    /// Maximum total argument length after expansion to prevent memory exhaustion.
    /// Prevents DoS attacks via excessive argument expansion.
    /// </summary>
    private const int MaxTotalArgumentLength = 1_000_000; // 1MB

    /// <summary>
    /// Maximum number of arguments after expansion to prevent excessive processing.
    /// Prevents DoS attacks via glob expansion matching thousands of files.
    /// </summary>
    private const int MaxExpandedArguments = 10_000;

    /// <summary>
    /// Initializes a new instance of the <see cref="CliArgumentParser"/> class.
    /// </summary>
    /// <param name="args">The command-line arguments to parse.</param>
    public CliArgumentParser(string[] args)
    {
        _args = args ?? [];
    }

    /// <summary>
    /// Parses the command-line arguments and returns a CliOptions object.
    /// Expands response files (@filename) with recursion depth and file size limits.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when argument processing fails due to limits being exceeded.</exception>
    public CliOptions Parse()
    {
        // First, expand response files (@filename) with protection against infinite recursion
        var expandedArgs = ExpandResponseFiles(_args, 0);

        // Validate total argument length to prevent memory exhaustion
        var totalLength = expandedArgs.Sum(arg => arg?.Length ?? 0);
        if (totalLength > MaxTotalArgumentLength)
        {
            throw new ArgumentException(
                $"Total argument length exceeds maximum allowed ({MaxTotalArgumentLength} bytes). " +
                $"Actual: {totalLength} bytes. This may indicate malicious input.");
        }

        // Validate number of arguments to prevent excessive processing
        if (expandedArgs.Count > MaxExpandedArguments)
        {
            throw new ArgumentException(
                $"Too many arguments after expansion ({expandedArgs.Count} > {MaxExpandedArguments}). " +
                "This may indicate malicious input or excessive glob expansion.");
        }

        var options = new CliOptions();
        _index = 0;

        while (_index < expandedArgs.Count)
        {
            var arg = expandedArgs[_index];

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
                options.ProjectPath = GetNextValue(expandedArgs, "--project");
                _index++;
            }
            else if (arg.StartsWith("--file="))
            {
                options.FilePath = arg.Substring(7);
                _index++;
            }
            else if (arg == "--file")
            {
                options.FilePath = GetNextValue(expandedArgs, "--file");
                _index++;
            }
            else if (arg.StartsWith("--output="))
            {
                options.OutputFile = arg.Substring(9);
                _index++;
            }
            else if (arg == "--output")
            {
                options.OutputFile = GetNextValue(expandedArgs, "--output");
                _index++;
            }
            else if (arg.StartsWith("--format="))
            {
                options.OutputFormat = arg.Substring(9);
                _index++;
            }
            else if (arg == "--format")
            {
                options.OutputFormat = GetNextValue(expandedArgs, "--format");
                _index++;
            }
            else if (arg.StartsWith("--config="))
            {
                options.ConfigFile = arg.Substring(9);
                _index++;
            }
            else if (arg == "--config")
            {
                options.ConfigFile = GetNextValue(expandedArgs, "--config");
                _index++;
            }
            else if (arg.StartsWith("--timeout="))
            {
                if (int.TryParse(arg.Substring(10), NumberStyles.Integer, CultureInfo.InvariantCulture, out var timeout))
                    options.AnalysisTimeoutSeconds = timeout;
                _index++;
            }
            else if (arg == "--timeout")
            {
                var value = GetNextValue(expandedArgs, "--timeout");
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var timeout))
                    options.AnalysisTimeoutSeconds = timeout;
                _index++;
            }
            else if (arg.StartsWith("--threads="))
            {
                if (int.TryParse(arg.Substring(10), NumberStyles.Integer, CultureInfo.InvariantCulture, out var threads))
                    options.MaxParallelThreads = threads;
                _index++;
            }
            else if (arg == "--threads")
            {
                var value = GetNextValue(expandedArgs, "--threads");
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var threads))
                    options.MaxParallelThreads = threads;
                _index++;
            }
            else if (arg.StartsWith("--log-level="))
            {
                if (int.TryParse(arg.Substring(12), NumberStyles.Integer, CultureInfo.InvariantCulture, out var level))
                    options.LogLevel = level;
                _index++;
            }
            else if (arg == "--log-level")
            {
                var value = GetNextValue(expandedArgs, "--log-level");
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var level))
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
                var value = GetNextValue(expandedArgs, "--rule-filter");
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
                options.ReportType = GetNextValue(expandedArgs, "--report-type");
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
    /// Expands response files (@filename) in the arguments array.
    /// Response files can contain additional command-line arguments, one per line.
    /// </summary>
    /// <param name="args">The original arguments array.</param>
    /// <param name="recursionDepth">Current recursion depth to prevent infinite loops.</param>
    /// <returns>Expanded arguments array with response file contents inserted.</returns>
    /// <exception cref="ArgumentException">Thrown when recursion depth or file size limits are exceeded.</exception>
    private List<string> ExpandResponseFiles(string[] args, int recursionDepth)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (recursionDepth >= MaxResponseFileRecursionDepth)
        {
            throw new ArgumentException(
                $"Response file expansion recursion depth exceeded maximum of {MaxResponseFileRecursionDepth}. " +
                "This may indicate a circular reference in response files.");
        }

        var result = new List<string>();
        var processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var arg in args)
        {
            // Check for response file pattern (@filename)
            if (arg.StartsWith("@", StringComparison.Ordinal))
            {
                var filePath = arg.Substring(1);

                // Validate file path is not empty
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentException("Response file path cannot be empty. Use @filename to reference a response file.");
                }

                // Prevent duplicate file processing
                if (processedFiles.Contains(filePath))
                {
                    // Skip duplicate to prevent infinite loops from same file referenced multiple times
                    continue;
                }
                processedFiles.Add(filePath);

                // Read and parse the response file
                var fileArgs = ReadResponseFile(filePath);

                // Recursively expand any response files in the loaded file
                var expandedFileArgs = ExpandResponseFiles(fileArgs, recursionDepth + 1);

                // Insert expanded arguments at current position
                result.AddRange(expandedFileArgs);
            }
            else
            {
                result.Add(arg);
            }
        }

        return result;
    }

    /// <summary>
    /// Reads a response file and returns its contents as an array of arguments.
    /// </summary>
    /// <param name="filePath">Path to the response file.</param>
    /// <returns>Array of arguments from the response file.</returns>
    /// <exception cref="ArgumentException">Thrown when file cannot be read or size limits are exceeded.</exception>
    private string[] ReadResponseFile(string filePath)
    {
        // Check if file exists
        if (!File.Exists(filePath))
        {
            throw new ArgumentException($"Response file not found: {filePath}");
        }

        // Check file size to prevent memory exhaustion
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > MaxResponseFileSizeBytes)
        {
            throw new ArgumentException(
                $"Response file exceeds maximum size of {MaxResponseFileSizeBytes} bytes. " +
                $"File: {filePath}, Size: {fileInfo.Length} bytes. " +
                "This may indicate malicious input.");
        }

        // Read file contents
        var fileContents = File.ReadAllText(filePath, Encoding.UTF8);

        // Normalize line endings and split into arguments
        var lines = fileContents.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        // Filter out empty lines and comments (lines starting with # or //)
        var args = new List<string>();
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#") || trimmedLine.StartsWith("//"))
            {
                continue;
            }

            args.Add(trimmedLine);
        }

        return args.ToArray();
    }

    /// <summary>
    /// Gets the next value from arguments, handling the case where option value is separate.
    /// </summary>
    /// <param name="args">The arguments list to process.</param>
    /// <param name="optionName">The option name for error reporting.</param>
    /// <returns>The next argument value.</returns>
    /// <exception cref="ArgumentException">Thrown when option requires a value but none is available.</exception>
    private string GetNextValue(List<string> args, string optionName)
    {
        _index++;
        if (_index >= args.Count)
            throw new ArgumentException($"Option {optionName} requires a value");

        return args[_index];
    }

    /// <summary>
    /// Parses arguments with exception handling, useful for CLI entry points.
    /// </summary>
    /// <param name="args">The command-line arguments to parse.</param>
    /// <returns>Parsed CliOptions, or default options with help shown on error.</returns>
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
