#nullable enable
// =============================================================================
// Author: Automated Generation
// =============================================================================

using System;
using System.IO;
using FluentAssertions;
using RoslynGuardAnalyzer.Cli;
using Xunit;

namespace RoslynGuardAnalyzer.Tests.Validation;

public class CommandLineProcessorValidationTests
{
    // ------------------------------------------------------------------------
    // ArgumentNullException cases
    // ------------------------------------------------------------------------
    [Fact]
    public void Validate_NullProcessor_ThrowsArgumentNullException()
    {
        // Arrange
        CommandLineProcessor? processor = null;

        // Act
        Action act = () => processor!.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsValid_NullProcessor_ThrowsArgumentNullException()
    {
        // Arrange
        CommandLineProcessor? processor = null;

        // Act
        Action act = () => processor!.IsValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_NullProcessor_ThrowsArgumentNullException()
    {
        // Arrange
        CommandLineProcessor? processor = null;

        // Act
        Action act = () => processor!.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ------------------------------------------------------------------------
    // Happy path – valid arguments and existing files
    // ------------------------------------------------------------------------
    [Fact]
    public void Validate_ReturnsEmpty_WhenAllPathsExist()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var projectPath = Path.Combine(tempDir, "test.csproj");
        File.WriteAllText(projectPath, "<Project></Project>");

        var configPath = Path.Combine(tempDir, "config.json");
        File.WriteAllText(configPath, "{}");

        var processor = new CommandLineProcessor(
            [$"--project={projectPath}", $"--config={configPath}"]);
        processor.Process();

        // Act
        var errors = processor.Validate();

        // Assert
        errors.Should().BeEmpty();

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    // ------------------------------------------------------------------------
    // Error paths – missing files / invalid arguments
    // ------------------------------------------------------------------------
    [Fact]
    public void Validate_ReturnsError_WhenProjectPathDoesNotExist()
    {
        // Arrange
        var processor = new CommandLineProcessor(
            ["--project=/nonexistent/project.csproj"]);
        processor.Process();

        // Act
        var errors = processor.Validate();

        // Assert
        errors.Should().ContainMatch("*Path not found*");
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenProcessorIsValid()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var projectPath = Path.Combine(tempDir, "test.csproj");
        File.WriteAllText(projectPath, "<Project></Project>");

        var processor = new CommandLineProcessor([$"--project={projectPath}"]);
        processor.Process();

        // Act
        var isValid = processor.IsValid();

        // Assert
        isValid.Should().BeTrue();

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenProcessorHasErrors()
    {
        // Arrange
        var processor = new CommandLineProcessor(
            ["--project=/nonexistent/project.csproj"]);
        processor.Process();

        // Act
        var isValid = processor.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_WhenProcessorIsInvalid()
    {
        // Arrange
        var processor = new CommandLineProcessor(
            ["--project=/nonexistent/project.csproj"]);
        processor.Process();

        // Act
        Action act = () => processor.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Command-line processor is not valid*");
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_WhenHelpFlagIsProvided()
    {
        // Arrange
        var processor = new CommandLineProcessor(["--help"]);
        processor.Process();

        // Act
        Action act = () => processor.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }
}
