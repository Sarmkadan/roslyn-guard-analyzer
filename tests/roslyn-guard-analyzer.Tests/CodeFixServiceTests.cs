#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RoslynGuardAnalyzer.CodeFixes;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Provides unit tests for the <see cref="CodeFixService"/> class.
/// </summary>
public sealed class CodeFixServiceTests
{
    private readonly ILogger<CodeFixService> _logger;
    private readonly CodeFixService _service;

    public CodeFixServiceTests()
    {
        _logger = Substitute.For<ILogger<CodeFixService>>();
        _service = new CodeFixService(_logger);
    }

    /// <summary>
    /// Tests that <see cref="CodeFixService.GetFixesAsync"/> returns expected fixes for known rule violations.
    /// </summary>
    [Fact]
    public async Task GetFixesAsync_WithKnownViolations_ReturnsExpectedFixes()
    {
        // Arrange
        var violations = new[]
        {
            new RuleViolation("RG-N001", "Interface naming", "Interface should start with 'I'", "Test.cs")
            {
                LineNumber = 10,
                CodeSnippet = "interface MyInterface"
            },
            new RuleViolation("RG-N002", "Async method naming", "Async method should end with Async", "Test.cs")
            {
                LineNumber = 15,
                CodeSnippet = "public async Task MyMethod()"
            }
        };

        // Act
        var fixes = await _service.GetFixesAsync(violations);

        // Assert
        fixes.Should().HaveCount(2);
        fixes[0].RuleId.Should().Be("RG-N001");
        fixes[0].Title.Should().Be("Prefix interface 'MyInterface' with 'I'");
        fixes[0].OriginalCode.Should().Be("interface MyInterface");
        fixes[0].ReplacementCode.Should().Be("interface IMyInterface");

        fixes[1].RuleId.Should().Be("RG-N002");
        fixes[1].Title.Should().Be("Add 'Async' suffix to method 'MyMethod'");
        fixes[1].OriginalCode.Should().Be("MyMethod(");
        fixes[1].ReplacementCode.Should().Be("MyMethodAsync(");
    }

    /// <summary>
    /// Tests that <see cref="CodeFixService.GetFixesAsync"/> returns no fixes for unknown rule violations.
    /// </summary>
    [Fact]
    public async Task GetFixesAsync_WithUnknownRuleId_ReturnsNoFixes()
    {
        // Arrange
        var violations = new[]
        {
            new RuleViolation("RG-X999", "Unknown rule", "Some violation", "Test.cs")
            {
                LineNumber = 5,
                CodeSnippet = "some code"
            }
        };

        // Act
        var fixes = await _service.GetFixesAsync(violations);

        // Assert
        fixes.Should().BeEmpty();
        _logger.Received(1).LogDebug(
            Arg.Is<string>("No fix provider registered for rule {RuleId}."),
            Arg.Any<object[]>());
    }

    /// <summary>
    /// Tests that <see cref="CodeFixService.ApplyFixesAsync"/> correctly applies fixes to source code.
    /// </summary>
    [Fact]
    public async Task ApplyFixesAsync_WithValidFixes_AppliesChangesToSource()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, @"interface MyInterface { }");

            var fix = new CodeFix
            {
                Id = System.Guid.NewGuid().ToString(),
                ViolationId = System.Guid.NewGuid().ToString(),
                RuleId = "RG-N001",
                Title = "Prefix interface 'MyInterface' with 'I'",
                Description = "Rename 'MyInterface' to 'IMyInterface' to satisfy the interface naming convention (RG-N001).",
                FilePath = tempFile,
                StartLine = 1,
                EndLine = 1,
                OriginalCode = "interface MyInterface",
                ReplacementCode = "interface IMyInterface",
                Severity = SeverityLevel.Warning
            };

            // Act
            var result = await _service.ApplyFixesAsync(new[] { fix });

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.AppliedFixes.Should().HaveCount(1);
            result.FailedFixes.Should().BeEmpty();

            var content = await File.ReadAllTextAsync(tempFile);
            content.Should().Be(@"interface IMyInterface { }");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Tests that <see cref="CodeFixService.ApplyFixesAsync"/> handles overlapping fixes correctly by processing in reverse order.
    /// </summary>
    [Fact]
    public async Task ApplyFixesAsync_WithOverlappingFixes_HandlesOverlapsCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, @"interface MyInterface { }");

            var fix1 = new CodeFix
            {
                Id = System.Guid.NewGuid().ToString(),
                ViolationId = System.Guid.NewGuid().ToString(),
                RuleId = "RG-N001",
                Title = "Prefix interface 'MyInterface' with 'I'",
                Description = "Rename 'MyInterface' to 'IMyInterface' to satisfy the interface naming convention (RG-N001).",
                FilePath = tempFile,
                StartLine = 1,
                EndLine = 1,
                OriginalCode = "interface MyInterface",
                ReplacementCode = "interface IMyInterface",
                Severity = SeverityLevel.Warning
            };

            var fix2 = new CodeFix
            {
                Id = System.Guid.NewGuid().ToString(),
                ViolationId = System.Guid.NewGuid().ToString(),
                RuleId = "RG-N001",
                Title = "Prefix interface 'IMyInterface' with 'I'",
                Description = "Rename 'IMyInterface' to 'IIMyInterface' to satisfy the interface naming convention (RG-N001).",
                FilePath = tempFile,
                StartLine = 1,
                EndLine = 1,
                OriginalCode = "interface IMyInterface",
                ReplacementCode = "interface IIMyInterface",
                Severity = SeverityLevel.Warning
            };

            // Act
            var result = await _service.ApplyFixesAsync(new[] { fix1, fix2 });

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.AppliedFixes.Should().HaveCount(2);
            result.FailedFixes.Should().BeEmpty();

            var content = await File.ReadAllTextAsync(tempFile);
            // Should apply in reverse order: first fix2 (IMyInterface -> IIMyInterface), then fix1 (interface MyInterface -> interface IMyInterface)
            // But since fix2 runs first on original text, it won't match, so only fix1 should apply
            content.Should().Be(@"interface IMyInterface { }");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Tests that <see cref="CodeFixService.ApplyFixesAsync"/> performs no-op when no fixes are provided.
    /// </summary>
    [Fact]
    public async Task ApplyFixesAsync_WithNoFixes_ReturnsSuccessWithoutChanges()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, @"interface MyInterface { }");

            // Act
            var result = await _service.ApplyFixesAsync(Array.Empty<CodeFix>());

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.AppliedFixes.Should().BeEmpty();
            result.FailedFixes.Should().BeEmpty();

            var content = await File.ReadAllTextAsync(tempFile);
            content.Should().Be(@"interface MyInterface { }"); // Unchanged
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Tests that <see cref="CodeFixService.ApplyFixesAsync"/> handles missing files gracefully.
    /// </summary>
    [Fact]
    public async Task ApplyFixesAsync_WithNonExistentFile_HandlesGracefully()
    {
        // Arrange
        var fakeFilePath = Path.GetTempFileName();
        File.Delete(fakeFilePath); // Ensure file doesn't exist

        var fix = new CodeFix
        {
            Id = System.Guid.NewGuid().ToString(),
            ViolationId = System.Guid.NewGuid().ToString(),
            RuleId = "RG-N001",
            Title = "Prefix interface 'MyInterface' with 'I'",
            Description = "Rename 'MyInterface' to 'IMyInterface' to satisfy the interface naming convention (RG-N001).",
            FilePath = fakeFilePath,
            StartLine = 1,
            EndLine = 1,
            OriginalCode = "interface MyInterface",
            ReplacementCode = "interface IMyInterface",
            Severity = SeverityLevel.Warning
        };

        // Act
        var result = await _service.ApplyFixesAsync(new[] { fix });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.AppliedFixes.Should().BeEmpty();
        result.FailedFixes.Should().HaveCount(1);
        result.Messages.Should().ContainSingle(m => m.Contains("File not found"));
    }

    /// <summary>
    /// Tests that <see cref="CodeFixService.GetFixesAsync"/> handles null violations argument correctly.
    /// </summary>
    [Fact]
    public async Task GetFixesAsync_WithNullViolations_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _service.GetFixesAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("violations");
    }

    /// <summary>
    /// Tests that <see cref="CodeFixService.ApplyFixesAsync"/> handles null fixes argument correctly.
    /// </summary>
    [Fact]
    public async Task ApplyFixesAsync_WithNullFixes_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _service.ApplyFixesAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("fixes");
    }

    /// <summary>
    /// Tests that <see cref="CodeFixService.ApplyFixesAsync"/> respects dryRun mode and doesn't write changes.
    /// </summary>
    [Fact]
    public async Task ApplyFixesAsync_WithDryRunTrue_DoesNotWriteChangesToDisk()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, @"interface MyInterface { }");

            var fix = new CodeFix
            {
                Id = System.Guid.NewGuid().ToString(),
                ViolationId = System.Guid.NewGuid().ToString(),
                RuleId = "RG-N001",
                Title = "Prefix interface 'MyInterface' with 'I'",
                Description = "Rename 'MyInterface' to 'IMyInterface' to satisfy the interface naming convention (RG-N001).",
                FilePath = tempFile,
                StartLine = 1,
                EndLine = 1,
                OriginalCode = "interface MyInterface",
                ReplacementCode = "interface IMyInterface",
                Severity = SeverityLevel.Warning
            };

            // Act
            var result = await _service.ApplyFixesAsync(new[] { fix }, dryRun: true);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.AppliedFixes.Should().HaveCount(1);
            result.FailedFixes.Should().BeEmpty();
            result.Messages.Should().ContainSingle(m => m.Contains("Dry-run") && m.Contains("NOT written"));

            // File should remain unchanged
            var content = await File.ReadAllTextAsync(tempFile);
            content.Should().Be(@"interface MyInterface { }");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}