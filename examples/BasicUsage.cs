// BasicUsage.cs
using System;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Services;
using Microsoft.Extensions.DependencyInjection;

// Minimal setup and first call to analyze a project
public class BasicUsage
{
    public async Task RunBasicAnalysis(string projectPath)
    {
        // 1. Setup DI container
        var services = new ServiceCollection();
        services.AddRoslynGuardAnalyzer(); // Assuming extension method for DI
        var serviceProvider = services.BuildServiceProvider();

        // 2. Resolve the analysis service
        var analysisService = serviceProvider.GetRequiredService<IAnalysisService>();

        // 3. Run analysis
        Console.WriteLine($"Starting analysis for: {projectPath}");
        var result = await analysisService.AnalyzeProjectAsync(projectPath);

        // 4. Output results
        Console.WriteLine($"Analysis complete. Found {result.ViolationCount} violations.");
        foreach (var violation in result.Violations)
        {
            Console.WriteLine($"- [{violation.RuleId}] {violation.Message} at {violation.FilePath}:{violation.Line}");
        }
    }
}
