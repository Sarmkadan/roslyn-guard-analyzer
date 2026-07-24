#nullable enable

using System;
using System.Collections.Generic;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="ViolationReport"/> functionality.
/// Tests constructors, properties, methods and edge cases.
/// </summary>
public class ViolationReportTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Parameterless_InitializesWithDefaults()
    {
        // Act
        var report = new ViolationReport();

        // Assert
        report.Id.Should().NotBeNullOrEmpty();
        report.Title.Should().Be(string.Empty);
        report.ProjectName.Should().Be(string.Empty);
        report.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        report.ViolationGroups.Should().NotBeNull();
        report.ViolationGroups.Should().BeEmpty();
        report.Statistics.Should().NotBeNull();
        report.Summary.Should().Be(string.Empty);
        report.DetailedContent.Should().Be(string.Empty);
        report.Format.Should().Be(ReportFormat.Text);
    }

    [Fact]
    public void Constructor_WithTitleAndProjectName_SetsPropertiesCorrectly()
    {
        // Arrange
        const string title = "Test Report";
        const string projectName = "TestProject";

        // Act
        var report = new ViolationReport(title, projectName);

        // Assert
        report.Id.Should().NotBeNullOrEmpty();
        report.Title.Should().Be(title);
        report.ProjectName.Should().Be(projectName);
        report.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        report.ViolationGroups.Should().NotBeNull();
        report.ViolationGroups.Should().BeEmpty();
        report.Statistics.Should().NotBeNull();
        report.Summary.Should().Be(string.Empty);
        report.DetailedContent.Should().Be(string.Empty);
        report.Format.Should().Be(ReportFormat.Text);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Id_Property_CanBeSetAndRetrieved()
    {
        // Arrange
        var report = new ViolationReport();
        var expectedId = "test-id-123";

        // Act
        report.Id = expectedId;

        // Assert
        report.Id.Should().Be(expectedId);
    }

    [Fact]
    public void Title_Property_CanBeSetAndRetrieved()
    {
        // Arrange
        var report = new ViolationReport();
        const string expectedTitle = "My Test Report";

        // Act
        report.Title = expectedTitle;

        // Assert
        report.Title.Should().Be(expectedTitle);
    }

    [Fact]
    public void ProjectName_Property_CanBeSetAndRetrieved()
    {
        // Arrange
        var report = new ViolationReport();
        const string expectedProjectName = "MyTestProject";

        // Act
        report.ProjectName = expectedProjectName;

        // Assert
        report.ProjectName.Should().Be(expectedProjectName);
    }

    [Fact]
    public void GeneratedAt_Property_CanBeSetAndRetrieved()
    {
        // Arrange
        var report = new ViolationReport();
        var expectedDate = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        report.GeneratedAt = expectedDate;

        // Assert
        report.GeneratedAt.Should().Be(expectedDate);
    }

    [Fact]
    public void ViolationGroups_Property_CanBeSetAndRetrieved()
    {
        // Arrange
        var report = new ViolationReport();
        var expectedGroups = new List<ViolationGroup>
        {
            new ViolationGroup("Group1", "First group"),
            new ViolationGroup("Group2", "Second group")
        };

        // Act
        report.ViolationGroups = expectedGroups;

        // Assert
        report.ViolationGroups.Should().BeSameAs(expectedGroups);
    }

    [Fact]
    public void Statistics_Property_CanBeSetAndRetrieved()
    {
        // Arrange
        var report = new ViolationReport();
        var expectedStats = new ReportStatistics
        {
            TotalViolations = 5,
            CriticalCount = 2
        };

        // Act
        report.Statistics = expectedStats;

        // Assert
        report.Statistics.Should().BeSameAs(expectedStats);
    }

    [Fact]
    public void Summary_Property_CanBeSetAndRetrieved()
    {
        // Arrange
        var report = new ViolationReport();
        const string expectedSummary = "Test summary";

        // Act
        report.Summary = expectedSummary;

        // Assert
        report.Summary.Should().Be(expectedSummary);
    }

    [Fact]
    public void DetailedContent_Property_CanBeSetAndRetrieved()
    {
        // Arrange
        var report = new ViolationReport();
        const string expectedContent = "Detailed content here";

        // Act
        report.DetailedContent = expectedContent;

        // Assert
        report.DetailedContent.Should().Be(expectedContent);
    }

    [Fact]
    public void Format_Property_CanBeSetAndRetrieved()
    {
        // Arrange
        var report = new ViolationReport();

        // Act & Assert
        foreach (ReportFormat format in Enum.GetValues(typeof(ReportFormat)))
        {
            report.Format = format;
            report.Format.Should().Be(format);
        }
    }

    #endregion

    #region Method Tests

    [Fact]
    public void AddViolationGroup_WithNullGroup_DoesNothing()
    {
        // Arrange
        var report = new ViolationReport();
        var initialCount = report.ViolationGroups.Count;

        // Act
        report.AddViolationGroup(null);

        // Assert
        report.ViolationGroups.Count.Should().Be(initialCount);
    }

    [Fact]
    public void AddViolationGroup_WithValidGroup_AddsToCollection()
    {
        // Arrange
        var report = new ViolationReport();
        var group = new ViolationGroup("Test Group", "A test group");

        // Act
        report.AddViolationGroup(group);

        // Assert
        report.ViolationGroups.Should().Contain(group);
        report.ViolationGroups.Count.Should().Be(1);
    }

    [Fact]
    public void AddViolationGroup_WithValidGroup_UpdatesStatistics()
    {
        // Arrange
        var report = new ViolationReport();
        var violation = new RuleViolation("CA1822", "Mark members as static", "Test message", "Test.cs")
        {
            Severity = SeverityLevel.Warning,
            LineNumber = 10
        };
        var group = new ViolationGroup("Test Group", "A test group");
        group.AddViolation(violation);

        // Act
        report.AddViolationGroup(group);

        // Assert
        report.Statistics.TotalViolations.Should().Be(1);
        report.Statistics.WarningCount.Should().Be(1);
    }

    [Fact]
    public void GetViolationsBySeverity_WithNoViolations_ReturnsEmptyDictionary()
    {
        // Arrange
        var report = new ViolationReport();

        // Act
        var result = report.GetViolationsBySeverity();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetViolationsBySeverity_WithMixedSeverities_ReturnsGroupedCorrectly()
    {
        // Arrange
        var report = new ViolationReport();

        var criticalViolation = new RuleViolation("CA1822", "Critical Rule", "Critical message", "Test.cs")
        {
            Severity = SeverityLevel.Critical,
            LineNumber = 10
        };

        var errorViolation = new RuleViolation("CA1822", "Error Rule", "Error message", "Test.cs")
        {
            Severity = SeverityLevel.Error,
            LineNumber = 20
        };

        var warningViolation = new RuleViolation("CA1822", "Warning Rule", "Warning message", "Test.cs")
        {
            Severity = SeverityLevel.Warning,
            LineNumber = 30
        };

        var infoViolation = new RuleViolation("CA1822", "Info Rule", "Info message", "Test.cs")
        {
            Severity = SeverityLevel.Info,
            LineNumber = 40
        };

        var group1 = new ViolationGroup("Group1", "First group");
        group1.AddViolation(criticalViolation);
        group1.AddViolation(errorViolation);

        var group2 = new ViolationGroup("Group2", "Second group");
        group2.AddViolation(warningViolation);
        group2.AddViolation(infoViolation);

        report.AddViolationGroup(group1);
        report.AddViolationGroup(group2);

        // Act
        var result = report.GetViolationsBySeverity();

        // Assert
        result.Should().ContainKey(SeverityLevel.Critical);
        result[SeverityLevel.Critical].Should().Contain(criticalViolation);

        result.Should().ContainKey(SeverityLevel.Error);
        result[SeverityLevel.Error].Should().Contain(errorViolation);

        result.Should().ContainKey(SeverityLevel.Warning);
        result[SeverityLevel.Warning].Should().Contain(warningViolation);

        result.Should().ContainKey(SeverityLevel.Info);
        result[SeverityLevel.Info].Should().Contain(infoViolation);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void GetTotalViolationCount_WithNoViolations_ReturnsZero()
    {
        // Arrange
        var report = new ViolationReport();

        // Act
        var count = report.GetTotalViolationCount();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void GetTotalViolationCount_WithViolations_ReturnsCorrectCount()
    {
        // Arrange
        var report = new ViolationReport();

        var violation1 = new RuleViolation("CA1822", "Rule1", "Message1", "Test1.cs");
        var violation2 = new RuleViolation("CA1822", "Rule2", "Message2", "Test2.cs");
        var violation3 = new RuleViolation("CA1822", "Rule3", "Message3", "Test3.cs");

        var group1 = new ViolationGroup("Group1", "First group");
        group1.AddViolation(violation1);
        group1.AddViolation(violation2);

        var group2 = new ViolationGroup("Group2", "Second group");
        group2.AddViolation(violation3);

        report.AddViolationGroup(group1);
        report.AddViolationGroup(group2);

        // Act
        var count = report.GetTotalViolationCount();

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public void GetViolationsFromFile_WithNoViolations_ReturnsEmptyList()
    {
        // Arrange
        var report = new ViolationReport();

        // Act
        var violations = report.GetViolationsFromFile("Test.cs");

        // Assert
        violations.Should().BeEmpty();
    }

    [Fact]
    public void GetViolationsFromFile_WithMatchingViolations_ReturnsCorrectList()
    {
        // Arrange
        var report = new ViolationReport();

        var violation1 = new RuleViolation("CA1822", "Rule1", "Message1", "Test.cs")
        {
            LineNumber = 10
        };

        var violation2 = new RuleViolation("CA1822", "Rule2", "Message2", "test.cs") // Different case
        {
            LineNumber = 20
        };

        var violation3 = new RuleViolation("CA1822", "Rule3", "Message3", "Other.cs")
        {
            LineNumber = 30
        };

        var group = new ViolationGroup("Test Group", "A test group");
        group.AddViolation(violation1);
        group.AddViolation(violation2);
        group.AddViolation(violation3);

        report.AddViolationGroup(group);

        // Act
        var violations = report.GetViolationsFromFile("TEST.CS"); // Test case insensitive

        // Assert
        violations.Should().HaveCount(2);
        violations.Should().Contain(violation1);
        violations.Should().Contain(violation2);
        violations.Should().NotContain(violation3);
    }

    [Fact]
    public void GetViolationsFromFile_WithNullOrEmptyPath_ReturnsEmptyList()
    {
        // Arrange
        var report = new ViolationReport();

        var violation = new RuleViolation("CA1822", "Rule1", "Message1", "Test.cs");
        var group = new ViolationGroup("Test Group", "A test group");
        group.AddViolation(violation);
        report.AddViolationGroup(group);

        // Act
        var emptyResult = report.GetViolationsFromFile(string.Empty);
        var whitespaceResult = report.GetViolationsFromFile("   ");

        // Assert
        emptyResult.Should().BeEmpty();
        whitespaceResult.Should().BeEmpty();
    }

    [Fact]
    public void GenerateSummary_WithNoViolations_ReturnsFormattedSummary()
    {
        // Arrange
        var report = new ViolationReport("Test Report", "TestProject");

        // Act
        var summary = report.GenerateSummary();

        // Assert
        summary.Should().Contain("Analysis Report: Test Report");
        summary.Should().Contain("Project: TestProject");
        summary.Should().Contain("Total Violations: 0");
        summary.Should().Contain("Critical: 0");
        summary.Should().Contain("Errors: 0");
        summary.Should().Contain("Warnings: 0");
        summary.Should().Contain("Affected Files: 0");
    }

    [Fact]
    public void GenerateSummary_WithViolations_IncludesCorrectCounts()
    {
        // Arrange
        var report = new ViolationReport("Test Report", "TestProject");

        var criticalViolation = new RuleViolation("CA1822", "Critical Rule", "Critical message", "Test.cs")
        {
            Severity = SeverityLevel.Critical,
            LineNumber = 10
        };

        var errorViolation = new RuleViolation("CA1822", "Error Rule", "Error message", "Test.cs")
        {
            Severity = SeverityLevel.Error,
            LineNumber = 20
        };

        var warningViolation = new RuleViolation("CA1822", "Warning Rule", "Warning message", "Test.cs")
        {
            Severity = SeverityLevel.Warning,
            LineNumber = 30
        };

        var group = new ViolationGroup("Test Group", "A test group");
        group.AddViolation(criticalViolation);
        group.AddViolation(errorViolation);
        group.AddViolation(warningViolation);

        report.AddViolationGroup(group);

        // Act
        var summary = report.GenerateSummary();

        // Assert
        summary.Should().Contain("Analysis Report: Test Report");
        summary.Should().Contain("Project: TestProject");
        summary.Should().Contain("Total Violations: 3");
        summary.Should().Contain("Critical: 1");
        summary.Should().Contain("Errors: 1");
        summary.Should().Contain("Warnings: 1");
        summary.Should().Contain("Affected Files: 1");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void ViolationReport_WithLargeNumberOfViolations_HandlesCorrectly()
    {
        // Arrange
        var report = new ViolationReport();
        const int violationCount = 1000;

        // Act
        for (int i = 0; i < violationCount; i++)
        {
            var violation = new RuleViolation(
                $"CA{i:D4}",
                $"Rule {i}",
                $"Message {i}",
                $"File{i}.cs")
            {
                LineNumber = i,
                Severity = (SeverityLevel)(i % 4) // Cycle through all severities
            };

            var group = new ViolationGroup($"Group{i}", $"Group {i}");
            group.AddViolation(violation);
            report.AddViolationGroup(group);
        }

        // Assert
        report.GetTotalViolationCount().Should().Be(violationCount);
        report.Statistics.TotalViolations.Should().Be(violationCount);
    }

    [Fact]
    public void ViolationReport_WithDuplicateViolationGroups_AllowsDuplicates()
    {
        // Arrange
        var report = new ViolationReport();
        var group1 = new ViolationGroup("Same Name", "First group");
        var group2 = new ViolationGroup("Same Name", "Second group"); // Same name, different instance

        // Act
        report.AddViolationGroup(group1);
        report.AddViolationGroup(group2);

        // Assert
        report.ViolationGroups.Should().HaveCount(2);
        report.ViolationGroups.Should().Contain(group1);
        report.ViolationGroups.Should().Contain(group2);
    }

    #endregion
}