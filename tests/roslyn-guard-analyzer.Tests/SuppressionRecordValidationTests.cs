#nullable enable

using System;
using System.Collections.Generic;
using FluentAssertions;
using RoslynGuardAnalyzer.Suppressions;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class SuppressionRecordValidationTests
{
    private static SuppressionRecord CreateValidRecord()
    {
        var now = DateTime.UtcNow;
        return new SuppressionRecord
        {
            Id = "SR001",
            RuleId = "RULE001",
            Justification = "Justified because of ...",
            Author = "Jane Doe",
            TargetFile = "/path/to/file.cs",
            TargetElement = "MyClass.MyMethod",
            CreatedAt = now,
            ExpiresAt = now.AddDays(1)
        };
    }

    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        var result = record.Validate();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        var isValid = record.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        Action act = () => record.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_MissingRequiredFields_ReturnsErrors()
    {
        // Arrange
        var record = new SuppressionRecord
        {
            // All required string properties are left null/empty
            Id = "",
            RuleId = "   ",
            Justification = null,
            Author = "",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var errors = record.Validate();

        // Assert
        errors.Should().Contain("Id must not be null, empty, or whitespace.");
        errors.Should().Contain("RuleId must not be null, empty, or whitespace.");
        errors.Should().Contain("Justification must not be null, empty, or whitespace.");
        errors.Should().Contain("Author must not be null, empty, or whitespace.");
        errors.Should().HaveCountGreaterOrEqualTo(4);
    }

    [Fact]
    public void Validate_InvalidCreatedAt_ReturnsError()
    {
        // Arrange
        var record = CreateValidRecord();
        record.CreatedAt = default; // invalid default value

        // Act
        var errors = record.Validate();

        // Assert
        errors.Should().ContainSingle(e => e.Contains("CreatedAt must be a valid date and time."));
    }

    [Fact]
    public void Validate_ExpiresAtBeforeCreatedAt_ReturnsError()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var record = CreateValidRecord();
        record.CreatedAt = now;
        record.ExpiresAt = now.AddHours(-1); // earlier than CreatedAt

        // Act
        var errors = record.Validate();

        // Assert
        errors.Should().ContainSingle(e => e.Contains("ExpiresAt must be later than CreatedAt."));
    }

    [Fact]
    public void IsValid_WithErrors_ReturnsFalse()
    {
        // Arrange
        var record = new SuppressionRecord
        {
            Id = null,
            RuleId = null,
            Justification = null,
            Author = null,
            CreatedAt = default
        };

        // Act
        var isValid = record.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_WithErrors_ThrowsArgumentException()
    {
        // Arrange
        var record = new SuppressionRecord
        {
            Id = "",
            RuleId = "",
            Justification = "",
            Author = "",
            CreatedAt = default
        };

        // Act
        Action act = () => record.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*SuppressionRecord is invalid*")
            .Where(ex => ex.Message.Contains("Id must not be null, empty, or whitespace."));
    }

    [Fact]
    public void Validate_NullRecord_ThrowsArgumentNullException()
    {
        // Arrange
        SuppressionRecord? record = null;

        // Act
        Action act = () => record!.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
