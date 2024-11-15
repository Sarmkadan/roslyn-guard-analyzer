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

        sb.AppendLine("Analysis Targets:");
        sb.AppendLine("  --project PATH          Path to project file (.csproj) or directory");
        sb.AppendLine("  --file PATH             Path to single C# file to analyze");
        sb.AppendLine();

        sb.AppendLine("Output Options:");
        sb.AppendLine("  --format FORMAT         Output format: text, json, csv, html, xml (default: text)");
        sb.AppendLine("  --output FILE           Write output to file (default: stdout)");
        sb.AppendLine("  --report-type TYPE      Report type: summary, detailed, violations (default: summary)");
        sb.AppendLine("  --no-report             Skip report generation");
        sb.AppendLine();

        sb.AppendLine("Analysis Options:");
        sb.AppendLine("  --config FILE           Path to configuration file");
        sb.AppendLine("  --rule-filter RULES     Comma-separated rule names to apply");
        sb.AppendLine("  --timeout SECONDS       Analysis timeout in seconds (default: 300)");
        sb.AppendLine("  --threads NUM           Number of parallel threads (default: CPU count)");
        sb.AppendLine("  --skip-cache            Skip analysis result caching");
        sb.AppendLine();

        sb.AppendLine("Behavior Options:");
        sb.AppendLine("  --no-fail-on-violations Exit with 0 even if violations found");
        sb.AppendLine("  --verbose               Verbose output");
        sb.AppendLine("  --log-level LEVEL       Log level: 0=silent, 1=error, 2=warn, 3=info, 4=debug");
        sb.AppendLine();

        sb.AppendLine("Help & Information:");
        sb.AppendLine("  -h, --help              Show this help message");
        sb.AppendLine("  -v, --version           Show version information");
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
