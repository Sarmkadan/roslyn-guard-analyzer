#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text;

namespace RoslynGuardAnalyzer.Cli;

/// <summary>
/// Generates formatted help text for the CLI application.
/// Provides usage information, command descriptions, and examples.
/// </summary>
public sealed class HelpGenerator
{
    private const string AppName = "roslyn-guard-analyzer";
    private const string AppVersion = "1.0.0";

    /// <summary>
    /// Generates complete help text including usage and all options.
    /// </summary>
    public static string GenerateFullHelp()
    {
        var sb = new StringBuilder();
        var consoleWidth = Console.WindowWidth > 80 ? Console.WindowWidth : 80;

        sb.AppendLine($"{AppName} v{AppVersion}");
        sb.AppendLine("Roslyn-based code analyzer enforcing architectural rules");
        sb.AppendLine();

        sb.AppendLine("USAGE:");
        sb.AppendLine($"  {AppName} [OPTIONS] [PROJECT_PATH]");
        sb.AppendLine();

        sb.AppendLine("EXAMPLES:");
        sb.AppendLine($"  {AppName} ./src/MyProject.csproj");
        sb.AppendLine($"  {AppName} --project=./src/MyProject --format=json --output=report.json");
        sb.AppendLine($"  {AppName} --file=./src/MyClass.cs --verbose");
        sb.AppendLine($"  {AppName} --project=. --rule-filter=LayerDependency,NamingConvention");
        sb.AppendLine();

        sb.AppendLine("OPTIONS:");
        sb.AppendLine();

        var options = new[]
        {
            new { Name = "--project", Description = "Path to project file (.csproj) or directory" },
            new { Name = "--file", Description = "Path to single C# file to analyze" },
            new { Name = "--format", Description = "Output format: text, json, csv, html, xml (default: text)" },
            new { Name = "--output", Description = "Write output to file (default: stdout)" },
            new { Name = "--report-type", Description = "Report type: summary, detailed, violations (default: summary)" },
            new { Name = "--no-report", Description = "Skip report generation" },
            new { Name = "--config", Description = "Path to configuration file" },
            new { Name = "--rule-filter", Description = "Comma-separated rule names to apply" },
            new { Name = "--timeout", Description = "Analysis timeout in seconds (default: 300)" },
            new { Name = "--threads", Description = "Number of parallel threads (default: CPU count)" },
            new { Name = "--skip-cache", Description = "Skip analysis result caching" },
            new { Name = "--no-fail-on-violations", Description = "Exit with 0 even if violations found" },
            new { Name = "--verbose", Description = "Verbose output" },
            new { Name = "--log-level", Description = "Log level: 0=silent, 1=error, 2=warn, 3=info, 4=debug" },
            new { Name = "-h", Description = "Show this help message" },
            new { Name = "--help", Description = "Show this help message" },
            new { Name = "-v", Description = "Show version information" },
            new { Name = "--version", Description = "Show version information" },
        };

        foreach (var option in options)
        {
            var maxLength = option.Name.Length;
            if (option.Description.Length > maxLength)
            {
                maxLength = option.Description.Length;
            }

            sb.AppendLine($"  {option.Name,-20} {option.Description.PadRight(maxLength)}");
        }

        sb.AppendLine();

        sb.AppendLine("SUPPORTED RULES:");
        sb.AppendLine("  • LayerDependency       - Enforces layer dependency constraints");
        sb.AppendLine("  • NamingConvention      - Validates naming conventions");
        sb.AppendLine("  • AsyncPatterns         - Checks async/await usage patterns");
        sb.AppendLine("  • NullSafety            - Enforces null safety patterns");
        sb.AppendLine("  • CircularDependency    - Detects circular references");
        sb.AppendLine();

        sb.AppendLine("EXIT CODES:");
        sb.AppendLine("  0                       Success, no violations");
        sb.AppendLine("  1                       Success, but violations found");
        sb.AppendLine("  -1                      Fatal error during analysis");
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Generates brief help text for quick reference.
    /// </summary>
    public static string GenerateBriefHelp()
    {
        var sb = new StringBuilder();
        var consoleWidth = Console.WindowWidth > 80 ? Console.WindowWidth : 80;

        sb.AppendLine($"{AppName} - Architectural Rules Analyzer");
        sb.AppendLine();
        sb.AppendLine("Usage: roslyn-guard-analyzer [OPTIONS] [PROJECT_PATH]");
        sb.AppendLine();
        sb.AppendLine("Common options:");
        sb.AppendLine("  --project PATH          Analyze project at PATH");
        sb.AppendLine("  --file PATH             Analyze single file at PATH");
        sb.AppendLine("  --format FORMAT         Output format (text|json|csv|html|xml)");
        sb.AppendLine("  --output FILE           Write output to FILE");
        sb.AppendLine("  --verbose               Verbose output");
        sb.AppendLine("  -h, --help              Show detailed help");
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Generates version information string.
    /// </summary>
    public static string GenerateVersion()
    {
        return $"{AppName} version {AppVersion}" + Environment.NewLine +
               "Copyright 2026 Vladyslav Zaiets" + Environment.NewLine +
               "License: MIT" + Environment.NewLine;
    }

    /// <summary>
    /// Generates error message with suggestion to use --help.
    /// </summary>
    public static string GenerateErrorMessage(string error)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Error: {error}");
        sb.AppendLine($"Use '{AppName} --help' for more information.");
        return sb.ToString();
    }

    /// <summary>
    /// Generates usage summary for quick reference.
    /// </summary>
    public static string GenerateUsageSummary()
    {
        return $"Usage: {AppName} [OPTIONS] [PROJECT_PATH]" + Environment.NewLine +
               $"       Use '{AppName} --help' for detailed information" + Environment.NewLine;
    }
}
