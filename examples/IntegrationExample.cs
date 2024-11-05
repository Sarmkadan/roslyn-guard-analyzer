// IntegrationExample.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RoslynGuardAnalyzer.Infrastructure;

// Demonstrating how to wire Roslyn Guard Analyzer into an ASP.NET Core application
public class IntegrationExample
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register Roslyn Guard Analyzer services
        services.AddRoslynGuardAnalyzer();
        
        // Register your custom rules
        // services.AddSingleton<AnalysisRule, MyCustomRule>();
    }

    public void ExampleUsageInController(IAnalysisService analysisService)
    {
        // Use in a controller or background service
        // var result = await analysisService.AnalyzeProjectAsync("./my-project.csproj");
    }
}
