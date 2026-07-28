using System;
using System.Collections.Generic;
using RoslynGuardAnalyzer.Configuration;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class RoslynGuardAnalyzerOptionsValidationTests
{
    private static RoslynGuardAnalyzerOptions CreateValidOptions()
    {
        return new RoslynGuardAnalyzerOptions
        {
            ProjectPath = "/tmp/project.csproj",
            AnalysisTimeoutSeconds = 30,
            MaxViolationsToReport = 1000,
            LogLevel = 2,
            OutputFormat = "json",
            ReportType = "summary",
            MinimumSeverity = "Medium",
            MaxParallelThreads = 4,
            RuleFilter = new List<string>(),
            ExcludePatterns = new List<string>()
        };
    }

    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act
        var problems = options.Validate();

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act
        var isValid = options.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act / Assert
        var exception = Record.Exception(() => options.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_NullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        RoslynGuardAnalyzerOptions? options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options!.Validate());
    }

    [Fact]
    public void IsValid_NullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        RoslynGuardAnalyzerOptions? options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options!.IsValid());
    }

    [Fact]
    public void EnsureValid_InvalidOptions_ThrowsArgumentException_WithProblems()
    {
        // Arrange: create options with several problems
        var options = new RoslynGuardAnalyzerOptions
        {
            ProjectPath = "   ",               // invalid
            AnalysisTimeoutSeconds = 0,       // invalid
            MaxViolationsToReport = 0,        // invalid
            LogLevel = -1,                    // invalid
            OutputFormat = "yaml",            // invalid
            ReportType = "unknown",           // invalid
            MinimumSeverity = "None",         // invalid
            MaxParallelThreads = 0,           // invalid
            RuleFilter = null!,               // invalid (null)
            ExcludePatterns = null!           // invalid (null)
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() => options.EnsureValid());

        // Assert
        var message = ex.Message;
        Assert.Contains("ProjectPath cannot be null or whitespace.", message);
        Assert.Contains("AnalysisTimeoutSeconds must be greater than 0.", message);
        Assert.Contains("MaxViolationsToReport must be at least 1.", message);
        Assert.Contains("LogLevel must be between 0 and 4 (inclusive).", message);
        Assert.Contains("OutputFormat must be one of: text, json, csv, html, xml.", message);
        Assert.Contains("ReportType must be one of: summary, detailed, full.", message);
        Assert.Contains("MinimumSeverity must be one of: Low, Medium, High, Critical.", message);
        Assert.Contains("MaxParallelThreads must be at least 1.", message);
        Assert.Contains("RuleFilter cannot be null.", message);
        Assert.Contains("ExcludePatterns cannot be null.", message);
    }

    [Theory]
    [InlineData(1, 0, 0, 0, "text", "summary", "Low", 1)]
    [InlineData(100000, 4, 4, 4, "xml", "full", "Critical", 64)]
    public void Validate_BoundaryValues_AreAccepted(
        int maxViolations,
        int logLevel,
        int maxParallelThreads,
        int dummy, // placeholder to keep signature simple
        string outputFormat,
        string reportType,
        string minimumSeverity,
        int ignored)
    {
        // Arrange
        var options = CreateValidOptions();
        options.MaxViolationsToReport = maxViolations;
        options.LogLevel = logLevel;
        options.MaxParallelThreads = maxParallelThreads;
        options.OutputFormat = outputFormat;
        options.ReportType = reportType;
        options.MinimumSeverity = minimumSeverity;

        // Act
        var problems = options.Validate();

        // Assert
        Assert.Empty(problems);
    }
}
