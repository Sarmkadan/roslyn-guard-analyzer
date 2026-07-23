// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using Xunit;
using RoslynGuardAnalyzer.Cli;

namespace RoslynGuardAnalyzer.Tests;

public class CliOptionsExtensionsTests
{
    [Fact]
    public void RequiresProjectPath_Returns_False_When_ProjectPath_Is_Specified()
    {
        // Arrange
        var options = new CliOptions { ProjectPath = "path/to/project" };

        // Act
        var result = CliOptionsExtensions.RequiresProjectPath(options);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RequiresProjectPath_Returns_False_When_FilePath_Is_Specified()
    {
        // Arrange
        var options = new CliOptions { FilePath = "path/to/file" };

        // Act
        var result = CliOptionsExtensions.RequiresProjectPath(options);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RequiresProjectPath_Returns_True_When_ProjectPath_And_FilePath_Are_Empty()
    {
        // Arrange
        var options = new CliOptions();

        // Act
        var result = CliOptionsExtensions.RequiresProjectPath(options);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void RequiresProjectPath_Throws_ArgumentNullException_When_Options_Are_Null()
    {
        // Arrange
        CliOptions? options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CliOptionsExtensions.RequiresProjectPath(options));
    }

    [Fact]
    public void ValidateOutputSettings_Does_Not_Throw_When_Report_Type_Is_Specified()
    {
        // Arrange
        var options = new CliOptions { GenerateReport = true, ReportType = "report type" };

        // Act & Assert
        CliOptionsExtensions.ValidateOutputSettings(options);
    }

    [Fact]
    public void ValidateOutputSettings_Throws_ArgumentException_When_Report_Type_Is_Not_Specified()
    {
        // Arrange
        var options = new CliOptions { GenerateReport = true };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CliOptionsExtensions.ValidateOutputSettings(options));
    }

    [Fact]
    public void ValidateOutputSettings_Does_Not_Throw_When_Output_File_Is_Specified()
    {
        // Arrange
        var options = new CliOptions { OutputFile = "output file" };

        // Act & Assert
        CliOptionsExtensions.ValidateOutputSettings(options);
    }

    [Fact]
    public void ValidateOutputSettings_Throws_ArgumentException_When_Output_File_Is_Specified_With_Console_Output_Format()
    {
        // Arrange
        var options = new CliOptions { OutputFile = "output file", OutputFormat = "console" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CliOptionsExtensions.ValidateOutputSettings(options));
    }

    [Fact]
    public void GetMaxDegreeOfParallelism_Returns_Processor_Count_When_Max_Parallel_Threads_Is_Zero()
    {
        // Arrange
        var options = new CliOptions { MaxParallelThreads = 0 };

        // Act
        var result = CliOptionsExtensions.GetMaxDegreeOfParallelism(options);

        // Assert
        Assert.Equal(Environment.ProcessorCount, result);
    }

    [Fact]
    public void GetMaxDegreeOfParallelism_Returns_Max_Parallel_Threads_When_Max_Parallel_Threads_Is_Positive()
    {
        // Arrange
        var options = new CliOptions { MaxParallelThreads = 10 };

        // Act
        var result = CliOptionsExtensions.GetMaxDegreeOfParallelism(options);

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public void GetMaxDegreeOfParallelism_Throws_ArgumentNullException_When_Options_Are_Null()
    {
        // Arrange
        CliOptions? options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CliOptionsExtensions.GetMaxDegreeOfParallelism(options));
    }
}
