#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Suppressions;
using Xunit;

/// <summary>
/// Tests for the <see cref="SuppressionManager"/> class.
/// </summary>
public sealed class SuppressionManagerTests
{
    /// <summary>
    /// Tests that a new suppression record is stored and matches a violation.
    /// </summary>
    [Fact]
    public void AddSuppression_NewRecord_IsStoredAndMatchesViolation()
    {
        // Arrange
        var manager = new SuppressionManager(Substitute.For<ILogger<SuppressionManager>>());
        var violation = CreateViolation();
        var record = new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            TargetElement = "LegacyRepository",
            Justification = "Known exception",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        manager.AddSuppression(record);

        // Assert
        manager.GetSuppressions().Should().ContainSingle();
        manager.IsSuppressed(violation).Should().BeTrue();
    }

    /// <summary>
    /// Tests that a violation is returned when filtering suppressed violations with an expired suppression.
    /// </summary>
    [Fact]
    public void FilterSuppressed_WithExpiredSuppression_ReturnsViolation()
    {
        // Arrange
        var manager = new SuppressionManager(Substitute.For<ILogger<SuppressionManager>>());
        var violation = CreateViolation();
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            TargetElement = "LegacyRepository",
            Justification = "Expired",
            Author = "tester",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsActive = true
        });

        // Act
        var remaining = manager.FilterSuppressed(new[] { violation });

        // Assert
        remaining.Should().ContainSingle();
    }

    /// <summary>
    /// Tests that a violation is hidden when filtering suppressed violations with an active suppression.
    /// </summary>
    [Fact]
    public void FilterSuppressed_ActiveSuppression_HidesViolation()
    {
        // Arrange
        var manager = new SuppressionManager(Substitute.For<ILogger<SuppressionManager>>());
        var violation = CreateViolation();
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            TargetElement = "LegacyRepository",
            Justification = "Approved suppression",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            IsActive = true
        });

        // Act
        var remaining = manager.FilterSuppressed(new[] { violation });

        // Assert
        remaining.Should().BeEmpty();
    }

    /// <summary>
    /// Creates a new <see cref="RuleViolation"/> instance for testing purposes.
    /// </summary>
    /// <returns>A new <see cref="RuleViolation"/> instance.</returns>
    private static RuleViolation CreateViolation()
    {
        var violation = new RuleViolation("LYR001", "Layer rule", "Violation", "/src/LegacyRepository.cs")
        {
            Severity = SeverityLevel.Error,
            LineNumber = 10,
            Category = RuleCategory.LayerDependency
        };
        violation.AddMetadata("ElementName", "LegacyRepository");
        return violation;
    }
}
