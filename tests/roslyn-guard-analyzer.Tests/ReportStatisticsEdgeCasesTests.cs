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
/// Additional edge case tests for ReportStatistics aggregation behavior.
/// Tests scenarios where statistics might be inconsistent or edge cases not covered by basic tests.
/// </summary>
public sealed class ReportStatisticsEdgeCasesTests
{
    [Fact]
    public void EmptyReport_ShouldHaveZeroAverageViolationsPerFile()
    {
        // Arrange
        var report = new ViolationReport("Empty Report", "TestProject");

        // Act
        var statistics = report.Statistics;

        // Assert - Average should be 0, not NaN or infinity
        statistics.AverageViolationsPerFile.Should().Be(0, "Average violations per file should be 0 for empty report");
        statistics.TotalViolations.Should().Be(0);
        statistics.AffectedFileCount.Should().Be(0);
    }

    [Fact]
    public void ReportWithOnlyCriticalViolations_ShouldCalculateCorrectly()
    {
        // Arrange
        var report = new ViolationReport("Critical Only Report", "TestProject");
        var group = new ViolationGroup("Critical Issues", "All critical violations");

        // Add 7 critical violations
        for (int i = 0; i < 7; i++)
        {
            group.AddViolation(new RuleViolation("critical-rule-001", "Critical Rule", $"Critical violation {i}", $"file{i}.cs")
            {
                Severity = SeverityLevel.Critical
            });
        }

        report.AddViolationGroup(group);

        // Act
        var statistics = report.Statistics;

        // Assert
        statistics.TotalViolations.Should().Be(7);
        statistics.CriticalCount.Should().Be(7);
        statistics.ErrorCount.Should().Be(0);
        statistics.WarningCount.Should().Be(0);
        statistics.InfoCount.Should().Be(0);
        statistics.AffectedFileCount.Should().Be(7);
        statistics.AverageViolationsPerFile.Should().Be(1.0);
        statistics.IsPassing().Should().BeFalse("Report with critical violations should not be passing");
    }

    [Fact]
    public void ReportWithOnlyInfoViolations_ShouldBePassing()
    {
        // Arrange
        var report = new ViolationReport("Info Only Report", "TestProject");
        var group = new ViolationGroup("Info Issues", "Only informational violations");

        // Add 10 info violations
        for (int i = 0; i < 10; i++)
        {
            group.AddViolation(new RuleViolation("info-rule-001", "Info Rule", $"Info violation {i}", $"file{i}.cs")
            {
                Severity = SeverityLevel.Info
            });
        }

        report.AddViolationGroup(group);

        // Act
        var statistics = report.Statistics;

        // Assert
        statistics.TotalViolations.Should().Be(10);
        statistics.InfoCount.Should().Be(10);
        statistics.CriticalCount.Should().Be(0);
        statistics.ErrorCount.Should().Be(0);
        statistics.WarningCount.Should().Be(0);
        statistics.IsPassing().Should().BeTrue("Report with only info violations should be passing");
    }

    [Fact]
    public void MultipleFilesWithDifferentViolationCounts_ShouldCalculateAverageCorrectly()
    {
        // Arrange
        var report = new ViolationReport("Variable File Count Report", "TestProject");
        var group = new ViolationGroup("Variable Violations", "Testing average with different file counts");

        // Create pattern: file1 has 5 violations, file2 has 3, file3 has 1
        string[] filePaths = { "file1.cs", "file2.cs", "file3.cs" };
        int[] violationsPerFile = { 5, 3, 1 };

        for (int fileIndex = 0; fileIndex < filePaths.Length; fileIndex++)
        {
            for (int violationNum = 0; violationNum < violationsPerFile[fileIndex]; violationNum++)
            {
                group.AddViolation(new RuleViolation("rule-001", "Test Rule", $"Violation {violationNum} in {filePaths[fileIndex]}", filePaths[fileIndex])
                {
                    Severity = SeverityLevel.Warning
                });
            }
        }

        report.AddViolationGroup(group);

        // Act
        var statistics = report.Statistics;

        // Assert
        statistics.TotalViolations.Should().Be(9); // 5 + 3 + 1
        statistics.AffectedFileCount.Should().Be(3);
        statistics.AverageViolationsPerFile.Should().Be(3.0); // 9 / 3 = 3.0
    }

    [Fact]
    public void SingleFileWithManyViolations_ShouldCalculateAverageCorrectly()
    {
        // Arrange
        var report = new ViolationReport("Single File Heavy Report", "TestProject");
        var group = new ViolationGroup("Heavy File", "Single file with many violations");

        const int violationCount = 50;
        string filePath = "Program.cs";

        // Add many violations to single file
        for (int i = 0; i < violationCount; i++)
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
        statistics.TotalViolations.Should().Be(violationCount);
        statistics.AffectedFileCount.Should().Be(1);
        statistics.AverageViolationsPerFile.Should().Be(50.0);
        statistics.IsPassing().Should().BeTrue();
    }

    [Fact]
    public void CalculateSeverityScore_WithMaximumCriticalViolations()
    {
        // Arrange
        var report = new ViolationReport("Max Critical Report", "TestProject");
        var group = new ViolationGroup("Max Critical", "Testing maximum critical impact");

        // Add maximum critical violations that would drive score to minimum
        for (int i = 0; i < 20; i++)
        {
            group.AddViolation(new RuleViolation("critical-rule", "Critical", $"Critical {i}", $"file{i}.cs")
            {
                Severity = SeverityLevel.Critical
            });
        }

        report.AddViolationGroup(group);
        var statistics = report.Statistics;

        // Act
        var score = statistics.CalculateSeverityScore();

        // Assert - With 20 critical violations: 20 * 10 = 200 deduction
        // Score = max(0, 100 - 200) = max(0, -100) = 0
        score.Should().Be(0, "Score should be clamped to minimum of 0");
    }

    [Fact]
    public void CalculateSeverityScore_WithMaximumErrorViolations()
    {
        // Arrange
        var report = new ViolationReport("Max Error Report", "TestProject");
        var group = new ViolationGroup("Max Errors", "Testing maximum error impact");

        // Add maximum error violations that would drive score close to minimum
        for (int i = 0; i < 20; i++)
        {
            group.AddViolation(new RuleViolation("error-rule", "Error", $"Error {i}", $"file{i}.cs")
            {
                Severity = SeverityLevel.Error
            });
        }

        report.AddViolationGroup(group);
        var statistics = report.Statistics;

        // Act
        var score = statistics.CalculateSeverityScore();

        // Assert - With 20 error violations: 20 * 5 = 100 deduction
        // Score = max(0, 100 - 100) = max(0, 0) = 0
        score.Should().Be(0, "Score should be clamped to minimum of 0");
    }

    [Fact]
    public void Statistics_ShouldBeConsistentAcrossMultipleUpdates()
    {
        // Arrange
        var report = new ViolationReport("Consistency Test Report", "TestProject");
        var statistics = report.Statistics;

        // Initial state
        statistics.TotalViolations.Should().Be(0);
        statistics.CriticalCount.Should().Be(0);
        statistics.ErrorCount.Should().Be(0);
        statistics.WarningCount.Should().Be(0);
        statistics.InfoCount.Should().Be(0);
        statistics.AffectedFileCount.Should().Be(0);
        statistics.AffectedNamespaceCount.Should().Be(0);
        statistics.RuleCount.Should().Be(0);
        statistics.AverageViolationsPerFile.Should().Be(0);

        // Add first batch
        var group1 = new ViolationGroup("Group 1", "First batch");
        group1.AddViolation(new RuleViolation("rule-1", "Rule 1", "Message 1", "file1.cs") { Severity = SeverityLevel.Error });
        group1.AddViolation(new RuleViolation("rule-2", "Rule 2", "Message 2", "file2.cs") { Severity = SeverityLevel.Warning });
        report.AddViolationGroup(group1);

        // Verify consistency after first batch
        statistics = report.Statistics;
        statistics.TotalViolations.Should().Be(2);
        statistics.ErrorCount.Should().Be(1);
        statistics.WarningCount.Should().Be(1);
        statistics.AffectedFileCount.Should().Be(2);

        // Add second batch
        var group2 = new ViolationGroup("Group 2", "Second batch");
        group2.AddViolation(new RuleViolation("rule-3", "Rule 3", "Message 3", "file3.cs") { Severity = SeverityLevel.Critical });
        group2.AddViolation(new RuleViolation("rule-4", "Rule 4", "Message 4", "file4.cs") { Severity = SeverityLevel.Error });
        group2.AddViolation(new RuleViolation("rule-5", "Rule 5", "Message 5", "file5.cs") { Severity = SeverityLevel.Error });
        report.AddViolationGroup(group2);

        // Verify final consistency
        statistics = report.Statistics;
        statistics.TotalViolations.Should().Be(5);
        statistics.CriticalCount.Should().Be(1);
        statistics.ErrorCount.Should().Be(3); // 1 from first batch + 2 from second
        statistics.WarningCount.Should().Be(1);
        statistics.AffectedFileCount.Should().Be(5);
        statistics.AverageViolationsPerFile.Should().Be(1.0);
        statistics.IsPassing().Should().BeFalse("Report with critical and errors should not be passing");
    }

    [Fact]
    public void MultipleGroupsWithSameFilePath_ShouldCountUniqueFilesOnly()
    {
        // Arrange
        var report = new ViolationReport("Same File Different Groups", "TestProject");

        var group1 = new ViolationGroup("Group 1", "First group");
        var group2 = new ViolationGroup("Group 2", "Second group");
        var group3 = new ViolationGroup("Group 3", "Third group");

        string filePath = "SharedFile.cs";

        // Add violations to same file across different groups
        for (int i = 0; i < 3; i++)
        {
            group1.AddViolation(new RuleViolation("rule-1", "Rule 1", $"Group1 violation {i}", filePath) { Severity = SeverityLevel.Warning });
        }

        for (int i = 0; i < 2; i++)
        {
            group2.AddViolation(new RuleViolation("rule-2", "Rule 2", $"Group2 violation {i}", filePath) { Severity = SeverityLevel.Error });
        }

        for (int i = 0; i < 5; i++)
        {
            group3.AddViolation(new RuleViolation("rule-3", "Rule 3", $"Group3 violation {i}", filePath) { Severity = SeverityLevel.Info });
        }

        report.AddViolationGroup(group1);
        report.AddViolationGroup(group2);
        report.AddViolationGroup(group3);

        // Act
        var statistics = report.Statistics;

        // Assert
        statistics.TotalViolations.Should().Be(10); // 3 + 2 + 5
        statistics.AffectedFileCount.Should().Be(1, "Should count unique files only, even across multiple groups");
        statistics.AverageViolationsPerFile.Should().Be(10.0);
    }

    [Fact]
    public void EmptyViolationGroup_ShouldNotAffectStatistics()
    {
        // Arrange
        var report = new ViolationReport("Report With Empty Group", "TestProject");

        var emptyGroup = new ViolationGroup("Empty Group", "This group has no violations");
        var populatedGroup = new ViolationGroup("Populated Group", "This group has violations");

        populatedGroup.AddViolation(new RuleViolation("rule-1", "Rule 1", "Message", "file1.cs") { Severity = SeverityLevel.Warning });

        report.AddViolationGroup(emptyGroup);
        report.AddViolationGroup(populatedGroup);

        // Act
        var statistics = report.Statistics;

        // Assert
        statistics.TotalViolations.Should().Be(1);
        statistics.WarningCount.Should().Be(1);
        statistics.AffectedFileCount.Should().Be(1);
    }

    [Fact]
    public void CalculateSeverityScore_WithMixedSeveritiesAndClamping()
    {
        // Arrange - Test the clamping behavior at boundaries
        var report = new ViolationReport("Boundary Test Report", "TestProject");
        var group = new ViolationGroup("Boundary Test", "Testing score clamping");

        // Add violations that would result in score just above 0
        group.AddViolation(new RuleViolation("critical", "Critical", "Critical", "file1.cs") { Severity = SeverityLevel.Critical });
        for (int i = 0; i < 19; i++) // 19 more criticals = 20 total
        {
            group.AddViolation(new RuleViolation("critical", "Critical", $"Critical {i}", $"file{i+2}.cs") { Severity = SeverityLevel.Critical });
        }

        report.AddViolationGroup(group);
        var statistics = report.Statistics;

        // Act
        var score = statistics.CalculateSeverityScore();

        // Assert - 20 criticals: 20 * 10 = 200 deduction
        // Score = max(0, 100 - 200) = 0
        score.Should().Be(0);
    }
}