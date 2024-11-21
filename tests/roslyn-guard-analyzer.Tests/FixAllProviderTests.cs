#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
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

public sealed class FixAllProviderTests
{
    [Fact]
    public async Task PreviewAllAsync_WithViolations_ReturnsExpectedFixes()
    {
        // Arrange
        var codeFixService = Substitute.For<ICodeFixService>();
        var provider = new FixAllProvider(codeFixService, Substitute.For<ILogger<FixAllProvider>>());
        var violations = new[] { CreateViolation("RG-N001", SeverityLevel.Warning) };
        var expectedFixes = new List<CodeFix> { CreateFix(violations[0]) }.AsReadOnly();

        codeFixService.GetFixesAsync(Arg.Any<IEnumerable<RuleViolation>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((IReadOnlyList<CodeFix>)expectedFixes));

        // Act
        var fixes = await provider.PreviewAllAsync(violations, new FixAllOptions());

        // Assert
        fixes.Should().HaveCount(1);
        fixes[0].RuleId.Should().Be("RG-N001");
    }

    [Fact]
    public async Task ApplyAllAsync_DryRun_DoesNotApplyChanges()
    {
        // Arrange
        var codeFixService = Substitute.For<ICodeFixService>();
        var provider = new FixAllProvider(codeFixService, Substitute.For<ILogger<FixAllProvider>>());
        var violations = new[] { CreateViolation("RG-N001", SeverityLevel.Warning) };
        var fixes = new List<CodeFix> { CreateFix(violations[0]) }.AsReadOnly();

        codeFixService.GetFixesAsync(Arg.Any<IEnumerable<RuleViolation>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((IReadOnlyList<CodeFix>)fixes));
        codeFixService.ApplyFixesAsync(Arg.Any<IEnumerable<CodeFix>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CodeFixResult { IsSuccess = true }));

        // Act
        var result = await provider.ApplyAllAsync(violations, new FixAllOptions { DryRun = true });

        // Assert
        result.IsSuccess.Should().BeTrue();
        await codeFixService.Received(1).ApplyFixesAsync(Arg.Any<IEnumerable<CodeFix>>(), true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyAllAsync_WithMinimumSeverity_FiltersLowSeverityViolations()
    {
        // Arrange
        var codeFixService = Substitute.For<ICodeFixService>();
        var provider = new FixAllProvider(codeFixService, Substitute.For<ILogger<FixAllProvider>>());
        var low = CreateViolation("RG-N001", SeverityLevel.Info);
        var high = CreateViolation("RG-A001", SeverityLevel.Error);
        codeFixService.GetFixesAsync(Arg.Any<IEnumerable<RuleViolation>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var input = new List<RuleViolation>(callInfo.Arg<IEnumerable<RuleViolation>>());
                var fixes = new List<CodeFix>();
                foreach (var violation in input)
                    fixes.Add(CreateFix(violation));
                return Task.FromResult((IReadOnlyList<CodeFix>)fixes.AsReadOnly());
            });
        codeFixService.ApplyFixesAsync(Arg.Any<IEnumerable<CodeFix>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CodeFixResult { IsSuccess = true }));

        // Act
        var result = await provider.ApplyAllAsync(new[] { low, high }, new FixAllOptions { MinimumSeverity = SeverityLevel.Warning });

        // Assert
        result.FixableViolations.Should().Be(1);
        await codeFixService.Received(1).GetFixesAsync(
            Arg.Is<IEnumerable<RuleViolation>>(items => new List<RuleViolation>(items).TrueForAll(v => v.Severity >= SeverityLevel.Warning)),
            Arg.Any<CancellationToken>());
    }

    private static RuleViolation CreateViolation(string ruleId, SeverityLevel severity)
    {
        return new RuleViolation(ruleId, "Rule", "Message", "/src/File.cs")
        {
            Severity = severity,
            LineNumber = 12,
            Category = RuleCategory.CodeStructure
        };
    }

    private static CodeFix CreateFix(RuleViolation violation)
    {
        return new CodeFix
        {
            ViolationId = violation.Id,
            RuleId = violation.RuleId,
            FilePath = violation.FilePath,
            OriginalCode = "Old",
            ReplacementCode = "New",
            StartLine = violation.LineNumber,
            Title = "Fix",
            Severity = violation.Severity
        };
    }
}
