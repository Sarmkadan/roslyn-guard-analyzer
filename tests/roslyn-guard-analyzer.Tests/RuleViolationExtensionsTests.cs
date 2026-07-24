#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class RuleViolationExtensionsTests
{
    private readonly RuleViolation _baseViolation;

    public RuleViolationExtensionsTests()
    {
        _baseViolation = new RuleViolation(
            "TEST001",
            "TestRule",
            "This is a test violation message",
            "TestFile.cs"
        )
        {
            Severity = SeverityLevel.Warning,
            LineNumber = 42,
            ColumnNumber = 15,
            CodeSnippet = "public class TestClass { }",
            ProjectName = "TestProject",
            Category = RuleCategory.CodeStructure,
            Metadata = new Dictionary<string, string> { { "Key1", "Value1" } }
        };
    }

    [Fact]
    public void WithMessage_WithValidNewMessage_ShouldCreateNewViolationWithUpdatedMessage()
    {
        // Arrange
        var newMessage = "This is a new violation message";

        // Act
        var result = _baseViolation.WithMessage(newMessage);

        // Assert
        result.Should().NotBeSameAs(_baseViolation);
        result.Id.Should().NotBe(_baseViolation.Id);
        result.Message.Should().Be(newMessage);
        result.RuleId.Should().Be(_baseViolation.RuleId);
        result.RuleName.Should().Be(_baseViolation.RuleName);
        result.Severity.Should().Be(_baseViolation.Severity);
        result.FilePath.Should().Be(_baseViolation.FilePath);
        result.LineNumber.Should().Be(_baseViolation.LineNumber);
        result.ColumnNumber.Should().Be(_baseViolation.ColumnNumber);
        result.CodeSnippet.Should().Be(_baseViolation.CodeSnippet);
        result.ProjectName.Should().Be(_baseViolation.ProjectName);
        result.Category.Should().Be(_baseViolation.Category);
        result.Metadata.Should().BeEquivalentTo(_baseViolation.Metadata);
        result.DetectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void WithMessage_WithNullViolation_ShouldThrowArgumentNullException()
    {
        // Arrange
        RuleViolation? nullViolation = null;
        var newMessage = "New message";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullViolation!.WithMessage(newMessage));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithMessage_WithInvalidNewMessage_ShouldThrowException(string? invalidMessage)
    {
        // Act & Assert
        Assert.ThrowsAny<Exception>(() => _baseViolation.WithMessage(invalidMessage!));
    }

    [Fact]
    public void WithLocation_WithValidLocation_ShouldCreateNewViolationWithUpdatedLocation()
    {
        // Arrange
        var newFilePath = "NewFile.cs";
        const int newLineNumber = 100;
        const int newColumnNumber = 25;

        // Act
        var result = _baseViolation.WithLocation(newFilePath, newLineNumber, newColumnNumber);

        // Assert
        result.Should().NotBeSameAs(_baseViolation);
        result.Id.Should().NotBe(_baseViolation.Id);
        result.FilePath.Should().Be(newFilePath);
        result.LineNumber.Should().Be(newLineNumber);
        result.ColumnNumber.Should().Be(newColumnNumber);
        result.Message.Should().Be(_baseViolation.Message);
        result.RuleId.Should().Be(_baseViolation.RuleId);
        result.RuleName.Should().Be(_baseViolation.RuleName);
        result.Severity.Should().Be(_baseViolation.Severity);
        result.CodeSnippet.Should().Be(_baseViolation.CodeSnippet);
        result.ProjectName.Should().Be(_baseViolation.ProjectName);
        result.Category.Should().Be(_baseViolation.Category);
        result.Metadata.Should().BeEquivalentTo(_baseViolation.Metadata);
        result.DetectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void WithLocation_WithZeroLineNumber_ShouldThrowArgumentException()
    {
        // Arrange
        var newFilePath = "NewFile.cs";
        const int zeroLineNumber = 0;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _baseViolation.WithLocation(newFilePath, zeroLineNumber));
        exception.ParamName.Should().Be("newLineNumber");
    }

    [Fact]
    public void WithLocation_WithNegativeLineNumber_ShouldThrowArgumentException()
    {
        // Arrange
        var newFilePath = "NewFile.cs";
        const int negativeLineNumber = -1;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _baseViolation.WithLocation(newFilePath, negativeLineNumber));
        exception.ParamName.Should().Be("newLineNumber");
    }

    [Fact]
    public void WithLocation_WithNegativeColumnNumber_ShouldThrowArgumentException()
    {
        // Arrange
        var newFilePath = "NewFile.cs";
        const int newLineNumber = 100;
        const int negativeColumnNumber = -1;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _baseViolation.WithLocation(newFilePath, newLineNumber, negativeColumnNumber));
        exception.ParamName.Should().Be("newColumnNumber");
    }

    [Fact]
    public void WithLocation_WithNullViolation_ShouldThrowArgumentNullException()
    {
        // Arrange
        RuleViolation? nullViolation = null;
        var newFilePath = "NewFile.cs";
        const int newLineNumber = 100;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => nullViolation!.WithLocation(newFilePath, newLineNumber));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithLocation_WithInvalidFilePath_ShouldThrowException(string? invalidFilePath)
    {
        // Arrange
        const int newLineNumber = 100;

        // Act & Assert
        Assert.ThrowsAny<Exception>(
            () => _baseViolation.WithLocation(invalidFilePath!, newLineNumber));
    }

    [Fact]
    public void WithSeverity_WithValidSeverity_ShouldCreateNewViolationWithUpdatedSeverity()
    {
        // Arrange
        var newSeverity = SeverityLevel.Error;

        // Act
        var result = _baseViolation.WithSeverity(newSeverity);

        // Assert
        result.Should().NotBeSameAs(_baseViolation);
        result.Id.Should().NotBe(_baseViolation.Id);
        result.Severity.Should().Be(newSeverity);
        result.Message.Should().Be(_baseViolation.Message);
        result.RuleId.Should().Be(_baseViolation.RuleId);
        result.RuleName.Should().Be(_baseViolation.RuleName);
        result.FilePath.Should().Be(_baseViolation.FilePath);
        result.LineNumber.Should().Be(_baseViolation.LineNumber);
        result.ColumnNumber.Should().Be(_baseViolation.ColumnNumber);
        result.CodeSnippet.Should().Be(_baseViolation.CodeSnippet);
        result.ProjectName.Should().Be(_baseViolation.ProjectName);
        result.Category.Should().Be(_baseViolation.Category);
        result.Metadata.Should().BeEquivalentTo(_baseViolation.Metadata);
        result.DetectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void WithSeverity_WithNullViolation_ShouldThrowArgumentNullException()
    {
        // Arrange
        RuleViolation? nullViolation = null;
        var newSeverity = SeverityLevel.Critical;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => RuleViolationExtensions.WithSeverity(nullViolation, newSeverity));
    }

    [Fact]
    public void WithMetadata_WithValidKeyValue_ShouldCreateNewViolationWithAdditionalMetadata()
    {
        // Arrange
        var key = "NewKey";
        var value = "NewValue";

        // Act
        var result = _baseViolation.WithMetadata(key, value);

        // Assert
        result.Should().NotBeSameAs(_baseViolation);
        result.Id.Should().NotBe(_baseViolation.Id);
        result.Metadata.Should().ContainKey(key).WhoseValue.Should().Be(value);
        result.Metadata.Should().ContainKey("Key1").WhoseValue.Should().Be("Value1");
        result.Message.Should().Be(_baseViolation.Message);
        result.RuleId.Should().Be(_baseViolation.RuleId);
        result.RuleName.Should().Be(_baseViolation.RuleName);
        result.Severity.Should().Be(_baseViolation.Severity);
        result.FilePath.Should().Be(_baseViolation.FilePath);
        result.LineNumber.Should().Be(_baseViolation.LineNumber);
        result.ColumnNumber.Should().Be(_baseViolation.ColumnNumber);
        result.CodeSnippet.Should().Be(_baseViolation.CodeSnippet);
        result.ProjectName.Should().Be(_baseViolation.ProjectName);
        result.Category.Should().Be(_baseViolation.Category);
        result.DetectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void WithMetadata_WithExistingKey_ShouldUpdateExistingMetadataValue()
    {
        // Arrange
        var key = "Key1";
        var newValue = "UpdatedValue";

        // Act
        var result = _baseViolation.WithMetadata(key, newValue);

        // Assert
        result.Metadata.Should().ContainKey(key).WhoseValue.Should().Be(newValue);
        result.Metadata.Should().HaveCount(_baseViolation.Metadata.Count);
    }

    [Fact]
    public void WithMetadata_WithNullViolation_ShouldThrowArgumentNullException()
    {
        // Arrange
        RuleViolation? nullViolation = null;
        var key = "Key";
        var value = "Value";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullViolation!.WithMetadata(key, value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithMetadata_WithInvalidKey_ShouldThrowException(string? invalidKey)
    {
        // Arrange
        var value = "Value";

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => _baseViolation.WithMetadata(invalidKey!, value));
    }

    [Fact]
    public void WithDetectedAt_WithValidDateTime_ShouldCreateNewViolationWithUpdatedTimestamp()
    {
        // Arrange
        var newDetectedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var result = _baseViolation.WithDetectedAt(newDetectedAt);

        // Assert
        result.Should().NotBeSameAs(_baseViolation);
        result.Id.Should().NotBe(_baseViolation.Id);
        result.DetectedAt.Should().Be(newDetectedAt);
        result.Message.Should().Be(_baseViolation.Message);
        result.RuleId.Should().Be(_baseViolation.RuleId);
        result.RuleName.Should().Be(_baseViolation.RuleName);
        result.Severity.Should().Be(_baseViolation.Severity);
        result.FilePath.Should().Be(_baseViolation.FilePath);
        result.LineNumber.Should().Be(_baseViolation.LineNumber);
        result.ColumnNumber.Should().Be(_baseViolation.ColumnNumber);
        result.CodeSnippet.Should().Be(_baseViolation.CodeSnippet);
        result.ProjectName.Should().Be(_baseViolation.ProjectName);
        result.Category.Should().Be(_baseViolation.Category);
        result.Metadata.Should().BeEquivalentTo(_baseViolation.Metadata);
    }

    [Fact]
    public void WithDetectedAt_WithNullViolation_ShouldThrowArgumentNullException()
    {
        // Arrange
        RuleViolation? nullViolation = null;
        var newDetectedAt = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullViolation!.WithDetectedAt(newDetectedAt));
    }

    [Fact]
    public void HasCategory_WithMatchingCategory_ShouldReturnTrue()
    {
        // Arrange
        var category = RuleCategory.CodeStructure;

        // Act
        var result = _baseViolation.HasCategory(category);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasCategory_WithNonMatchingCategory_ShouldReturnFalse()
    {
        // Arrange
        var category = RuleCategory.LayerDependency;

        // Act
        var result = _baseViolation.HasCategory(category);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasCategory_WithNullViolation_ShouldThrowArgumentNullException()
    {
        // Arrange
        RuleViolation? nullViolation = null;
        var category = RuleCategory.NamingConvention;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullViolation!.HasCategory(category));
    }

    [Fact]
    public void HasAnyCategory_WithMatchingSingleCategory_ShouldReturnTrue()
    {
        // Arrange
        var categories = new[] { RuleCategory.CodeStructure };

        // Act
        var result = _baseViolation.HasAnyCategory(categories);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasAnyCategory_WithMatchingMultipleCategories_ShouldReturnTrue()
    {
        // Arrange
        var categories = new[] { RuleCategory.LayerDependency, RuleCategory.CodeStructure, RuleCategory.NamingConvention };

        // Act
        var result = _baseViolation.HasAnyCategory(categories);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasAnyCategory_WithNoMatchingCategories_ShouldReturnFalse()
    {
        // Arrange
        var categories = new[] { RuleCategory.LayerDependency, RuleCategory.NamingConvention, RuleCategory.AsyncPattern };

        // Act
        var result = _baseViolation.HasAnyCategory(categories);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasAnyCategory_WithNullViolation_ShouldThrowArgumentNullException()
    {
        // Arrange
        RuleViolation? nullViolation = null;
        var categories = new[] { RuleCategory.CodeStructure };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullViolation!.HasAnyCategory(categories));
    }

    [Fact]
    public void HasAnyCategory_WithNullCategoriesArray_ShouldThrowArgumentNullException()
    {
        // Arrange
        RuleViolation? nullViolation = _baseViolation;
        RuleCategory[]? nullCategories = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullViolation!.HasAnyCategory(nullCategories!));
    }

    [Fact]
    public void GetFormattedCodeSnippet_WithCodeSnippet_ShouldReturnFormattedSnippetWithLineNumbers()
    {
        // Arrange
        var violation = new RuleViolation("TEST002", "TestRule2", "Test message", "TestFile.cs")
        {
            LineNumber = 10,
            CodeSnippet = "public class Test\n{\n    public void Method() { }\n}"
        };

        // Act
        var result = violation.GetFormattedCodeSnippet();

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("  10 | public class Test");
        result.Should().Contain("  11 | {");
        result.Should().Contain("  12 |     public void Method() { }");
        result.Should().Contain("  13 | }");
        result.Should().NotContain("  14 |");
    }

    [Fact]
    public void GetFormattedCodeSnippet_WithNullCodeSnippet_ShouldReturnNull()
    {
        // Arrange
        var violation = new RuleViolation("TEST003", "TestRule3", "Test message", "TestFile.cs")
        {
            CodeSnippet = null
        };

        // Act
        var result = violation.GetFormattedCodeSnippet();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetFormattedCodeSnippet_WithEmptyCodeSnippet_ShouldReturnNull()
    {
        // Arrange
        var violation = new RuleViolation("TEST004", "TestRule4", "Test message", "TestFile.cs")
        {
            CodeSnippet = ""
        };

        // Act
        var result = violation.GetFormattedCodeSnippet();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetFormattedCodeSnippet_WithWhitespaceCodeSnippet_ShouldReturnNull()
    {
        // Arrange
        var violation = new RuleViolation("TEST005", "TestRule5", "Test message", "TestFile.cs")
        {
            CodeSnippet = "   \n  \t  "
        };

        // Act
        var result = violation.GetFormattedCodeSnippet();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetFormattedCodeSnippet_WithNullViolation_ShouldThrowArgumentNullException()
    {
        // Arrange
        RuleViolation? nullViolation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullViolation!.GetFormattedCodeSnippet());
    }

    [Fact]
    public void GetFormattedCodeSnippet_WithSingleLineCodeSnippet_ShouldFormatCorrectly()
    {
        // Arrange
        var violation = new RuleViolation("TEST006", "TestRule6", "Test message", "TestFile.cs")
        {
            LineNumber = 1,
            CodeSnippet = "public class TestClass { }"
        };

        // Act
        var result = violation.GetFormattedCodeSnippet();

        // Assert
        result.Should().Be("   1 | public class TestClass { }");
    }
}