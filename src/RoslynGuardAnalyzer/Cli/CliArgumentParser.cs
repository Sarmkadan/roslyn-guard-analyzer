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
    private const string HelpShortOption = "-h";
    private const string HelpOption = "--help";
    private const string VersionShortOption = "-v";
    private const string VersionOption = "--version";
    private const string VerboseOption = "--verbose";
    private const string SkipCacheOption = "--skip-cache";
    private const string ProjectOption = "--project";
    private const string FileOption = "--file";
    private const string OutputOption = "--output";
    private const string FormatOption = "--format";
    private const string ConfigOption = "--config";
    private const string TimeoutOption = "--timeout";
    private const string ThreadsOption = "--threads";
    private const string LogLevelOption = "--log-level";
    private const string RuleFilterOption = "--rule-filter";
    private const string NoFailOnViolationsOption = "--no-fail-on-violations";
    private const string NoReportOption = "--no-report";
    private const string ReportTypeOption = "--report-type";
    private const string OptionValueSeparator = "=";
    private const string OptionPrefix = "-";
    private const string ResponseFilePrefix = "@";
    private const string ResponseFileCommentPrefix = "#";
    private const string ResponseFileAlternativeCommentPrefix = "//";

    private const string TotalArgumentLengthExceededMessage = "Total argument length exceeds maximum allowed ({0} bytes). Actual: {1} bytes. This may indicate malicious input.";
    private const string TooManyArgumentsMessage = "Too many arguments after expansion ({0} > {1}). This may indicate malicious input or excessive glob expansion.";
    private const string ResponseFileRecursionDepthExceededMessage = "Response file expansion recursion depth exceeded maximum of {0}. This may indicate a circular reference in response files.";
    private const string EmptyResponseFilePathMessage = "Response file path cannot be empty. Use @filename to reference a response file.";
    private const string ResponseFileNotFoundMessage = "Response file not found: {0}";
    private const string ResponseFileSizeExceededMessage = "Response file exceeds maximum size of {0} bytes. File: {1}, Size: {2} bytes. This may indicate malicious input.";
    private const string MissingOptionValueMessage = "Option {0} requires a value";
    private const string ArgumentParsingErrorMessage = "Error parsing arguments: {0}";

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
            throw new ArgumentException(string.Format(
                CultureInfo.CurrentCulture,
                TotalArgumentLengthExceededMessage,
                MaxTotalArgumentLength,
                totalLength));
        }

        // Validate number of arguments to prevent excessive processing
        if (expandedArgs.Count > MaxExpandedArguments)
        {
            throw new ArgumentException(string.Format(
                CultureInfo.CurrentCulture,
                TooManyArgumentsMessage,
                expandedArgs.Count,
                MaxExpandedArguments));
        }

        var options = new CliOptions();
        _index = 0;

        while (_index < expandedArgs.Count)
        {
            var arg = expandedArgs[_index];

            if (arg == HelpShortOption || arg == HelpOption)
            {
                options.ShowHelp = true;
                _index++;
            }
            else if (arg == VersionShortOption || arg == VersionOption)
            {
                options.ShowVersion = true;
                _index++;
            }
            else if (arg == VerboseOption)
            {
                options.Verbose = true;
                _index++;
            }
            else if (arg == SkipCacheOption)
            {
                options.SkipCache = true;
                _index++;
            }
            else if (arg.StartsWith(ProjectOption + OptionValueSeparator))
            {
                options.ProjectPath = arg.Substring(ProjectOption.Length + OptionValueSeparator.Length);
                _index++;
            }
            else if (arg == ProjectOption)
            {
                options.ProjectPath = GetNextValue(expandedArgs, ProjectOption);
                _index++;
            }
            else if (arg.StartsWith(FileOption + OptionValueSeparator))
            {
                options.FilePath = arg.Substring(FileOption.Length + OptionValueSeparator.Length);
                _index++;
            }
            else if (arg == FileOption)
            {
                options.FilePath = GetNextValue(expandedArgs, FileOption);
                _index++;
            }
            else if (arg.StartsWith(OutputOption + OptionValueSeparator))
            {
                options.OutputFile = arg.Substring(OutputOption.Length + OptionValueSeparator.Length);
                _index++;
            }
            else if (arg == OutputOption)
            {
                options.OutputFile = GetNextValue(expandedArgs, OutputOption);
                _index++;
            }
            else if (arg.StartsWith(FormatOption + OptionValueSeparator))
            {
                options.OutputFormat = arg.Substring(FormatOption.Length + OptionValueSeparator.Length);
                _index++;
            }
            else if (arg == FormatOption)
            {
                options.OutputFormat = GetNextValue(expandedArgs, FormatOption);
                _index++;
            }
            else if (arg.StartsWith(ConfigOption + OptionValueSeparator))
            {
                options.ConfigFile = arg.Substring(ConfigOption.Length + OptionValueSeparator.Length);
                _index++;
            }
            else if (arg == ConfigOption)
            {
                options.ConfigFile = GetNextValue(expandedArgs, ConfigOption);
                _index++;
            }
            else if (arg.StartsWith(TimeoutOption + OptionValueSeparator))
            {
                if (int.TryParse(arg.Substring(TimeoutOption.Length + OptionValueSeparator.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out var timeout))
                    options.AnalysisTimeoutSeconds = timeout;
                _index++;
            }
            else if (arg == TimeoutOption)
            {
                var value = GetNextValue(expandedArgs, TimeoutOption);
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var timeout))
                    options.AnalysisTimeoutSeconds = timeout;
                _index++;
            }
            else if (arg.StartsWith(ThreadsOption + OptionValueSeparator))
            {
                if (int.TryParse(arg.Substring(ThreadsOption.Length + OptionValueSeparator.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out var threads))
                    options.MaxParallelThreads = threads;
                _index++;
            }
            else if (arg == ThreadsOption)
            {
                var value = GetNextValue(expandedArgs, ThreadsOption);
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var threads))
                    options.MaxParallelThreads = threads;
                _index++;
            }
            else if (arg.StartsWith(LogLevelOption + OptionValueSeparator))
            {
                if (int.TryParse(arg.Substring(LogLevelOption.Length + OptionValueSeparator.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out var level))
                    options.LogLevel = level;
                _index++;
            }
            else if (arg == LogLevelOption)
            {
                var value = GetNextValue(expandedArgs, LogLevelOption);
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var level))
                    options.LogLevel = level;
                _index++;
            }
            else if (arg.StartsWith(RuleFilterOption + OptionValueSeparator))
            {
                var filters = arg.Substring(RuleFilterOption.Length + OptionValueSeparator.Length).Split(',');
                options.RuleFilter.AddRange(filters.Select(f => f.Trim()));
                _index++;
            }
            else if (arg == RuleFilterOption)
            {
                var value = GetNextValue(expandedArgs, RuleFilterOption);
                var filters = value.Split(',');
                options.RuleFilter.AddRange(filters.Select(f => f.Trim()));
                _index++;
            }
            else if (arg == NoFailOnViolationsOption)
            {
                options.FailOnViolations = false;
                _index++;
            }
            else if (arg == NoReportOption)
            {
                options.GenerateReport = false;
                _index++;
            }
            else if (arg.StartsWith(ReportTypeOption + OptionValueSeparator))
            {
                options.ReportType = arg.Substring(ReportTypeOption.Length + OptionValueSeparator.Length);
                _index++;
            }
            else if (arg == ReportTypeOption)
            {
                options.ReportType = GetNextValue(expandedArgs, ReportTypeOption);
                _index++;
            }
            else
            {
                // Try to treat as positional argument
                if (!arg.StartsWith(OptionPrefix) && string.IsNullOrWhiteSpace(options.ProjectPath))
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
            throw new ArgumentException(string.Format(
                CultureInfo.CurrentCulture,
                ResponseFileRecursionDepthExceededMessage,
                MaxResponseFileRecursionDepth));
        }

        var result = new List<string>();
        var processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var arg in args)
        {
            // Check for response file pattern (@filename)
            if (arg.StartsWith(ResponseFilePrefix, StringComparison.Ordinal))
            {
                var filePath = arg.Substring(ResponseFilePrefix.Length);

                // Validate file path is not empty
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentException(EmptyResponseFilePathMessage);
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
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ResponseFileNotFoundMessage, filePath));
        }

        // Check file size to prevent memory exhaustion
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > MaxResponseFileSizeBytes)
        {
            throw new ArgumentException(string.Format(
                CultureInfo.CurrentCulture,
                ResponseFileSizeExceededMessage,
                MaxResponseFileSizeBytes,
                filePath,
                fileInfo.Length));
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
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(ResponseFileCommentPrefix) || trimmedLine.StartsWith(ResponseFileAlternativeCommentPrefix))
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
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, MissingOptionValueMessage, optionName));

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
            Console.Error.WriteLine(ArgumentParsingErrorMessage, ex.Message);
            return new CliOptions { ShowHelp = true };
        }
    }
}
