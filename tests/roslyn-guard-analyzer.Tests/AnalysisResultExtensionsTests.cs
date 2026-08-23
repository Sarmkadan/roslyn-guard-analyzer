// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;
using RoslynGuardAnalyzer.Domain.Models;
using SeverityLevel = RoslynGuardAnalyzer.Core.SeverityLevel;

namespace RoslynGuardAnalyzer.Tests;

public class AnalysisResultExtensionsTests
{
    [Fact]
    public void GetSummary_Returns_Summary_String()
    {
        // Arrange
        var result = new AnalysisResult
        {
            ProjectName = "Project Name",
            ProjectPath = "Project Path",
            TotalFilesAnalyzed = 10,
            TotalElementsAnalyzed = 100,
            AnalysisSucceeded = true
        };

        // Act
        var summary = AnalysisResultExtensions.GetSummary(result);

        // Assert
        Assert.Equal("Analysis of Project Name (Project Path): 10 files, 100 elements, succeeded", summary);
    }

    [Fact]
    public void GetSummary_Throws_ArgumentNullException_When_Result_Is_Null()
    {
        // Arrange
        AnalysisResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AnalysisResultExtensions.GetSummary(result));
    }

    [Fact]
    public void GetTotalViolationsBySeverity_Returns_Empty_Dictionary_When_Violations_Are_Null()
    {
        // Arrange
        var result = new AnalysisResult();

        // Act
        var violations = AnalysisResultExtensions.GetTotalViolationsBySeverity(result);

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public void GetTotalViolationsBySeverity_Returns_Dictionary_With_Severity_Counts()
    {
        // Arrange
        var result = new AnalysisResult
        {
            ViolationsBySeverity = new Dictionary<string, int>
            {
                {"Severity1", 10},
                {"Severity2", 20}
            }
        };

        // Act
        var violations = AnalysisResultExtensions.GetTotalViolationsBySeverity(result);

        // Assert
        Assert.Equal(2, violations.Count);
        Assert.Equal(10, violations["Severity1"]);
        Assert.Equal(20, violations["Severity2"]);
    }

    [Fact]
    public void GetElapsedTime_Returns_TimeSpan_When_Result_Has_Valid_Start_And_End_Times()
    {
        // Arrange
        var result = new AnalysisResult
        {
            AnalysisStartTime = DateTime.Now,
            AnalysisEndTime = DateTime.Now.AddHours(1)
        };

        // Act
        var elapsedTime = AnalysisResultExtensions.GetElapsedTime(result);

        // Assert
        Assert.NotEqual(TimeSpan.Zero, elapsedTime);
    }

    [Fact]
    public void GetElapsedTime_Throws_ArgumentException_When_Result_Has_Invalid_Start_And_End_Times()
    {
        // Arrange
        var result = new AnalysisResult
        {
            AnalysisStartTime = DateTime.Now,
            AnalysisEndTime = DateTime.Now.AddHours(-1)
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => AnalysisResultExtensions.GetElapsedTime(result));
    }

    [Fact]
    public void ToCsv_Returns_Csv_String()
    {
        // Arrange
        var result = new AnalysisResult
        {
            ProjectName = "Project Name",
            ProjectPath = "Project Path",
            TotalFilesAnalyzed = 10,
            TotalElementsAnalyzed = 100,
            AnalysisSucceeded = true
        };

        // Act
        var csv = AnalysisResultExtensions.ToCsv(result);

        // Assert
        Assert.NotEmpty(csv);
    }

    [Fact]
    public void ToCsv_Throws_ArgumentNullException_When_Result_Is_Null()
    {
        // Arrange
        AnalysisResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AnalysisResultExtensions.ToCsv(result));
    }

    [Fact]
    public void WorstSeverity_Returns_Highest_Severity_Among_Violations()
    {
        // Arrange
        var result = new AnalysisResult();
        result.AddViolation(new RuleViolation("R1", "Test", "Test", "a.cs") { Severity = SeverityLevel.Info });
        result.AddViolation(new RuleViolation("R2", "Test", "Test", "b.cs") { Severity = SeverityLevel.Error });
        result.AddViolation(new RuleViolation("R3", "Test", "Test", "c.cs") { Severity = SeverityLevel.Warning });

        // Act
        var worst = result.WorstSeverity();

        // Assert
        Assert.Equal(SeverityLevel.Error, worst);
    }

    [Fact]
    public void WorstSeverity_Returns_Critical_When_Present()
    {
        // Arrange
        var result = new AnalysisResult();
        result.AddViolation(new RuleViolation("R1", "Test", "Test", "a.cs") { Severity = SeverityLevel.Critical });
        result.AddViolation(new RuleViolation("R2", "Test", "Test", "b.cs") { Severity = SeverityLevel.Error });

        // Act
        var worst = result.WorstSeverity();

        // Assert
        Assert.Equal(SeverityLevel.Critical, worst);
    }

    [Fact]
    public void WorstSeverity_Returns_Null_When_No_Violations()
    {
        // Arrange
        var result = new AnalysisResult();

        // Act
        var worst = result.WorstSeverity();

        // Assert
        Assert.Null(worst);
    }

    [Fact]
    public void WorstSeverity_Throws_ArgumentNullException_When_Result_Is_Null()
    {
        // Arrange
        AnalysisResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.WorstSeverity());
    }

    [Fact]
    public void GroupByFile_Returns_Violations_Grouped_By_File()
    {
        // Arrange
        var result = new AnalysisResult();
        result.AddViolation(new RuleViolation("R1", "Test", "Test", "a.cs"));
        result.AddViolation(new RuleViolation("R2", "Test", "Test", "a.cs"));
        result.AddViolation(new RuleViolation("R3", "Test", "Test", "b.cs"));

        // Act
        var groups = result.GroupByFile().ToList();

        // Assert
        Assert.Equal(2, groups.Count);
        Assert.Equal(2, groups.Single(g => g.Key == "a.cs").Count());
        Assert.Equal(1, groups.Single(g => g.Key == "b.cs").Count());
    }

    [Fact]
    public void GroupByFile_Returns_Empty_When_No_Violations()
    {
        // Arrange
        var result = new AnalysisResult();

        // Act
        var groups = result.GroupByFile().ToList();

        // Assert
        Assert.Empty(groups);
    }

    [Fact]
    public void GroupByFile_Throws_ArgumentNullException_When_Result_Is_Null()
    {
        // Arrange
        AnalysisResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.GroupByFile());
    }

    [Fact]
    public void ToExitCode_Returns_Zero_When_Clean()
    {
        // Arrange
        var result = new AnalysisResult();
        result.AddViolation(new RuleViolation("R1", "Test", "Test", "a.cs") { Severity = SeverityLevel.Info });

        // Act
        var exitCode = result.ToExitCode();

        // Assert
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void ToExitCode_Returns_One_When_Warnings_Present()
    {
        // Arrange
        var result = new AnalysisResult();
        result.AddViolation(new RuleViolation("R1", "Test", "Test", "a.cs") { Severity = SeverityLevel.Warning });

        // Act
        var exitCode = result.ToExitCode();

        // Assert
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public void ToExitCode_Returns_Two_When_Errors_Present()
    {
        // Arrange
        var result = new AnalysisResult();
        result.AddViolation(new RuleViolation("R1", "Test", "Test", "a.cs") { Severity = SeverityLevel.Error });

        // Act
        var exitCode = result.ToExitCode();

        // Assert
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public void ToExitCode_Returns_Two_When_Critical_Present()
    {
        // Arrange
        var result = new AnalysisResult();
        result.AddViolation(new RuleViolation("R1", "Test", "Test", "a.cs") { Severity = SeverityLevel.Critical });

        // Act
        var exitCode = result.ToExitCode();

        // Assert
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public void ToExitCode_Returns_Two_When_Errors_Outrank_Warnings()
    {
        // Arrange
        var result = new AnalysisResult();
        result.AddViolation(new RuleViolation("R1", "Test", "Test", "a.cs") { Severity = SeverityLevel.Warning });
        result.AddViolation(new RuleViolation("R2", "Test", "Test", "b.cs") { Severity = SeverityLevel.Error });

        // Act
        var exitCode = result.ToExitCode();

        // Assert
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public void ToExitCode_Throws_ArgumentNullException_When_Result_Is_Null()
    {
        // Arrange
        AnalysisResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.ToExitCode());
    }
}
