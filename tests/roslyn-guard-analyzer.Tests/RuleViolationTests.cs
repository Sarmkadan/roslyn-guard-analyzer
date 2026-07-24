using System;
using System.Collections.Generic;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public sealed class RuleViolationTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var violation = new RuleViolation();

        // Assert
        violation.Id.Should().NotBeNullOrEmpty();
        violation.RuleId.Should().BeEmpty();
        violation.RuleName.Should().BeEmpty();
        violation.Message.Should().BeEmpty();
        violation.FilePath.Should().BeEmpty();
        violation.Severity.Should().Be(SeverityLevel.Warning);
        violation.LineNumber.Should().Be(0);
        violation.ColumnNumber.Should().Be(0);
        violation.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void ParameterizedConstructor_SetsValuesCorrectly()
    {
        // Act
        var violation = new RuleViolation("rule-id", "rule-name", "message", "file.cs");

        // Assert
        violation.RuleId.Should().Be("rule-id");
        violation.RuleName.Should().Be("rule-name");
        violation.Message.Should().Be("message");
        violation.FilePath.Should().Be("file.cs");
    }

    [Fact]
    public void GetFormattedLocation_ReturnsCorrectFormat()
    {
        // Arrange
        var violation = new RuleViolation("r", "r", "m", "/path/to/file.cs")
        {
            LineNumber = 10,
            ColumnNumber = 5
        };

        // Act
        var location = violation.GetFormattedLocation();

        // Assert
        location.Should().Be("file.cs(10, 5)");
    }

    [Fact]
    public void GetFullDescription_ReturnsCorrectDescription()
    {
        // Arrange
        var violation = new RuleViolation("R001", "RuleName", "Violation message", "/path/to/file.cs")
        {
            LineNumber = 5,
            ColumnNumber = 10,
            Severity = SeverityLevel.Error
        };

        // Act
        var description = violation.GetFullDescription();

        // Assert
        description.Should().Be("[R001] Error: Violation message at file.cs(5, 10)");
    }

    [Theory]
    [InlineData(SeverityLevel.Critical, true)]
    [InlineData(SeverityLevel.Error, true)]
    [InlineData(SeverityLevel.Warning, false)]
    [InlineData(SeverityLevel.Info, false)]
    public void IsCritical_ReturnsCorrectResult(SeverityLevel severity, bool expectedResult)
    {
        // Arrange
        var violation = new RuleViolation { Severity = severity };

        // Act
        var result = violation.IsCritical();

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void AddMetadata_AddsOrUpdatesMetadata()
    {
        // Arrange
        var violation = new RuleViolation();

        // Act
        violation.AddMetadata("key1", "value1");
        violation.AddMetadata("key1", "updated");
        violation.AddMetadata("key2", "value2");

        // Assert
        violation.Metadata.Should().HaveCount(2);
        violation.Metadata["key1"].Should().Be("updated");
        violation.Metadata["key2"].Should().Be("value2");
    }

    [Fact]
    public void AddMetadata_ThrowsExceptionForEmptyKey()
    {
        // Arrange
        var violation = new RuleViolation();

        // Act
        Action act = () => violation.AddMetadata("", "value");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetMetadata_ReturnsValueOrDefault()
    {
        // Arrange
        var violation = new RuleViolation();
        violation.AddMetadata("key", "value");

        // Act
        var value = violation.GetMetadata("key");
        var missing = violation.GetMetadata("missing", "default");

        // Assert
        value.Should().Be("value");
        missing.Should().Be("default");
    }

    [Fact]
    public void WithSeverity_CreatesNewInstanceWithCorrectSeverity()
    {
        // Arrange
        var original = new RuleViolation("id", "name", "msg", "file.cs")
        {
            Severity = SeverityLevel.Info,
            LineNumber = 1
        };
        original.AddMetadata("m", "v");

        // Act
        var copy = original.WithSeverity(SeverityLevel.Critical);

        // Assert
        copy.Should().NotBeSameAs(original);
        copy.Id.Should().NotBe(original.Id);
        copy.Severity.Should().Be(SeverityLevel.Critical);
        copy.RuleId.Should().Be(original.RuleId);
        copy.Metadata.Should().ContainKey("m").WhoseValue.Should().Be("v");
    }

    [Theory]
    [InlineData("id", "name", "msg", "file.cs", 1, 1, true)]
    [InlineData("", "name", "msg", "file.cs", 1, 1, false)]
    [InlineData("id", "", "msg", "file.cs", 1, 1, false)]
    [InlineData("id", "name", "", "file.cs", 1, 1, false)]
    [InlineData("id", "name", "msg", "", 1, 1, false)]
    [InlineData("id", "name", "msg", "file.cs", 0, 1, false)]
    [InlineData("id", "name", "msg", "file.cs", 1, -1, false)]
    public void IsValid_ReturnsExpectedResult(string ruleId, string ruleName, string message, string filePath, int line, int col, bool expected)
    {
        // Arrange
        var violation = new RuleViolation(ruleId, ruleName, message, filePath)
        {
            LineNumber = line,
            ColumnNumber = col
        };

        // Act
        var result = violation.IsValid();

        // Assert
        result.Should().Be(expected);
    }
}
