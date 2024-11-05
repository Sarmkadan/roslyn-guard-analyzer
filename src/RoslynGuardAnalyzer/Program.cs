#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using RoslynGuardAnalyzer.Configuration;
using RoslynGuardAnalyzer.Infrastructure;
using RoslynGuardAnalyzer.Services;

namespace RoslynGuardAnalyzer;

/// <summary>
/// Main entry point for the Roslyn Guard Analyzer application.
/// Initializes the dependency injection container and orchestrates the analysis workflow.
/// </summary>
internal sealed class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            // Build configuration from multiple sources
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables("RoslynGuardAnalyzer__")
                .AddCommandLine(args, new Dictionary<string, string>())
                .Build();

            // Configure services with validation
            var services = new ServiceCollection();
            services.AddOptions<RoslynGuardAnalyzerOptions>()
                .Bind(configuration.GetSection("RoslynGuardAnalyzer"))
                .ValidateOnStart();

            services.RegisterAnalyzerServices();
            services.AddSingleton<IValidateOptions<RoslynGuardAnalyzerOptions>, RoslynGuardAnalyzerOptionsValidator>();

            var serviceProvider = services.BuildServiceProvider();

            // Resolve and validate options
            var options = serviceProvider.GetRequiredService<IOptions<RoslynGuardAnalyzerOptions>>().Value;
            var validationErrors = options.Validate();

            if (validationErrors.Count > 0)
            {
                Console.Error.WriteLine("Configuration validation errors:");
                foreach (var error in validationErrors)
                {
                    Console.Error.WriteLine($"  - {error}");
                }
                return 1;
            }

            var analysisService = serviceProvider.GetRequiredService<IAnalysisService>();
            var reportingService = serviceProvider.GetRequiredService<IReportingService>();

            Console.WriteLine("=== Roslyn Guard Analyzer ===");
            Console.WriteLine("Starting architecture rule analysis...\n");

            // Handle CLI-specific options
            var cliOptions = ParseCliOptions(args);
            if (cliOptions.ShowHelp)
            {
                ShowHelp();
                return 0;
            }

            if (cliOptions.ShowVersion)
            {
                ShowVersion();
                return 0;
            }

            // CLI options override configuration
            options.MergeWithCliOptions(cliOptions);

            // Validate after CLI merge
            validationErrors = options.Validate();
            if (validationErrors.Count > 0)
            {
                Console.Error.WriteLine("Configuration validation errors after CLI merge:");
                foreach (var error in validationErrors)
                {
                    Console.Error.WriteLine($"  - {error}");
                }
                return 1;
            }

            if (string.IsNullOrWhiteSpace(options.ProjectPath))
            {
                Console.Error.WriteLine("Error: Project path must be specified");
                ShowHelp();
                return 1;
            }

            if (!Directory.Exists(options.ProjectPath) && !File.Exists(options.ProjectPath))
            {
                Console.Error.WriteLine($"Error: Project path not found: {options.ProjectPath}");
                return 1;
            }

            Console.WriteLine($"Analyzing: {options.ProjectPath}");
            Console.WriteLine($"Output format: {options.OutputFormat}");
            if (!string.IsNullOrWhiteSpace(options.OutputFile))
            {
                Console.WriteLine($"Output file: {options.OutputFile}");
            }
            Console.WriteLine();

            var result = await analysisService.AnalyzeProjectAsync(options.ProjectPath);
            var report = reportingService.GenerateReport(result);

            if (!string.IsNullOrWhiteSpace(options.OutputFile))
            {
                await File.WriteAllTextAsync(options.OutputFile, report);
                Console.WriteLine($"Report saved to: {options.OutputFile}");
            }
            else
            {
                Console.WriteLine(report);
            }

            Console.WriteLine($"\nAnalysis completed: {result.ViolationCount} violations found");

            return options.FailOnViolations && result.ViolationCount > 0 ? 1 : 0;
        }
        catch (OptionsValidationException ex)
        {
            Console.Error.WriteLine("Configuration validation failed:");
            foreach (var failure in ex.Failures)
            {
                Console.Error.WriteLine($"  - {failure}");
            }
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            if (ex is not null)
            {
                Console.Error.WriteLine(ex.StackTrace);
            }
            return -1;
        }
    }

    /// <summary>
    /// Parses command-line arguments into CliOptions structure.
    /// </summary>
    private static Cli.CliOptions ParseCliOptions(string[] args)
    {
        var options = new Cli.CliOptions();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--help" or "-h":
                    options.ShowHelp = true;
                    return options;
                case "--version":
                    options.ShowVersion = true;
                    return options;
                case "--project":
                    if (i + 1 < args.Length) options.ProjectPath = args[++i];
                    break;
                case "--file":
                    if (i + 1 < args.Length) options.FilePath = args[++i];
                    break;
                case "--format" or "-f":
                    if (i + 1 < args.Length) options.OutputFormat = args[++i];
                    break;
                case "--output" or "-o":
                    if (i + 1 < args.Length) options.OutputFile = args[++i];
                    break;
                case "--config" or "-c":
                    if (i + 1 < args.Length) options.ConfigFile = args[++i];
                    break;
                case "--rules" or "-r":
                    if (i + 1 < args.Length)
                    {
                        var rules = args[++i].Split(',', StringSplitOptions.RemoveEmptyEntries);
                        options.RuleFilter.AddRange(rules);
                    }
                    break;
                case "--strict" or "-s":
                    options.FailOnViolations = true;
                    break;
                case "--verbose" or "-v":
                    options.Verbose = true;
                    break;
                case "--quiet" or "-q":
                    options.LogLevel = 0;
                    break;
                case var arg when arg.StartsWith("--max-threads="):
                    if (int.TryParse(arg["--max-threads=".Length..], out var threads))
                    {
                        options.MaxParallelThreads = threads;
                    }
                    break;
                case var arg when arg.StartsWith("--timeout="):
                    if (int.TryParse(arg["--timeout=".Length..], out var timeout))
                    {
                        options.AnalysisTimeoutSeconds = timeout;
                    }
                    break;
                case var arg when !arg.StartsWith("-"):
                    // Positional argument - treat as project path
                    if (string.IsNullOrWhiteSpace(options.ProjectPath))
                    {
                        options.ProjectPath = arg;
                    }
                    break;
            }
        }

        return options;
    }

    /// <summary>
    /// Displays help information.
    /// </summary>
    private static void ShowHelp()
    {
        Console.WriteLine("Roslyn Guard Analyzer - Usage:");
        Console.WriteLine();
        Console.WriteLine("  roslyn-guard-analyzer <path> [options]");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <path>              Project file (.csproj) or directory to analyze");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --project <path>     Project file or directory path");
        Console.WriteLine("  --file <path>        Single file to analyze");
        Console.WriteLine("  --format, -f <type>  Output format: text, json, csv, html, xml");
        Console.WriteLine("  --output, -o <file>  Output file path");
        Console.WriteLine("  --config, -c <file>  Configuration file path");
        Console.WriteLine("  --rules, -r <ids>   Comma-separated rule IDs to execute");
        Console.WriteLine("  --strict, -s         Fail on any violation (exit code 1)");
        Console.WriteLine("  --verbose, -v        Verbose output");
        Console.WriteLine("  --quiet, -q          Suppress console output");
        Console.WriteLine("  --max-threads=N       Maximum parallel threads (default: CPU count)");
        Console.WriteLine("  --timeout=N           Analysis timeout in seconds (default: 600)");
        Console.WriteLine("  --help, -h           Show this help message");
        Console.WriteLine("  --version            Show version information");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  roslyn-guard-analyzer ./src/MyProject.csproj");
        Console.WriteLine("  roslyn-guard-analyzer . --format json --output report.json");
        Console.WriteLine("  roslyn-guard-analyzer ./src -r LYR001,NAM001 --strict");
    }

    /// <summary>
    /// Displays version information.
    /// </summary>
    private static void ShowVersion()
    {
        Console.WriteLine("Roslyn Guard Analyzer v1.0.0");
        Console.WriteLine("Copyright (c) 2026 Vladyslav Zaiets");
    }
}
