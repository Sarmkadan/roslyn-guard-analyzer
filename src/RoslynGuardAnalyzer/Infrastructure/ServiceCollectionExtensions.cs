#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using RoslynGuardAnalyzer.CodeFixes;
using RoslynGuardAnalyzer.Data;
using RoslynGuardAnalyzer.Rules;
using RoslynGuardAnalyzer.Services;
using RoslynGuardAnalyzer.Suppressions;

namespace RoslynGuardAnalyzer.Infrastructure;

/// <summary>
/// Extension methods for configuring dependency injection in the application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all analyzer services into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static void RegisterAnalyzerServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<RuleRepository>();
        services.AddSingleton<AnalysisResultRepository>();
        services.AddSingleton<ProjectRepository>();

        AddCoreAnalyzerServices(services);

        services.AddSingleton<AnalyzerConfiguration>();
    }

    /// <summary>
    /// Registers analyzer services with a custom data directory.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="dataDirectory">The data directory path for storing analyzer data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="dataDirectory"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="dataDirectory"/> is empty or whitespace.</exception>
    public static void RegisterAnalyzerServices(this IServiceCollection services, string dataDirectory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory, nameof(dataDirectory));

        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton(new RuleRepository(dataDirectory));
        services.AddSingleton(new AnalysisResultRepository(dataDirectory));
        services.AddSingleton(new ProjectRepository(dataDirectory));

        AddCoreAnalyzerServices(services);

        services.AddSingleton(new AnalyzerConfiguration { DataDirectory = dataDirectory });
    }

    /// <summary>
    /// Registers the core analyzer services shared by all registration overloads.
    /// Kept in one place so the overloads cannot drift apart.
    /// </summary>
    private static void AddCoreAnalyzerServices(IServiceCollection services)
    {
        services.AddSingleton<IRuleRegistry, RuleRegistry>();
        services.AddSingleton<ICustomRuleRegistry, CustomRuleRegistry>();
        services.AddSingleton<IRuleEngine, RuleEngine>();
        services.AddSingleton<CustomRuleEngine>();
        services.AddSingleton<ISuppressionManager, SuppressionManager>();
        services.AddSingleton<ICodeFixService, CodeFixService>();
        services.AddSingleton<IFixAllProvider, FixAllProvider>();
        services.AddSingleton<IReportingService, ReportingService>();
        services.AddSingleton<IValidationService, ValidationService>();
        services.AddSingleton<IAnalysisService, AnalysisService>();
    }

    /// <summary>
    /// Initializes analyzer services after registration.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve services from.</param>
    /// <param name="logger">Optional logger for initialization messages.</param>
    /// <returns>A task representing the initialization operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    public static async Task InitializeAnalyzerAsync(this IServiceProvider serviceProvider, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var ruleRepository = serviceProvider.GetRequiredService<RuleRepository>();
        var projectRepository = serviceProvider.GetRequiredService<ProjectRepository>();

        try
        {
            await ruleRepository.LoadAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Could not load rules from disk");
        }

        try
        {
            await projectRepository.LoadAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Could not load projects from disk");
        }
    }

    /// <summary>
    /// Registers only the validation service for lightweight usage.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static void RegisterValidationOnly(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IValidationService, ValidationService>();
    }

    /// <summary>
    /// Registers only the reporting service.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static void RegisterReportingOnly(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IReportingService, ReportingService>();
    }

    /// <summary>
    /// Configures the analyzer with custom settings.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configure">Configuration action to apply.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public static void ConfigureAnalyzer(this IServiceCollection services, Action<AnalyzerConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

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
    public bool FailOnError { get; set; }

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
    /// <returns><see langword="true"/> if the configuration is valid; otherwise, <see langword="false"/>.</returns>
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
    /// <exception cref="IOException">Thrown when the directory cannot be created.</exception>
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
    /// <returns>A new <see cref="AnalyzerConfiguration"/> instance with the same values.</returns>
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