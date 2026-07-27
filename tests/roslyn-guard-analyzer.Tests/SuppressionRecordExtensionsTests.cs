using System;
using FluentAssertions;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Suppressions;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class SuppressionRecordExtensionsTests
{
    [Fact]
    public void IsExpired_WithExpiredRecord_ReturnsTrue()
    {
        // Arrange
        var record = new SuppressionRecord { ExpiresAt = DateTime.UtcNow.AddHours(-1) };

        // Act
        var result = record.IsExpired();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WithNotExpiredRecord_ReturnsFalse()
    {
        // Arrange
        var record = new SuppressionRecord { ExpiresAt = DateTime.UtcNow.AddHours(1) };

        // Act
        var result = record.IsExpired();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetDescription_WithValidRecord_ReturnsDescription()
    {
        // Arrange
        var record = new SuppressionRecord
        {
            RuleId = "RuleId",
            TargetFile = "TargetFile",
            TargetElement = "TargetElement",
            Justification = "Justification",
            Author = "Author"
        };

        // Act
        var result = record.GetDescription();

        // Assert
        result.Should().Contain("RuleId");
        result.Should().Contain("TargetFile");
        result.Should().Contain("TargetElement");
        result.Should().Contain("Justification");
        result.Should().Contain("Author");
    }

    [Fact]
    public void MatchesRuleAndFile_WithMatchingRecord_ReturnsTrue()
    {
        // Arrange
        var record = new SuppressionRecord { RuleId = "RuleId", TargetFile = "TargetFile" };

        // Act
        var result = record.MatchesRuleAndFile("RuleId", "TargetFile");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MatchesRuleAndFile_WithNonMatchingRecord_ReturnsFalse()
    {
        // Arrange
        var record = new SuppressionRecord { RuleId = "RuleId", TargetFile = "TargetFile" };

        // Act
        var result = record.MatchesRuleAndFile("DifferentRuleId", "TargetFile");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithNullRecord_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((SuppressionRecord?)null)!.IsExpired());
    }

    [Fact]
    public void GetDescription_WithNullRecord_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((SuppressionRecord?)null)!.GetDescription());
    }

    [Fact]
    public void MatchesRuleAndFile_WithNullRecord_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((SuppressionRecord?)null)!.MatchesRuleAndFile("RuleId", "TargetFile"));
    }
}
