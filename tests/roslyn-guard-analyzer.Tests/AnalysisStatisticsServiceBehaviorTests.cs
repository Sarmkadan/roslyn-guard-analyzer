#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Behavioural unit tests for <see cref="AnalysisStatisticsService"/> covering aggregation, rankings,
/// distributions, risk scoring, health assessment and summary rendering.
/// </summary>
public sealed class AnalysisStatisticsServiceBehaviorTests
{
    [Fact]
    public void CalculateStatistics_EmptyInput_ReturnsZeroedStatistics()
    {
        // Arrange
        var emptyResult = new AnalysisResult();

        // Act
        var fromList = AnalysisStatisticsService.CalculateStatistics(Array.Empty<RuleViolation>());
        var fromResult = AnalysisStatisticsService.CalculateStatistics(emptyResult);

        // Assert
        foreach (var stats in new[] { fromList, fromResult })
        {
            stats.TotalCount.Should().Be(0);
            stats.CriticalCount.Should().Be(0);
            stats.HighCount.Should().Be(0);
            stats.MediumCount.Should().Be(0);
            stats.LowCount.Should().Be(0);
            stats.AffectedFiles.Should().Be(0);
            stats.AffectedRules.Should().Be(0);
            stats.ByRule.Should().BeEmpty();
            stats.ByFile.Should().BeEmpty();
            stats.BySeverity.Should().BeEmpty();
        }
    }

    [Fact]
    public void CalculateStatistics_MixedSeverities_CountsEachBucketAndDistinctKeys()
    {
        // Arrange
        var violations = new List<RuleViolation>
        {
            Violation("R1", "src/A.cs", SeverityLevel.Critical),
            Violation("R1", "src/B.cs", SeverityLevel.Critical),
            Violation("R2", "src/A.cs", SeverityLevel.Error),
            Violation("R2", "src/C.cs", SeverityLevel.Warning),
            Violation("R3", "src/C.cs", SeverityLevel.Warning),
            Violation("R3", "src/A.cs", SeverityLevel.Info)
        };

        // Act
        var stats = AnalysisStatisticsService.CalculateStatistics(violations);

        // Assert
        stats.TotalCount.Should().Be(6);
        stats.CriticalCount.Should().Be(2);
        stats.HighCount.Should().Be(1);
        stats.MediumCount.Should().Be(2);
        stats.LowCount.Should().Be(1);
        stats.AffectedFiles.Should().Be(3);
        stats.AffectedRules.Should().Be(3);
        stats.ByRule.Should().BeEquivalentTo(new Dictionary<string, int> { ["R1"] = 2, ["R2"] = 2, ["R3"] = 2 });
        stats.ByFile.Should().BeEquivalentTo(new Dictionary<string, int> { ["src/A.cs"] = 3, ["src/B.cs"] = 1, ["src/C.cs"] = 2 });
        stats.BySeverity.Should().BeEquivalentTo(new Dictionary<SeverityLevel, int> { [SeverityLevel.Critical] = 2, [SeverityLevel.Error] = 1, [SeverityLevel.Warning] = 2, [SeverityLevel.Info] = 1 });
    }

    [Fact]
    public void GetTopRulesByViolations_Count_OrdersDescendingAndLimits()
    {
        // Arrange
        var violations = Repeat("R1", 3).Concat(Repeat("R3", 2)).Concat(Repeat("R2", 1)).ToList();

        // Act
        var top2 = AnalysisStatisticsService.GetTopRulesByViolations(violations, 2);
        var none = AnalysisStatisticsService.GetTopRulesByViolations(violations, 0);

        // Assert
        top2.Should().Equal(("R1", 3), ("R3", 2));
        none.Should().BeEmpty();
    }

    [Fact]
    public void GetTopFilesByViolations_GroupsByFileNameOnly()
    {
        // Arrange
        var violations = new List<RuleViolation>
        {
            Violation("R", "src/a/Program.cs", SeverityLevel.Warning),
            Violation("R", "src/a/Program.cs", SeverityLevel.Warning),
            Violation("R", "src/b/Program.cs", SeverityLevel.Warning),
            Violation("R", "src/Other.cs", SeverityLevel.Warning)
        };

        // Act
        var top = AnalysisStatisticsService.GetTopFilesByViolations(violations);

        // Assert
        top.Should().Equal(("Program.cs", 3), ("Other.cs", 1));
    }

    [Fact]
    public void GetSeverityDistribution_EmptyAndMixedInput_ReturnsEmptyOrPercentages()
    {
        // Arrange
        var mixed = new List<RuleViolation>
        {
            Violation("R1", "src/A.cs", SeverityLevel.Critical),
            Violation("R1", "src/A.cs", SeverityLevel.Critical),
            Violation("R2", "src/A.cs", SeverityLevel.Info)
        };

        // Act
        var empty = AnalysisStatisticsService.GetSeverityDistribution(Array.Empty<RuleViolation>());
        var distribution = AnalysisStatisticsService.GetSeverityDistribution(mixed);

        // Assert
        empty.Should().BeEmpty();
        distribution["Critical"].Should().BeApproximately(66.6667, 0.001);
        distribution["Low"].Should().BeApproximately(33.3333, 0.001);
        distribution["High"].Should().Be(0);
        distribution["Medium"].Should().Be(0);
    }

    [Fact]
    public void CalculateRiskScore_Boundaries_IntegerDivisionAndCappedAt100()
    {
        // Arrange
        var empty = new AnalysisStatisticsService.ViolationStatistics();
        var nineLow = new AnalysisStatisticsService.ViolationStatistics { LowCount = 9 };
        var tenLow = new AnalysisStatisticsService.ViolationStatistics { LowCount = 10 };
        var oneCritical = new AnalysisStatisticsService.ViolationStatistics { CriticalCount = 1 };
        var hugeCritical = new AnalysisStatisticsService.ViolationStatistics { CriticalCount = 1000 };

        // Act & Assert
        AnalysisStatisticsService.CalculateRiskScore(empty).Should().Be(0);
        AnalysisStatisticsService.CalculateRiskScore(nineLow).Should().Be(0);
        AnalysisStatisticsService.CalculateRiskScore(tenLow).Should().Be(1);
        AnalysisStatisticsService.CalculateRiskScore(oneCritical).Should().Be(1);
        AnalysisStatisticsService.CalculateRiskScore(hugeCritical).Should().Be(100);
    }

    [Fact]
    public void GetHealthAssessment_Thresholds_RespectPrecedenceAndBoundaries()
    {
        // Arrange & Act & Assert
        AnalysisStatisticsService.GetHealthAssessment(new()).Should().Contain("Excellent");
        AnalysisStatisticsService.GetHealthAssessment(new() { LowCount = 3, TotalCount = 3 }).Should().Contain("Acceptable");
        AnalysisStatisticsService.GetHealthAssessment(new() { MediumCount = 10, TotalCount = 10 }).Should().Contain("Acceptable");
        AnalysisStatisticsService.GetHealthAssessment(new() { MediumCount = 11, TotalCount = 11 }).Should().Contain("Good");
        AnalysisStatisticsService.GetHealthAssessment(new() { HighCount = 1, TotalCount = 1 }).Should().Contain("Fair");
        AnalysisStatisticsService.GetHealthAssessment(new() { HighCount = 5, TotalCount = 5 }).Should().Contain("Fair");
        AnalysisStatisticsService.GetHealthAssessment(new() { HighCount = 6, TotalCount = 6 }).Should().Contain("Poor");
        AnalysisStatisticsService.GetHealthAssessment(new() { CriticalCount = 1, HighCount = 10, TotalCount = 11 }).Should().Contain("Critical");
    }

    [Fact]
    public void GenerateSummaryReport_EightViolations_ShowsPercentagesAndCapsTopRulesAtFive()
    {
        // Arrange
        var violations = Repeat("R1", 2, SeverityLevel.Critical)
            .Concat(Enumerable.Range(2, 6).Select(i => Violation($"R{i}", "src/A.cs", SeverityLevel.Info)))
            .ToList();
        var stats = AnalysisStatisticsService.CalculateStatistics(violations);

        // Act
        var report = AnalysisStatisticsService.GenerateSummaryReport(stats);

        // Assert
        report.Should().Contain("Total Violations: 8");
        report.Should().Contain("Critical: 2 (25%)");
        report.Should().Contain("Affected Rules: 7");
        report.Should().Contain("  - R1: 2 violations");
        report.Split('\n').Count(line => line.StartsWith("  - ", StringComparison.Ordinal)).Should().Be(5);
    }

    private static RuleViolation Violation(string rule, string file, SeverityLevel severity) => new()
    {
        RuleId = rule,
        RuleName = rule,
        FilePath = file,
        Severity = severity
    };

    private static IEnumerable<RuleViolation> Repeat(string rule, int count, SeverityLevel severity = SeverityLevel.Warning) =>
        Enumerable.Range(0, count).Select(_ => Violation(rule, "src/F.cs", severity));
}
