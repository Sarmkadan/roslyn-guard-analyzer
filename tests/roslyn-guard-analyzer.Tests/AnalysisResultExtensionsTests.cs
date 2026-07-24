// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;
using RoslynGuardAnalyzer.Domain.Models;

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
}
