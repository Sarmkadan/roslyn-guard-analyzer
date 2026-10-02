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
        var options = CreateValidOptions();
        var problems = options.Validate();
        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        var options = CreateValidOptions();
        var isValid = RoslynGuardAnalyzerOptionsValidation.IsValid(options);
        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        var options = CreateValidOptions();
        var exception = Record.Exception(() => RoslynGuardAnalyzerOptionsValidation.EnsureValid(options));
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_NullOptions_ThrowsNullReferenceException()
    {
        // Member Validate() method is called on null - throws NullReferenceException
        RoslynGuardAnalyzerOptions? options = null;
        Assert.Throws<NullReferenceException>(() => options!.Validate());
    }

    [Fact]
    public void ExtensionValidate_NullOptions_ThrowsArgumentNullException()
    {
        // Extension method explicitly checks for null
        Assert.Throws<ArgumentNullException>(() => RoslynGuardAnalyzerOptionsValidation.Validate(null!));
    }

    [Fact]
    public void EnsureValid_InvalidOptions_ThrowsArgumentException_WithProblems()
    {
        // Using the extension method explicitly for validation with detailed messages
        var options = new RoslynGuardAnalyzerOptions
        {
            ProjectPath = "   ",
            AnalysisTimeoutSeconds = 0,
            MaxViolationsToReport = 0,
            LogLevel = -1,
            OutputFormat = "yaml",
            ReportType = "unknown",
            MinimumSeverity = "None",
            MaxParallelThreads = 0,
            RuleFilter = null!,
            ExcludePatterns = null!
        };

        var ex = Assert.Throws<ArgumentException>(() => RoslynGuardAnalyzerOptionsValidation.EnsureValid(options));
        var message = ex.Message;
        Assert.Contains("RoslynGuardAnalyzerOptions validation failed", message);
    }

    [Theory]
    [InlineData(1000, 0, 4, 0, "text", "summary", "Low", 1)]
    [InlineData(100000, 4, 4, 4, "xml", "full", "Critical", 64)]
    public void Validate_BoundaryValues_AreAccepted(
        int maxViolations,
        int logLevel,
        int maxParallelThreads,
        int dummy,
        string outputFormat,
        string reportType,
        string minimumSeverity,
        int ignored)
    {
        var options = CreateValidOptions();
        options.MaxViolationsToReport = maxViolations;
        options.LogLevel = logLevel;
        options.MaxParallelThreads = maxParallelThreads;
        options.OutputFormat = outputFormat;
        options.ReportType = reportType;
        options.MinimumSeverity = minimumSeverity;

        var problems = options.Validate();
        Assert.Empty(problems);
    }
}
