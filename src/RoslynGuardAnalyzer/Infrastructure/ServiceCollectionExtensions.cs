// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

using Microsoft.Extensions.DependencyInjection;
namespace RoslynGuardAnalyzer.Infrastructure;

/// <summary>
/// Extension methods for configuring dependency injection in the application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all analyzer services into the dependency injection container.
    /// </summary>
    public static void RegisterAnalyzerServices(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register repositories
        services.AddSingleton<RuleRepository>();
        services.AddSingleton<AnalysisResultRepository>();
        services.AddSingleton<ProjectRepository>();

        // Register services
        services.AddSingleton<IRuleRegistry, RuleRegistry>();
        services.AddSingleton<IRuleEngine, RuleEngine>();
        services.AddSingleton<IReportingService, ReportingService>();
        services.AddSingleton<IValidationService, ValidationService>();
        services.AddSingleton<IAnalysisService, AnalysisService>();

        // Register configuration
        services.AddSingleton<AnalyzerConfiguration>();
    }

    /// <summary>
    /// Registers analyzer services with a custom data directory.
    /// </summary>
    public static void RegisterAnalyzerServices(this IServiceCollection services, string dataDirectory)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register repositories with custom data directory
        services.AddSingleton(new RuleRepository(dataDirectory));
        services.AddSingleton(new AnalysisResultRepository(dataDirectory));
        services.AddSingleton(new ProjectRepository(dataDirectory));

        // Register services
        services.AddSingleton<IRuleRegistry, RuleRegistry>();
        services.AddSingleton<IRuleEngine, RuleEngine>();
        services.AddSingleton<IReportingService, ReportingService>();
        services.AddSingleton<IValidationService, ValidationService>();
        services.AddSingleton<IAnalysisService, AnalysisService>();

        // Register configuration
        services.AddSingleton(new AnalyzerConfiguration { DataDirectory = dataDirectory });
    }

    /// <summary>
    /// Initializes analyzer services after registration.
    /// </summary>
    public static async Task InitializeAnalyzerAsync(this IServiceProvider serviceProvider)
    {
        var ruleRepository = serviceProvider.GetRequiredService<RuleRepository>();
        var projectRepository = serviceProvider.GetRequiredService<ProjectRepository>();

        try
        {
            await ruleRepository.LoadAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load rules from disk: {ex.Message}");
        }

        try
        {
            await projectRepository.LoadAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load projects from disk: {ex.Message}");
        }
    }

    /// <summary>
    /// Registers only the validation service for lightweight usage.
    /// </summary>
    public static void RegisterValidationOnly(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddSingleton<IValidationService, ValidationService>();
    }

    /// <summary>
    /// Registers only the reporting service.
    /// </summary>
    public static void RegisterReportingOnly(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddSingleton<IReportingService, ReportingService>();
    }

    /// <summary>
    /// Configures the analyzer with custom settings.
    /// </summary>
    public static void ConfigureAnalyzer(this IServiceCollection services, Action<AnalyzerConfiguration> configure)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var config = new AnalyzerConfiguration();
        configure(config);

        services.AddSingleton(config);
    }
}

/// <summary>
/// Configuration settings for the analyzer.
/// </summary>
public sealed class AnalyzerConfiguration
{
    /// <summary>
    /// Gets or sets the data directory for storing rules, results, and projects.
    /// </summary>
    public string DataDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RoslynGuardAnalyzer");

    /// <summary>
    /// Gets or sets the maximum number of violations to report per analysis.
    /// </summary>
    public int MaxViolationsToReport { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the analysis timeout in seconds.
    /// </summary>
    public int AnalysisTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets whether to fail on analysis errors.
    /// </summary>
    public bool FailOnError { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to generate detailed reports.
    /// </summary>
    public bool GenerateDetailedReports { get; set; } = true;

    /// <summary>
    /// Gets or sets the default output format for reports.
    /// </summary>
    public string DefaultReportFormat { get; set; } = "text";

    /// <summary>
    /// Gets or sets the log level (0=Silent, 1=Errors, 2=Warnings, 3=Info, 4=Debug).
    /// </summary>
    public int LogLevel { get; set; } = 2;

    /// <summary>
    /// Gets or sets whether to use parallel analysis.
    /// </summary>
    public bool UseParallelAnalysis { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of parallel threads to use.
    /// </summary>
    public int MaxParallelThreads { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(DataDirectory)
            && MaxViolationsToReport > 0
            && AnalysisTimeoutSeconds > 0
            && LogLevel >= 0 && LogLevel <= 4
            && MaxParallelThreads > 0;
    }

    /// <summary>
    /// Ensures the data directory exists.
    /// </summary>
    public void EnsureDataDirectoryExists()
    {
        if (!Directory.Exists(DataDirectory))
        {
            Directory.CreateDirectory(DataDirectory);
        }
    }

    /// <summary>
    /// Creates a copy of this configuration.
    /// </summary>
    public AnalyzerConfiguration Clone()
    {
        return new AnalyzerConfiguration
        {
            DataDirectory = DataDirectory,
            MaxViolationsToReport = MaxViolationsToReport,
            AnalysisTimeoutSeconds = AnalysisTimeoutSeconds,
            FailOnError = FailOnError,
            GenerateDetailedReports = GenerateDetailedReports,
            DefaultReportFormat = DefaultReportFormat,
            LogLevel = LogLevel,
            UseParallelAnalysis = UseParallelAnalysis,
            MaxParallelThreads = MaxParallelThreads
        };
    }
}
