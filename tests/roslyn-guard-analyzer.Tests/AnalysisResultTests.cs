using System;
using System.Collections.Generic;
using System.Linq;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class AnalysisResultTests
{
    [Fact]
    public void Constructor_InitializesDefaultValues()
    {
        // Arrange & Act
        var result = new AnalysisResult("TestProject", "C:\\TestPath");

        // Assert
        Assert.Equal("TestProject", result.ProjectName);
        Assert.Equal("C:\\TestPath", result.ProjectPath);
        Assert.NotNull(result.Id);
        Assert.Empty(result.Violations);
        Assert.Empty(result.AnalyzedElements);
        Assert.True(result.AnalysisSucceeded);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void AddViolation_UpdatesViolationsAndStatistics()
    {
        // Arrange
        var result = new AnalysisResult();
        var violation = new RuleViolation { Category = RuleCategory.CodeStructure, Severity = SeverityLevel.Warning };

        // Act
        result.AddViolation(violation);

        // Assert
        Assert.Single(result.Violations);
        Assert.Equal(1, result.ViolationsByCategory["CodeStructure"]);
        Assert.Equal(1, result.ViolationsBySeverity["Warning"]);
    }

    [Fact]
    public void AddViolation_NullViolation_ThrowsArgumentNullException()
    {
        // Arrange
        var result = new AnalysisResult();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.AddViolation(null!));
    }

    [Fact]
    public void AddAnalyzedElement_UpdatesElementsAndCount()
    {
        // Arrange
        var result = new AnalysisResult();
        var element = new CodeElement { Name = "TestClass" };

        // Act
        result.AddAnalyzedElement(element);

        // Assert
        Assert.Single(result.AnalyzedElements);
        Assert.Equal(1, result.TotalElementsAnalyzed);
    }

    [Fact]
    public void GetViolationCountBySeverity_ReturnsCorrectCount()
    {
        // Arrange
        var result = new AnalysisResult();
        result.AddViolation(new RuleViolation { Severity = SeverityLevel.Error });
        result.AddViolation(new RuleViolation { Severity = SeverityLevel.Warning });
        result.AddViolation(new RuleViolation { Severity = SeverityLevel.Error });

        // Act
        var errorCount = result.GetViolationCountBySeverity(SeverityLevel.Error);

        // Assert
        Assert.Equal(2, errorCount);
    }

    [Fact]
    public void MarkAsFailed_SetsFailedState()
    {
        // Arrange
        var result = new AnalysisResult();
        var errorMessage = "Analysis failed due to timeout";

        // Act
        result.MarkAsFailed(errorMessage);

        // Assert
        Assert.False(result.AnalysisSucceeded);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.NotEqual(default, result.AnalysisEndTime);
    }

    [Fact]
    public void GetDuration_ReturnsCorrectTimeSpan()
    {
        // Arrange
        var result = new AnalysisResult();
        result.AnalysisStartTime = DateTime.UtcNow.AddMinutes(-5);
        result.MarkAsCompleted();

        // Act
        var duration = result.GetDuration();

        // Assert
        Assert.True(duration.TotalMinutes >= 5);
    }
}
