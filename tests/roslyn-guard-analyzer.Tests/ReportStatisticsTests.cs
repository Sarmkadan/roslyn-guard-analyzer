// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Tests for ReportStatistics edge cases and aggregation behavior.
/// </summary>
public sealed class ReportStatisticsTests
{
    [Fact]
    public void EmptyReport_ShouldNotThrowOrProduceNaN()
    {
        // Arrange
        var report = new ViolationReport("Empty Test Report", "TestProject");

        // Act - Statistics should be initialized but empty
        var statistics = report.Statistics;

        // Assert
        statistics.TotalViolations.Should().Be(0);
        statistics.CriticalCount.Should().Be(0);
        statistics.ErrorCount.Should().Be(0);
        statistics.WarningCount.Should().Be(0);
        statistics.InfoCount.Should().Be(0);
        statistics.AffectedFileCount.Should().Be(0);
        statistics.AffectedNamespaceCount.Should().Be(0);
        statistics.RuleCount.Should().Be(0);
        statistics.AverageViolationsPerFile.Should().Be(0);
    }

    [Fact]
    public void EmptyReport_StatisticsShouldBeConsistent()
    {
        // Arrange
        var report = new ViolationReport("Empty Report", "TestProject");

        // Act
        var statistics = report.Statistics;

        // Assert - All counts should be zero and no NaN values
        statistics.TotalViolations.Should().Be(0, "Total violations should be 0 for empty report");
        statistics.CriticalCount.Should().Be(0);
        statistics.ErrorCount.Should().Be(0);
        statistics.WarningCount.Should().Be(0);
        statistics.InfoCount.Should().Be(0);
        statistics.AffectedFileCount.Should().Be(0);
        statistics.AffectedNamespaceCount.Should().Be(0);
        statistics.RuleCount.Should().Be(0);
        statistics.AverageViolationsPerFile.Should().Be(0);

        // Verify IsPassing returns true for empty report
        statistics.IsPassing().Should().BeTrue("Empty report should be passing");
    }

    [Fact]
    public void SingleSeverityConcentratedReport_ShouldCalculateCorrectly()
    {
        // Arrange
        var report = new ViolationReport("Single Severity Report", "TestProject");
        var group = new ViolationGroup("Critical Violations", "All violations are critical");

        // Add 10 critical violations
        for (int i = 0; i < 10; i++)
        {
            group.AddViolation(new RuleViolation(
                "rule-critical-001",
                "Critical Rule",
                $"Critical violation {i}",
                $"file{i}.cs")
            {
                Severity = SeverityLevel.Critical
            });
        }

        report.AddViolationGroup(group);

        // Act
        var statistics = report.Statistics;

        // Assert
        statistics.TotalViolations.Should().Be(10);
        statistics.CriticalCount.Should().Be(10);
        statistics.ErrorCount.Should().Be(0);
        statistics.WarningCount.Should().Be(0);
        statistics.InfoCount.Should().Be(0);
        statistics.AffectedFileCount.Should().Be(10);
        statistics.IsPassing().Should().BeFalse("Report with critical violations should not be passing");
    }

    [Fact]
    public void MixedSeverityReport_ShouldCalculateTotalsCorrectly()
    {
        // Arrange
        var report = new ViolationReport("Mixed Severity Report", "TestProject");
        var criticalGroup = new ViolationGroup("Critical Issues", "Critical severity violations");
        var errorGroup = new ViolationGroup("Errors", "Error severity violations");
        var warningGroup = new ViolationGroup("Warnings", "Warning severity violations");
        var infoGroup = new ViolationGroup("Info", "Info severity violations");

        // Add violations to each group
        criticalGroup.AddViolation(new RuleViolation("rule-critical-001", "Critical Rule", "Critical issue", "file1.cs")
        {
            Severity = SeverityLevel.Critical
        });

        for (int i = 0; i < 5; i++)
        {
            errorGroup.AddViolation(new RuleViolation("rule-error-001", "Error Rule", $"Error {i}", $"file{i}.cs")
            {
                Severity = SeverityLevel.Error
            });
        }

        for (int i = 0; i < 15; i++)
        {
            warningGroup.AddViolation(new RuleViolation("rule-warning-001", "Warning Rule", $"Warning {i}", $"file{i}.cs")
            {
                Severity = SeverityLevel.Warning
            });
        }

        for (int i = 0; i < 20; i++)
        {
            infoGroup.AddViolation(new RuleViolation("rule-info-001", "Info Rule", $"Info {i}", $"file{i}.cs")
            {
                Severity = SeverityLevel.Info
            });
        }

        report.AddViolationGroup(criticalGroup);
        report.AddViolationGroup(errorGroup);
        report.AddViolationGroup(warningGroup);
        report.AddViolationGroup(infoGroup);

        // Act
        var statistics = report.Statistics;

        // Assert - Verify totals
        statistics.TotalViolations.Should().Be(41); // 1 + 5 + 15 + 20
        statistics.CriticalCount.Should().Be(1);
        statistics.ErrorCount.Should().Be(5);
        statistics.WarningCount.Should().Be(15);
        statistics.InfoCount.Should().Be(20);
        statistics.AffectedFileCount.Should().Be(41); // Each violation in separate file

        // Verify severity score calculation
        var severityScore = statistics.CalculateSeverityScore();
        severityScore.Should().BeGreaterThan(0);
        severityScore.Should().BeLessThanOrEqualTo(100);

        // Verify IsPassing returns false (has errors and critical)
        statistics.IsPassing().Should().BeFalse();
    }

    [Fact]
    public void MultipleViolationsSameFile_ShouldCountUniqueFilesOnly()
    {
        // Arrange
        var report = new ViolationReport("Same File Report", "TestProject");
        var group = new ViolationGroup("Multiple Violations", "Multiple violations in same file");

        string filePath = "Program.cs";

        // Add 5 violations all in the same file
        for (int i = 0; i < 5; i++)
        {
            group.AddViolation(new RuleViolation("rule-001", "Test Rule", $"Violation {i}", filePath)
            {
                Severity = SeverityLevel.Warning,
                LineNumber = i + 1
            });
        }

        report.AddViolationGroup(group);

        // Act
        var statistics = report.Statistics;

        // Assert
        statistics.TotalViolations.Should().Be(5);
        statistics.AffectedFileCount.Should().Be(1, "Should count unique files only");
        statistics.AverageViolationsPerFile.Should().Be(5.0);
    }

    [Fact]
    public void MultipleViolationsDifferentFiles_ShouldCalculateAverageCorrectly()
    {
        // Arrange
        var report = new ViolationReport("Average Calculation Report", "TestProject");
        var group = new ViolationGroup("Violations by File", "Testing average calculation");

        // Add 12 violations across 4 files (3 violations per file)
        for (int fileNum = 0; fileNum < 4; fileNum++)
        {
            string filePath = $"file{fileNum}.cs";
            for (int violationNum = 0; violationNum < 3; violationNum++)
            {
                group.AddViolation(new RuleViolation("rule-001", "Test Rule", $"Violation {violationNum}", filePath)
                {
                    Severity = SeverityLevel.Warning
                });
            }
        }

        report.AddViolationGroup(group);

        // Act
        var statistics = report.Statistics;

        // Assert
        statistics.TotalViolations.Should().Be(12);
        statistics.AffectedFileCount.Should().Be(4);
        statistics.AverageViolationsPerFile.Should().Be(3.0);
    }

    [Fact]
    public void SingleViolation_ShouldHaveCorrectStatistics()
    {
        // Arrange
        var report = new ViolationReport("Single Violation Report", "TestProject");
        var group = new ViolationGroup("Single Violation", "Single violation test");

        group.AddViolation(new RuleViolation("rule-001", "Test Rule", "Single violation message", "Program.cs")
        {
            Severity = SeverityLevel.Error
        });

        report.AddViolationGroup(group);

        // Act
        var statistics = report.Statistics;

        // Assert
        statistics.TotalViolations.Should().Be(1);
        statistics.CriticalCount.Should().Be(0);
        statistics.ErrorCount.Should().Be(1);
        statistics.WarningCount.Should().Be(0);
        statistics.InfoCount.Should().Be(0);
        statistics.AffectedFileCount.Should().Be(1);
        statistics.AverageViolationsPerFile.Should().Be(1.0);
        statistics.IsPassing().Should().BeFalse("Report with errors should not be passing");
    }

    [Fact]
    public void MultipleGroupsSameRule_ShouldCountUniqueRules()
    {
        // Arrange
        var report = new ViolationReport("Multiple Groups Report", "TestProject");

        var group1 = new ViolationGroup("Group 1", "First group");
        var group2 = new ViolationGroup("Group 2", "Second group");

        // Add violations with same rule ID in different groups
        group1.AddViolation(new RuleViolation("rule-001", "Common Rule", "Violation 1 in group 1", "file1.cs")
        {
            Severity = SeverityLevel.Warning
        });

        group2.AddViolation(new RuleViolation("rule-001", "Common Rule", "Violation 2 in group 2", "file2.cs")
        {
            Severity = SeverityLevel.Warning
        });

        group1.AddViolation(new RuleViolation("rule-002", "Different Rule", "Violation 3", "file3.cs")
        {
            Severity = SeverityLevel.Error
        });

        report.AddViolationGroup(group1);
        report.AddViolationGroup(group2);

        // Act
        var statistics = report.Statistics;

        // Assert
        statistics.TotalViolations.Should().Be(3);
        statistics.RuleCount.Should().Be(2, "Should count unique rules across groups");
    }

    [Fact]
    public void Statistics_ShouldUpdateWhenAddingGroups()
    {
        // Arrange
        var report = new ViolationReport("Dynamic Update Report", "TestProject");
        var statistics = report.Statistics;

        // Initially empty
        statistics.TotalViolations.Should().Be(0);

        // Add first group with violations
        var group1 = new ViolationGroup("Group 1", "First group");
        group1.AddViolation(new RuleViolation("rule-001", "Rule 1", "Message 1", "file1.cs")
        {
            Severity = SeverityLevel.Warning
        });
        report.AddViolationGroup(group1);

        statistics.TotalViolations.Should().Be(1);

        // Add second group with violations
        var group2 = new ViolationGroup("Group 2", "Second group");
        group2.AddViolation(new RuleViolation("rule-002", "Rule 2", "Message 2", "file2.cs")
        {
            Severity = SeverityLevel.Error
        });
        group2.AddViolation(new RuleViolation("rule-003", "Rule 3", "Message 3", "file3.cs")
        {
            Severity = SeverityLevel.Critical
        });
        report.AddViolationGroup(group2);

        // Assert final state
        statistics.TotalViolations.Should().Be(3);
        statistics.ErrorCount.Should().Be(1);
        statistics.CriticalCount.Should().Be(1);
        statistics.WarningCount.Should().Be(1);
    }

    [Fact]
    public void CalculateSeverityScore_ShouldReturnValidRange()
    {
        // Arrange
        var report = new ViolationReport("Severity Score Report", "TestProject");
        var group = new ViolationGroup("Score Test", "Testing severity score calculation");

        // Add various severity violations
        group.AddViolation(new RuleViolation("rule-critical", "Critical", "Critical", "file1.cs")
        {
            Severity = SeverityLevel.Critical
        });

        for (int i = 0; i < 3; i++)
        {
            group.AddViolation(new RuleViolation("rule-error", "Error", $"Error {i}", $"file{i+2}.cs")
            {
                Severity = SeverityLevel.Error
            });
        }

        for (int i = 0; i < 5; i++)
        {
            group.AddViolation(new RuleViolation("rule-warning", "Warning", $"Warning {i}", $"file{i+5}.cs")
            {
                Severity = SeverityLevel.Warning
            });
        }

        report.AddViolationGroup(group);
        var statistics = report.Statistics;

        // Act
        var score = statistics.CalculateSeverityScore();

        // Assert
        score.Should().BeGreaterOrEqualTo(0);
        score.Should().BeLessOrEqualTo(100);

        // With 1 critical and 3 errors, score should be significantly reduced
        // Base: 100
        // Critical weight (10): 1 * 10 = 10
        // Error weight (5): 3 * 5 = 15
        // Total deduction: 25
        // Expected score: 100 - 25 = 75
        score.Should().Be(75);
    }

    [Fact]
    public void CalculateSeverityScore_WithOnlyWarnings_ShouldReturnHighScore()
    {
        // Arrange
        var report = new ViolationReport("Warning Only Report", "TestProject");
        var group = new ViolationGroup("Warnings Only", "Only warning severity violations");

        for (int i = 0; i < 10; i++)
        {
            group.AddViolation(new RuleViolation("rule-warning", "Warning", $"Warning {i}", $"file{i}.cs")
            {
                Severity = SeverityLevel.Warning
            });
        }

        report.AddViolationGroup(group);
        var statistics = report.Statistics;

        // Act
        var score = statistics.CalculateSeverityScore();

        // Assert
        // Base: 100
        // Warning weight (1): 10 * 1 = 10
        // Expected score: 100 - 10 = 90
        score.Should().Be(90);
    }

    [Fact]
    public void CalculateSeverityScore_WithNoViolations_ShouldReturnMaxScore()
    {
        // Arrange
        var report = new ViolationReport("Clean Report", "TestProject");

        // Act
        var statistics = report.Statistics;
        var score = statistics.CalculateSeverityScore();

        // Assert
        // Base: 100, no deductions
        score.Should().Be(100);
    }

    [Fact]
    public void IsPassing_WithOnlyWarningsAndInfo_ShouldReturnTrue()
    {
        // Arrange
        var report = new ViolationReport("Passing Report", "TestProject");
        var group = new ViolationGroup("Warnings and Info", "Only non-critical violations");

        for (int i = 0; i < 5; i++)
        {
            group.AddViolation(new RuleViolation("rule-warning", "Warning", $"Warning {i}", $"file{i}.cs")
            {
                Severity = SeverityLevel.Warning
            });
        }

        for (int i = 0; i < 3; i++)
        {
            group.AddViolation(new RuleViolation("rule-info", "Info", $"Info {i}", $"file{i+5}.cs")
            {
                Severity = SeverityLevel.Info
            });
        }

        report.AddViolationGroup(group);

        // Act
        var statistics = report.Statistics;
        bool isPassing = statistics.IsPassing();

        // Assert
        isPassing.Should().BeTrue("Report with only warnings and info should be passing");
    }

    [Fact]
    public void IsPassing_WithErrors_ShouldReturnFalse()
    {
        // Arrange
        var report = new ViolationReport("Failing Report", "TestProject");
        var group = new ViolationGroup("Errors", "Contains errors");

        group.AddViolation(new RuleViolation("rule-error", "Error", "Error message", "file1.cs")
        {
            Severity = SeverityLevel.Error
        });

        report.AddViolationGroup(group);

        // Act
        var statistics = report.Statistics;
        bool isPassing = statistics.IsPassing();

        // Assert
        isPassing.Should().BeFalse("Report with errors should not be passing");
    }

    [Fact]
    public void IsPassing_WithCritical_ShouldReturnFalse()
    {
        // Arrange
        var report = new ViolationReport("Critical Report", "TestProject");
        var group = new ViolationGroup("Critical", "Contains critical violations");

        group.AddViolation(new RuleViolation("rule-critical", "Critical", "Critical message", "file1.cs")
        {
            Severity = SeverityLevel.Critical
        });

        report.AddViolationGroup(group);

        // Act
        var statistics = report.Statistics;
        bool isPassing = statistics.IsPassing();

        // Assert
        isPassing.Should().BeFalse("Report with critical violations should not be passing");
    }
}