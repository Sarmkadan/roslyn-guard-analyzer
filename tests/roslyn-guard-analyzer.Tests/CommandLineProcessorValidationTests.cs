#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for CommandLineProcessorValidation
// =====================================================================

using System;
using FluentAssertions;
using RoslynGuardAnalyzer.Cli;
using Xunit;

namespace RoslynGuardAnalyzer.Tests.Cli;

public class CommandLineProcessorValidationTests
{
    [Fact]
    public void Validate_NullProcessor_ThrowsArgumentNullException()
    {
        // Arrange
        CommandLineProcessor? processor = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => processor!.Validate());
    }

    [Fact]
    public void Validate_ProcessorNotProcessed_ReturnsError()
    {
        // Arrange
        var processor = new CommandLineProcessor(["--project=test.csproj"]);
        // Don't call Process()

        // Act
        var errors = processor.Validate();

        // Assert
        errors.Should().HaveCount(1);
        errors[0].Should().Contain("Either --project or --file must be specified");
    }

    [Fact]
    public void Validate_EmptyArgsProcessor_ReturnsError()
    {
        // Arrange
        var processor = new CommandLineProcessor([]);
        processor.Process();

        // Act
        var errors = processor.Validate();

        // Assert
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void IsValid_NullProcessor_ThrowsArgumentNullException()
    {
        // Arrange
        CommandLineProcessor? processor = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => processor!.IsValid());
    }

    [Fact]
    public void IsValid_InvalidProcessor_ReturnsFalse()
    {
        // Arrange
        var processor = new CommandLineProcessor(["--project=/nonexistent/path.csproj"]);
        processor.Process();

        // Act
        var isValid = processor.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_NullProcessor_ThrowsArgumentNullException()
    {
        // Arrange
        CommandLineProcessor? processor = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => processor!.EnsureValid());
    }

    [Fact]
    public void EnsureValid_InvalidProcessor_ThrowsArgumentException()
    {
        // Arrange
        var processor = new CommandLineProcessor(["--project=/nonexistent/path.csproj"]);
        processor.Process();

        // Act
        Action act = () => processor.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Command-line processor is not valid*");
    }

    [Fact]
    public void Validate_AnalysisModeWithInvalidPaths_ReturnsPathErrors()
    {
        // Arrange
        var processor = new CommandLineProcessor([
            "--project=/nonexistent/project.csproj",
            "--config=/nonexistent/config.json"
        ]);
        processor.Process();

        // Act
        var errors = processor.Validate();

        // Assert
        errors.Should().ContainMatch("*Path not found*");
        errors.Should().ContainMatch("*Config file not found*");
    }

    [Fact]
    public void Validate_FileModeWithInvalidFile_ReturnsError()
    {
        // Arrange
        var processor = new CommandLineProcessor(["--file=/nonexistent/file.cs"]);
        processor.Process();

        // Act
        var errors = processor.Validate();

        // Assert
        errors.Should().ContainMatch("*Path not found*");
    }

    [Fact]
    public void EnsureValid_DoesNotThrowWhenHelpFlag()
    {
        // Arrange - use help flag which is always valid
        var processor = new CommandLineProcessor(["--help"]);
        processor.Process();

        // Act
        Action act = () => processor.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }
}
