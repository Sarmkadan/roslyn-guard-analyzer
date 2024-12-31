// Demonstrating how to wire Roslyn Guard Analyzer into an ASP.NET Core application
/// <summary>
/// Example of how to integrate Roslyn Guard Analyzer into an ASP.NET Core application.
/// </summary>
public class IntegrationExample
{
    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    /// <param name="services">The services to configure.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        // Register Roslyn Guard Analyzer services
        services.AddRoslynGuardAnalyzer();
        
        // Register your custom rules
        // services.AddSingleton<AnalysisRule, MyCustomRule>();
    }

    /// <summary>
    /// Demonstrates how to use the analysis service in a controller or background service.
    /// </summary>
    /// <param name="analysisService">The analysis service to use.</param>
    public void ExampleUsageInController(IAnalysisService analysisService)
    {
        // Use in a controller or background service
        // var result = await analysisService.AnalyzeProjectAsync("./my-project.csproj");
    }
}
