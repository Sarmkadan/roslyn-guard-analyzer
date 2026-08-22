#nullable enable

using System;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoslynGuardAnalyzer.Data;
using RoslynGuardAnalyzer.Infrastructure;
using RoslynGuardAnalyzer.Services;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="ServiceCollectionExtensions"/>.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    private readonly ILogger _logger;

    public ServiceCollectionExtensionsTests()
    {
        _logger = LoggerFactory.Create(builder => { }).CreateLogger<ServiceCollectionExtensionsTests>();
    }
    #region RegisterAnalyzerServices (parameterless overload)

    [Fact]
    public void RegisterAnalyzerServices_WithNullServiceCollection_ThrowsArgumentNullException()
    {
        _logger.LogInformation("Testing RegisterAnalyzerServices with null service collection");
        try
        {
            // Arrange
            IServiceCollection? services = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services!.RegisterAnalyzerServices());

            _logger.LogInformation("Test completed: RegisterAnalyzerServices correctly threw ArgumentNullException for null service collection");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in RegisterAnalyzerServices_WithNullServiceCollection_ThrowsArgumentNullException");
            throw;
        }
    }

    [Fact]
    public void RegisterAnalyzerServices_WithValidServiceCollection_RegistersAllServices()
    {
        _logger.LogInformation("Testing RegisterAnalyzerServices with valid service collection");
        try
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.RegisterAnalyzerServices();

            // Assert
            services.Should().NotBeNull();
            services.Should().HaveCountGreaterThan(0);

            // Verify all expected services are registered
            services.Should().Contain(d => d.ServiceType == typeof(ILoggerFactory) && d.Lifetime == ServiceLifetime.Singleton);
            services.Should().Contain(d => d.ServiceType == typeof(RuleRepository) && d.Lifetime == ServiceLifetime.Singleton);
            services.Should().Contain(d => d.ServiceType == typeof(AnalysisResultRepository) && d.Lifetime == ServiceLifetime.Singleton);
            services.Should().Contain(d => d.ServiceType == typeof(ProjectRepository) && d.Lifetime == ServiceLifetime.Singleton);
            services.Should().Contain(d => d.ServiceType == typeof(AnalyzerConfiguration) && d.Lifetime == ServiceLifetime.Singleton);

            _logger.LogInformation("Test completed: RegisterAnalyzerServices successfully registered all services");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in RegisterAnalyzerServices_WithValidServiceCollection_RegistersAllServices");
            throw;
        }
    }

    #endregion

    #region RegisterAnalyzerServices (with dataDirectory parameter)

    [Fact]
    public void RegisterAnalyzerServices_WithNullServiceCollectionAndDataDirectory_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        var dataDirectory = "/tmp/test";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.RegisterAnalyzerServices(dataDirectory));
    }

    [Fact]
    public void RegisterAnalyzerServices_WithNullDataDirectory_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        string? dataDirectory = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.RegisterAnalyzerServices(dataDirectory!));
    }

    [Fact]
    public void RegisterAnalyzerServices_WithEmptyDataDirectory_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var dataDirectory = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.RegisterAnalyzerServices(dataDirectory));
    }

    [Fact]
    public void RegisterAnalyzerServices_WithWhitespaceDataDirectory_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var dataDirectory = "   ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.RegisterAnalyzerServices(dataDirectory));
    }

    [Fact]
    public void RegisterAnalyzerServices_WithValidDataDirectory_RegistersAllServicesWithDataDirectory()
    {
        // Arrange
        var services = new ServiceCollection();
        var dataDirectory = "/tmp/test-analyzer";

        // Act
        services.RegisterAnalyzerServices(dataDirectory);

        // Assert
        services.Should().NotBeNull();
        services.Should().HaveCountGreaterThan(0);

        // Verify services are registered with correct data directory
        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetService<AnalyzerConfiguration>();

        config.Should().NotBeNull();
        config!.DataDirectory.Should().Be(dataDirectory);
    }

    #endregion

    #region InitializeAnalyzerAsync

    [Fact]
    public async Task InitializeAnalyzerAsync_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceProvider? serviceProvider = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => serviceProvider!.InitializeAnalyzerAsync()).ConfigureAwait(false);
    }

    [Fact]
    public async Task InitializeAnalyzerAsync_WithValidServiceProvider_InitializesRepositories()
    {
        _logger.LogInformation("Testing InitializeAnalyzerAsync with valid service provider");
        try
        {
            // Arrange
            var services = new ServiceCollection();
            services.RegisterAnalyzerServices();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            await serviceProvider.InitializeAnalyzerAsync();

            // Assert - Should complete without throwing
            _logger.LogInformation("Test completed: InitializeAnalyzerAsync successfully initialized repositories");
            true.Should().BeTrue();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in InitializeAnalyzerAsync_WithValidServiceProvider_InitializesRepositories");
            throw;
        }
    }

    [Fact]
    public async Task InitializeAnalyzerAsync_WithNullLogger_StillInitializes()
    {
        _logger.LogInformation("Testing InitializeAnalyzerAsync with null logger");
        try
        {
            // Arrange
            var services = new ServiceCollection();
            services.RegisterAnalyzerServices();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            await serviceProvider.InitializeAnalyzerAsync(logger: null);

            // Assert
            _logger.LogInformation("Test completed: InitializeAnalyzerAsync successfully initialized with null logger");
            true.Should().BeTrue();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in InitializeAnalyzerAsync_WithNullLogger_StillInitializes");
            throw;
        }
    }

    #endregion

    #region RegisterValidationOnly

    [Fact]
    public void RegisterValidationOnly_WithNullServiceCollection_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.RegisterValidationOnly());
    }

    [Fact]
    public void RegisterValidationOnly_WithValidServiceCollection_RegistersValidationService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.RegisterValidationOnly();

        // Assert
        services.Should().Contain(d => d.ServiceType == typeof(IValidationService) && d.Lifetime == ServiceLifetime.Singleton);

        var serviceProvider = services.BuildServiceProvider();
        var validationService = serviceProvider.GetService<IValidationService>();
        validationService.Should().NotBeNull();
    }

    #endregion

    #region RegisterReportingOnly

    [Fact]
    public void RegisterReportingOnly_WithNullServiceCollection_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.RegisterReportingOnly());
    }

    [Fact]
    public void RegisterReportingOnly_WithValidServiceCollection_RegistersReportingService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.RegisterReportingOnly();

        // Assert
        services.Should().Contain(d => d.ServiceType == typeof(IReportingService) && d.Lifetime == ServiceLifetime.Singleton);

        var serviceProvider = services.BuildServiceProvider();
        var reportingService = serviceProvider.GetService<IReportingService>();
        reportingService.Should().NotBeNull();
    }

    #endregion

    #region ConfigureAnalyzer

    [Fact]
    public void ConfigureAnalyzer_WithNullServiceCollection_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        Action<AnalyzerConfiguration>? configure = _ => { };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.ConfigureAnalyzer(configure!));
    }

    [Fact]
    public void ConfigureAnalyzer_WithNullConfigureAction_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<AnalyzerConfiguration>? configure = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.ConfigureAnalyzer(configure!));
    }

    [Fact]
    public void ConfigureAnalyzer_WithValidConfiguration_RegistersAnalyzerConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var customDataDirectory = "/custom/data/dir";
        var maxViolations = 500;
        var timeoutSeconds = 600;
        var failOnError = true;

        // Act
        services.ConfigureAnalyzer(config =>
        {
            config.DataDirectory = customDataDirectory;
            config.MaxViolationsToReport = maxViolations;
            config.AnalysisTimeoutSeconds = timeoutSeconds;
            config.FailOnError = failOnError;
        });

        // Assert
        services.Should().Contain(d => d.ServiceType == typeof(AnalyzerConfiguration) && d.Lifetime == ServiceLifetime.Singleton);

        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<AnalyzerConfiguration>();

        config.Should().NotBeNull();
        config.DataDirectory.Should().Be(customDataDirectory);
        config.MaxViolationsToReport.Should().Be(maxViolations);
        config.AnalysisTimeoutSeconds.Should().Be(timeoutSeconds);
        config.FailOnError.Should().Be(failOnError);
    }

    #endregion

    #region AnalyzerConfiguration Properties

    [Fact]
    public void AnalyzerConfiguration_DataDirectory_DefaultsToAppData()
    {
        // Arrange
        var config = new AnalyzerConfiguration();

        // Act
        var dataDirectory = config.DataDirectory;

        // Assert
        dataDirectory.Should().NotBeNullOrWhiteSpace();
        dataDirectory.Should().StartWith(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RoslynGuardAnalyzer"));
    }

    [Fact]
    public void AnalyzerConfiguration_MaxViolationsToReport_DefaultsTo1000()
    {
        // Arrange
        var config = new AnalyzerConfiguration();

        // Act
        var maxViolations = config.MaxViolationsToReport;

        // Assert
        maxViolations.Should().Be(1000);
    }

    [Fact]
    public void AnalyzerConfiguration_AnalysisTimeoutSeconds_DefaultsTo300()
    {
        // Arrange
        var config = new AnalyzerConfiguration();

        // Act
        var timeoutSeconds = config.AnalysisTimeoutSeconds;

        // Assert
        timeoutSeconds.Should().Be(300);
    }

    [Fact]
    public void AnalyzerConfiguration_FailOnError_DefaultsToFalse()
    {
        // Arrange
        var config = new AnalyzerConfiguration();

        // Act
        var failOnError = config.FailOnError;

        // Assert
        failOnError.Should().BeFalse();
    }

    [Fact]
    public void AnalyzerConfiguration_GenerateDetailedReports_DefaultsToTrue()
    {
        // Arrange
        var config = new AnalyzerConfiguration();

        // Act
        var generateDetailedReports = config.GenerateDetailedReports;

        // Assert
        generateDetailedReports.Should().BeTrue();
    }

    [Fact]
    public void AnalyzerConfiguration_DefaultReportFormat_DefaultsToText()
    {
        // Arrange
        var config = new AnalyzerConfiguration();

        // Act
        var defaultReportFormat = config.DefaultReportFormat;

        // Assert
        defaultReportFormat.Should().Be("text");
    }

    [Fact]
    public void AnalyzerConfiguration_LogLevel_DefaultsTo2()
    {
        // Arrange
        var config = new AnalyzerConfiguration();

        // Act
        var logLevel = config.LogLevel;

        // Assert
        logLevel.Should().Be(2);
    }

    [Fact]
    public void AnalyzerConfiguration_UseParallelAnalysis_DefaultsToTrue()
    {
        // Arrange
        var config = new AnalyzerConfiguration();

        // Act
        var useParallelAnalysis = config.UseParallelAnalysis;

        // Assert
        useParallelAnalysis.Should().BeTrue();
    }

    [Fact]
    public void AnalyzerConfiguration_MaxParallelThreads_DefaultsToProcessorCount()
    {
        // Arrange
        var config = new AnalyzerConfiguration();

        // Act
        var maxParallelThreads = config.MaxParallelThreads;

        // Assert
        maxParallelThreads.Should().BeGreaterThan(0);
        maxParallelThreads.Should().Be(Environment.ProcessorCount);
    }

    #endregion
}
