#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

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
            var services = new ServiceCollection();
            services.RegisterAnalyzerServices();

            var serviceProvider = services.BuildServiceProvider();

            var analysisService = serviceProvider.GetRequiredService<IAnalysisService>();
            var reportingService = serviceProvider.GetRequiredService<IReportingService>();

            Console.WriteLine("=== Roslyn Guard Analyzer ===");
            Console.WriteLine("Starting architecture rule analysis...\n");

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: roslyn-guard-analyzer <project-path>");
                Console.WriteLine("Example: roslyn-guard-analyzer ./src/MyProject.csproj");
                return 1;
            }

            var projectPath = args[0];
            if (!Directory.Exists(projectPath) && !File.Exists(projectPath))
            {
                Console.WriteLine($"Error: Project path not found: {projectPath}");
                return 1;
            }

            var result = await analysisService.AnalyzeProjectAsync(projectPath);
            var report = reportingService.GenerateReport(result);

            Console.WriteLine(report);
            Console.WriteLine($"\nAnalysis completed: {result.ViolationCount} violations found");

            return result.ViolationCount > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return -1;
        }
    }
}
