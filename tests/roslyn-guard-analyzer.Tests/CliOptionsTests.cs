// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using Xunit;
using RoslynGuardAnalyzer.Cli;
using RoslynGuardAnalyzer.Core; // for SeverityLevel

namespace RoslynGuardAnalyzer.Tests;

public class CliOptionsTests
{
    [Fact]
    public void Validate_ReturnsTrue_WhenProjectPathIsProvided_AndAllDefaultsAreValid()
    {
        // Arrange
        var options = new CliOptions
        {
            ProjectPath = "MyProject.csproj",
            OutputFormat = "json",
            MaxParallelThreads = 2,
            AnalysisTimeoutSeconds = 60,
            LogLevel = 2,
            RuleFilter = new List<string>()
        };

        // Act
        var isValid = options.Validate(out var errors);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_Fails_WhenNeitherProjectNorFileIsSpecified()
    {
        // Arrange
        var options = new CliOptions
        {
            OutputFormat = "text"
        };

        // Act
        var isValid = options.Validate(out var errors);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Either --project or --file must be specified"));
    }

    [Fact]
    public void Validate_Fails_WhenBothProjectAndFileAreSpecified()
    {
        // Arrange
        var options = new CliOptions
        {
            ProjectPath = "proj.csproj",
            FilePath = "file.cs",
            OutputFormat = "text"
        };

        // Act
        var isValid = options.Validate(out var errors);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Cannot specify both --project and --file"));
    }

    [Theory]
    [InlineData("xml", true)]
    [InlineData("invalidformat", false)]
    public void Validate_OutputFormatValidation(string format, bool expectedValid)
    {
        // Arrange
        var options = new CliOptions
        {
            ProjectPath = "proj.csproj",
            OutputFormat = format
        };

        // Act
        var isValid = options.Validate(out var errors);

        // Assert
        if (expectedValid)
        {
            Assert.True(isValid);
            Assert.Empty(errors);
        }
        else
        {
            Assert.False(isValid);
            Assert.Contains(errors, e => e.Contains("Invalid output format"));
        }
    }

    [Fact]
    public void Validate_Fails_OnInvalidLogLevel()
    {
        // Arrange
        var options = new CliOptions
        {
            ProjectPath = "proj.csproj",
            LogLevel = 5 // outside 0‑4 range
        };

        // Act
        var isValid = options.Validate(out var errors);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Log level must be between"));
    }

    [Fact]
    public void Validate_Fails_OnInvalidBaselineFileName()
    {
        // Arrange
        var options = new CliOptions
        {
            ProjectPath = "proj.csproj",
            BaselineFile = "invalid|name.json"
        };

        // Act
        var isValid = options.Validate(out var errors);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Baseline file name contains invalid characters"));
    }

    [Fact]
    public void IsAnalysisMode_ReturnsTrue_WhenProjectPathSet_AndHelpVersionNotRequested()
    {
        // Arrange
        var options = new CliOptions
        {
            ProjectPath = "proj.csproj",
            ShowHelp = false,
            ShowVersion = false
        };

        // Act & Assert
        Assert.True(options.IsAnalysisMode);
    }

    [Fact]
    public void IsAnalysisMode_ReturnsFalse_WhenShowHelpIsTrue()
    {
        // Arrange
        var options = new CliOptions
        {
            ShowHelp = true,
            ProjectPath = "proj.csproj"
        };

        // Act & Assert
        Assert.False(options.IsAnalysisMode);
    }

    [Fact]
    public void GetTargetPath_ReturnsProjectPath_IfSet_OtherwiseFilePath()
    {
        // Project path takes precedence
        var options1 = new CliOptions { ProjectPath = "proj.csproj", FilePath = "file.cs" };
        Assert.Equal("proj.csproj", options1.GetTargetPath());

        // When project path is null, file path is used
        var options2 = new CliOptions { FilePath = "file.cs" };
        Assert.Equal("file.cs", options2.GetTargetPath());
    }

    [Fact]
    public void ShouldFailForSeverity_UsesFailOnViolations_WhenFailOnSeverityIsNull()
    {
        // Arrange
        var options = new CliOptions
        {
            FailOnSeverity = null,
            FailOnViolations = true
        };

        // Act
        var result = options.ShouldFailForSeverity(SeverityLevel.Info);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldFailForSeverity_RespectsSeverityThreshold()
    {
        // Arrange
        var options = new CliOptions
        {
            FailOnSeverity = SeverityLevel.Warning,
            FailOnViolations = false
        };

        // Act & Assert
        Assert.False(options.ShouldFailForSeverity(SeverityLevel.Info));    // below threshold
        Assert.True(options.ShouldFailForSeverity(SeverityLevel.Warning)); // equal
        Assert.True(options.ShouldFailForSeverity(SeverityLevel.Error));   // above
    }

    [Fact]
    public void ToString_IncludesAllRelevantProperties()
    {
        // Arrange
        var options = new CliOptions
        {
            ProjectPath = "proj.csproj",
            FilePath = null,
            OutputFormat = "json",
            Verbose = true,
            MaxParallelThreads = 4,
            AnalysisTimeoutSeconds = 120,
            RuleFilter = new List<string> { "R1", "R2" },
            BaselineFile = "baseline.json",
            CreateBaseline = true,
            FailOnSeverity = SeverityLevel.Error
        };

        // Act
        var str = options.ToString();

        // Assert
        Assert.Contains("ProjectPath=proj.csproj", str);
        Assert.Contains("OutputFormat=json", str);
        Assert.Contains("Verbose=True", str);
        Assert.Contains("MaxParallelThreads=4", str);
        Assert.Contains("AnalysisTimeoutSeconds=120", str);
        Assert.Contains("RuleFilterCount=2", str);
        Assert.Contains("BaselineFile=baseline.json", str);
        Assert.Contains("CreateBaseline=True", str);
        Assert.Contains("FailOnSeverity=Error", str);
    }
}
