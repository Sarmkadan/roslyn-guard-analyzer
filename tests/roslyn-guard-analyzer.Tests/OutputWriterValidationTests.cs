#nullable enable

using System;
using System.Collections.Generic;

using FluentAssertions;
using RoslynGuardAnalyzer.Formatters;
using RoslynGuardAnalyzer.Services;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="OutputWriterValidation"/> covering validation results, null handling and read-only output.
/// </summary>
public sealed class OutputWriterValidationTests
{
    [Fact]
    public void Validate_ValidWriterWithDefaultRegistry_ReturnsEmptyList()
    {
        // Arrange
        var writer = new OutputWriter();

        // Act
        var problems = OutputWriterValidation.Validate(writer);

        // Assert
        problems.Should().NotBeNull();
        problems.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidWriterWithExplicitRegistry_ReturnsEmptyList()
    {
        // Arrange
        var writer = new OutputWriter(FormatterRegistry.CreateWithDefaults());

        // Act
        var problems = OutputWriterValidation.Validate(writer);

        // Assert
        problems.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ReturnedList_IsReadOnly()
    {
        // Arrange
        var writer = new OutputWriter();
        var problems = OutputWriterValidation.Validate(writer);

        // Act
        Action add = () => ((IList<string>)problems).Add("problem");

        // Assert
        add.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Validate_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        OutputWriter? writer = null;

        // Act
        Action act = () => OutputWriterValidation.Validate(writer);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    [Fact]
    public void IsValid_ValidWriter_ReturnsTrue()
    {
        // Arrange
        var writer = new OutputWriter();

        // Act
        var result = OutputWriterValidation.IsValid(writer);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_NullWriter_ReturnsFalseWithoutThrowing()
    {
        // Arrange
        OutputWriter? writer = null;

        // Act
        var result = OutputWriterValidation.IsValid(writer);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_ValidWriter_DoesNotThrow()
    {
        // Arrange
        var writer = new OutputWriter();

        // Act
        Action act = () => OutputWriterValidation.EnsureValid(writer);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        OutputWriter? writer = null;

        // Act
        Action act = () => OutputWriterValidation.EnsureValid(writer);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }
}
