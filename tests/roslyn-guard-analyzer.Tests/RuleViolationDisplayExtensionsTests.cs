#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="RuleViolationDisplayExtensions"/> extension methods.
/// Tests the console-friendly representations and grouping helpers for rule violations.
/// </summary>
public class RuleViolationDisplayExtensionsTests
{
    #region ToConsoleLine Tests

    [Fact]
    public void ToConsoleLine_WithValidViolation_ReturnsFormattedString()
    {
        // Arrange
        var violation = new RuleViolation("CA1822", "Mark members as static", "Test message", "TestClass.cs")
        {
            Severity = SeverityLevel.Error,
            LineNumber = 42
        };

        // Act
        var result = violation.ToConsoleLine();

        // Assert
        result.Should().Contain("Error");
        result.Should().Contain("TestClass.cs:42");
        result.Should().Contain("Test message");
    }

    [Fact]
    public void ToConsoleLine_WithCriticalSeverity_ReturnsCorrectFormat()
    {
        // Arrange
        var violation = new RuleViolation("CA1000", "Critical Rule", "Critical issue", "Program.cs")
        {
            Severity = SeverityLevel.Critical,
            LineNumber = 100
        };

        // Act
        var result = violation.ToConsoleLine();

        // Assert
        result.Should().Contain("Critical");
        result.Should().Contain("Program.cs:100");
        result.Should().Contain("Critical issue");
    }

    [Fact]
    public void ToConsoleLine_WithWarningSeverity_ReturnsCorrectFormat()
    {
        // Arrange
        var violation = new RuleViolation("CA1001", "Warning Rule", "Warning message", "Service.cs")
        {
            Severity = SeverityLevel.Warning,
            LineNumber = 5
        };

        // Act
        var result = violation.ToConsoleLine();

        // Assert
        result.Should().Contain("Warning");
        result.Should().Contain("Service.cs:5");
        result.Should().Contain("Warning message");
    }

    [Fact]
    public void ToConsoleLine_WithInfoSeverity_ReturnsCorrectFormat()
    {
        // Arrange
        var violation = new RuleViolation("CA1002", "Info Rule", "Informational message", "Documentation.cs")
        {
            Severity = SeverityLevel.Info,
            LineNumber = 1
        };

        // Act
        var result = violation.ToConsoleLine();

        // Assert
        result.Should().Contain("Info");
        result.Should().Contain("Documentation.cs:1");
        result.Should().Contain("Informational message");
    }

    [Fact]
    public void ToConsoleLine_WithNullViolation_ThrowsArgumentNullException()
    {
        // Arrange
        RuleViolation? violation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => violation!.ToConsoleLine());
    }

    [Fact]
    public void ToConsoleLine_WithNullFilePath_HandlesGracefully()
    {
        // Arrange
        var violation = new RuleViolation("CA1822", "Test Rule", "Test message", null)
        {
            Severity = SeverityLevel.Error,
            LineNumber = 1
        };

        // Act
        var result = violation.ToConsoleLine();

        // Assert
        result.Should().Contain("Error");
        result.Should().Contain(":1");
        result.Should().Contain("Test message");
    }

    [Fact]
    public void ToConsoleLine_WithEmptyFilePath_HandlesGracefully()
    {
        // Arrange
        var violation = new RuleViolation("CA1822", "Test Rule", "Test message", string.Empty)
        {
            Severity = SeverityLevel.Error,
            LineNumber = 1
        };

        // Act
        var result = violation.ToConsoleLine();

        // Assert
        result.Should().Contain("Error");
        result.Should().Contain(":1");
        result.Should().Contain("Test message");
    }

    #endregion

    #region GetSeverityColor Tests

    [Fact]
    public void GetSeverityColor_WithCritical_ReturnsRed()
    {
        // Arrange & Act
        var color = SeverityLevel.Critical.GetSeverityColor();

        // Assert
        color.Should().Be(ConsoleColor.Red);
    }

    [Fact]
    public void GetSeverityColor_WithError_ReturnsDarkRed()
    {
        // Arrange & Act
        var color = SeverityLevel.Error.GetSeverityColor();

        // Assert
        color.Should().Be(ConsoleColor.DarkRed);
    }

    [Fact]
    public void GetSeverityColor_WithWarning_ReturnsYellow()
    {
        // Arrange & Act
        var color = SeverityLevel.Warning.GetSeverityColor();

        // Assert
        color.Should().Be(ConsoleColor.Yellow);
    }

    [Fact]
    public void GetSeverityColor_WithInfo_ReturnsCyan()
    {
        // Arrange & Act
        var color = SeverityLevel.Info.GetSeverityColor();

        // Assert
        color.Should().Be(ConsoleColor.Cyan);
    }

    [Fact]
    public void GetSeverityColor_WithUnknownSeverity_ReturnsWhite()
    {
        // Arrange & Act
        var color = ((SeverityLevel)99).GetSeverityColor();

        // Assert
        color.Should().Be(ConsoleColor.White);
    }

    #endregion

    #region GroupByFile Tests

    [Fact]
    public void GroupByFile_WithValidViolations_ReturnsGroupedByFilePath()
    {
        // Arrange
        var violations = new List<RuleViolation>
        {
            new RuleViolation("CA1822", "Rule1", "Message1", "File1.cs") { LineNumber = 1 },
            new RuleViolation("CA1823", "Rule2", "Message2", "File2.cs") { LineNumber = 2 },
            new RuleViolation("CA1824", "Rule3", "Message3", "File1.cs") { LineNumber = 3 }, // Same file as first
            new RuleViolation("CA1825", "Rule4", "Message4", "File3.cs") { LineNumber = 4 }
        };

        // Act
        var groups = violations.GroupByFile().ToList();

        // Assert
        groups.Should().HaveCount(3);
        groups.Should().Contain(g => g.Key == "File1.cs" && g.Count() == 2);
        groups.Should().Contain(g => g.Key == "File2.cs" && g.Count() == 1);
        groups.Should().Contain(g => g.Key == "File3.cs" && g.Count() == 1);
    }

    [Fact]
    public void GroupByFile_WithEmptyCollection_ReturnsEmptyGroupings()
    {
        // Arrange
        var violations = new List<RuleViolation>();

        // Act
        var groups = violations.GroupByFile();

        // Assert
        groups.Should().BeEmpty();
    }

    [Fact]
    public void GroupByFile_WithNullViolations_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<RuleViolation>? violations = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => violations!.GroupByFile());
    }

    [Fact]
    public void GroupByFile_WithSingleViolation_ReturnsSingleGroup()
    {
        // Arrange
        var violations = new List<RuleViolation>
        {
            new RuleViolation("CA1822", "Rule1", "Message1", "SingleFile.cs") { LineNumber = 1 }
        };

        // Act
        var groups = violations.GroupByFile().ToList();

        // Assert
        groups.Should().HaveCount(1);
        groups[0].Key.Should().Be("SingleFile.cs");
        groups[0].Should().ContainSingle();
    }

    [Fact]
    public void GroupByFile_WithSameFileDifferentPaths_ReturnsSeparateGroups()
    {
        // Arrange - using different path formats for same logical file
        var violations = new List<RuleViolation>
        {
            new RuleViolation("CA1822", "Rule1", "Message1", "/project/File.cs") { LineNumber = 1 },
            new RuleViolation("CA1823", "Rule2", "Message2", "File.cs") { LineNumber = 2 },
            new RuleViolation("CA1824", "Rule3", "Message3", @".\File.cs") { LineNumber = 3 }
        };

        // Act
        var groups = violations.GroupByFile().ToList();

        // Assert - different paths should create different groups
        groups.Should().HaveCount(3);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ToConsoleLine_And_GetSeverityColor_IntegrationTest()
    {
        // Arrange
        var violation = new RuleViolation("CA1000", "Integration Test Rule", "Test integration", "Integration.cs")
        {
            Severity = SeverityLevel.Warning,
            LineNumber = 10
        };

        // Act
        var consoleLine = violation.ToConsoleLine();
        var color = violation.Severity.GetSeverityColor();

        // Assert
        consoleLine.Should().Contain("Warning");
        consoleLine.Should().Contain("Integration.cs:10");
        consoleLine.Should().Contain("Test integration");
        color.Should().Be(ConsoleColor.Yellow);
    }

    [Fact]
    public void GroupByFile_And_ToConsoleLine_IntegrationTest()
    {
        // Arrange
        var violations = new List<RuleViolation>
        {
            new RuleViolation("CA1822", "Rule1", "Message1", "Program.cs") { Severity = SeverityLevel.Error, LineNumber = 1 },
            new RuleViolation("CA1823", "Rule2", "Message2", "Program.cs") { Severity = SeverityLevel.Warning, LineNumber = 5 },
            new RuleViolation("CA1824", "Rule3", "Message3", "Startup.cs") { Severity = SeverityLevel.Info, LineNumber = 10 }
        };

        // Act
        var groups = violations.GroupByFile().ToList();
        var programCsGroup = groups.First(g => g.Key == "Program.cs");
        var consoleLines = programCsGroup.Select(v => v.ToConsoleLine()).ToList();

        // Assert
        groups.Should().HaveCount(2);
        programCsGroup.Should().HaveCount(2);
        consoleLines.Should().Contain(line => line.Contains("Error") && line.Contains("Program.cs:1") && line.Contains("Message1"));
        consoleLines.Should().Contain(line => line.Contains("Warning") && line.Contains("Program.cs:5") && line.Contains("Message2"));
    }

    #endregion
}