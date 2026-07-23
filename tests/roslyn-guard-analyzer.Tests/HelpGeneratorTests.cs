using System;
using Xunit;
using RoslynGuardAnalyzer.Cli;

namespace RoslynGuardAnalyzer.Tests;

public class HelpGeneratorTests
{
    [Fact]
    public void GenerateFullHelp_Returns_NonEmptyString_And_Contains_Sections()
    {
        // Act
        var fullHelp = HelpGenerator.GenerateFullHelp();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(fullHelp));
        Assert.Contains("USAGE:", fullHelp);
        Assert.Contains("EXAMPLES:", fullHelp);
        Assert.Contains("OPTIONS:", fullHelp);
        Assert.Contains("SUPPORTED RULES:", fullHelp);
        Assert.Contains("EXIT CODES:", fullHelp);
        Assert.Contains("Help & Information:", fullHelp);
    }

    [Fact]
    public void GenerateBriefHelp_Returns_NonEmptyString_And_Contains_CommonOptions()
    {
        // Act
        var briefHelp = HelpGenerator.GenerateBriefHelp();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(briefHelp));
        Assert.Contains("Usage:", briefHelp);
        Assert.Contains("Common options:", briefHelp);
        Assert.Contains("--project PATH", briefHelp);
        Assert.Contains("--file PATH", briefHelp);
        Assert.Contains("--format FORMAT", briefHelp);
        Assert.Contains("--output FILE", briefHelp);
        Assert.Contains("--verbose", briefHelp);
    }

    [Fact]
    public void GenerateVersion_Returns_CorrectFormat()
    {
        // Act
        var versionInfo = HelpGenerator.GenerateVersion();

        // Assert
        Assert.Contains("roslyn-guard-analyzer version 1.0.0", versionInfo);
        Assert.Contains("License: MIT", versionInfo);
        Assert.EndsWith(Environment.NewLine, versionInfo);
    }

    [Fact]
    public void GenerateErrorMessage_WithError_Returns_Message_With_Error_And_Help()
    {
        // Arrange
        var error = "Something went wrong";

        // Act
        var message = HelpGenerator.GenerateErrorMessage(error);

        // Assert
        Assert.StartsWith("Error: Something went wrong", message);
        Assert.Contains("Use 'roslyn-guard-analyzer --help' for more information.", message);
    }

    [Fact]
    public void GenerateErrorMessage_WithNull_Returns_Message_With_Help()
    {
        // Arrange
        string? error = null;

        // Act
        var message = HelpGenerator.GenerateErrorMessage(error!);

        // Assert
        Assert.StartsWith("Error: ", message);
        Assert.Contains("Use 'roslyn-guard-analyzer --help' for more information.", message);
    }

    [Fact]
    public void GenerateUsageSummary_Returns_UsageAnd_Help()
    {
        // Act
        var summary = HelpGenerator.GenerateUsageSummary();

        // Assert
        Assert.Contains("Usage: roslyn-guard-analyzer [OPTIONS] [PROJECT_PATH]", summary);
        Assert.Contains("Use 'roslyn-guard-analyzer --help' for detailed information", summary);
    }

    [Fact]
    public void GenerateFullHelp_EndsWith_NewLine()
    {
        // Act
        var fullHelp = HelpGenerator.GenerateFullHelp();

        // Assert
        Assert.EndsWith(Environment.NewLine, fullHelp);
    }

    [Fact]
    public void GenerateBriefHelp_EndsWith_NewLine()
    {
        // Act
        var briefHelp = HelpGenerator.GenerateBriefHelp();

        // Assert
        Assert.EndsWith(Environment.NewLine, briefHelp);
    }
}
