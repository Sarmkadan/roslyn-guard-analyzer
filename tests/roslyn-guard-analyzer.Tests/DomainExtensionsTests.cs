#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for DomainExtensions
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class DomainExtensionsTests
{
    private readonly List<RuleViolation> _testViolations;

    public DomainExtensionsTests()
    {
        _testViolations = new List<RuleViolation>
        {
            new RuleViolation("R1001", "LayerViolation", "Layer violation detected", "src/Services/UserService.cs") { LineNumber = 10, ColumnNumber = 5, Severity = SeverityLevel.Error, Category = RuleCategory.LayerDependency },
            new RuleViolation("R1002", "NamingViolation", "Invalid naming convention", "src/Controllers/HomeController.cs") { LineNumber = 25, ColumnNumber = 0, Severity = SeverityLevel.Warning, Category = RuleCategory.NamingConvention },
            new RuleViolation("R1003", "AsyncViolation", "Missing await keyword", "src/Services/OrderService.cs") { LineNumber = 50, ColumnNumber = 12, Severity = SeverityLevel.Critical, Category = RuleCategory.AsyncPattern },
            new RuleViolation("R1004", "NullSafetyViolation", "Possible null reference", "src/Models/User.cs") { LineNumber = 5, ColumnNumber = 8, Severity = SeverityLevel.Info, Category = RuleCategory.NullSafety },
            new RuleViolation("R1001", "LayerViolation", "Another layer violation", "src/Controllers/ProductController.cs") { LineNumber = 15, ColumnNumber = 0, Severity = SeverityLevel.Error, Category = RuleCategory.LayerDependency },
            new RuleViolation("R1005", "CodeStructureViolation", "Complex method detected", "src/Services/PaymentService.cs") { LineNumber = 100, ColumnNumber = 0, Severity = SeverityLevel.Warning, Category = RuleCategory.CodeStructure }
        };
    }

    [Fact]
    public void GetDisplayName_ForAllSeverityLevels_ReturnsCorrectStrings()
    {
        // Act & Assert
        SeverityLevel.Info.GetDisplayName().Should().Be("ℹ️ Info");
        SeverityLevel.Warning.GetDisplayName().Should().Be("⚠️ Warning");
        SeverityLevel.Error.GetDisplayName().Should().Be("❌ Error");
        SeverityLevel.Critical.GetDisplayName().Should().Be("🔴 Critical");

        // Test default case
        ((SeverityLevel)99).GetDisplayName().Should().Be("Unknown");
    }

    [Fact]
    public void GetConsoleColor_ForAllSeverityLevels_ReturnsCorrectColors()
    {
        // Act & Assert
        SeverityLevel.Info.GetConsoleColor().Should().Be(ConsoleColor.Cyan);
        SeverityLevel.Warning.GetConsoleColor().Should().Be(ConsoleColor.Yellow);
        SeverityLevel.Error.GetConsoleColor().Should().Be(ConsoleColor.Red);
        SeverityLevel.Critical.GetConsoleColor().Should().Be(ConsoleColor.Magenta);

        // Test default case
        ((SeverityLevel)99).GetConsoleColor().Should().Be(ConsoleColor.Gray);
    }

    [Fact]
    public void IsBlockingViolation_ForCriticalAndError_ReturnsTrue()
    {
        // Arrange
        var criticalViolation = new RuleViolation("R1", "Test", "Test", "test.cs") { Severity = SeverityLevel.Critical, Category = RuleCategory.CodeStructure };
        var errorViolation = new RuleViolation("R2", "Test", "Test", "test.cs") { Severity = SeverityLevel.Error, Category = RuleCategory.CodeStructure };

        // Act
        var criticalResult = criticalViolation.IsBlockingViolation();
        var errorResult = errorViolation.IsBlockingViolation();

        // Assert
        criticalResult.Should().BeTrue();
        errorResult.Should().BeTrue();
    }

    [Fact]
    public void IsBlockingViolation_ForWarningAndInfo_ReturnsFalse()
    {
        // Arrange
        var warningViolation = new RuleViolation("R1", "Test", "Test", "test.cs") { Severity = SeverityLevel.Warning, Category = RuleCategory.CodeStructure };
        var infoViolation = new RuleViolation("R2", "Test", "Test", "test.cs") { Severity = SeverityLevel.Info, Category = RuleCategory.CodeStructure };

        // Act
        var warningResult = warningViolation.IsBlockingViolation();
        var infoResult = infoViolation.IsBlockingViolation();

        // Assert
        warningResult.Should().BeFalse();
        infoResult.Should().BeFalse();
    }

    [Fact]
    public void GroupByFileAndSort_WithViolations_ReturnsGroupedAndSortedDictionary()
    {
        // Act
        var result = _testViolations.GroupByFileAndSort();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);

        // Verify all files are grouped
        result.Should().ContainKey("src/Services/UserService.cs");
        result.Should().ContainKey("src/Controllers/HomeController.cs");
        result.Should().ContainKey("src/Services/OrderService.cs");
        result.Should().ContainKey("src/Models/User.cs");
        result.Should().ContainKey("src/Controllers/ProductController.cs");
        result.Should().ContainKey("src/Services/PaymentService.cs");

        // Check that each group is sorted by line number
        result["src/Services/UserService.cs"].Should().HaveCount(1);
        result["src/Services/UserService.cs"][0].LineNumber.Should().Be(10);

        result["src/Services/OrderService.cs"].Should().HaveCount(1);
        result["src/Services/OrderService.cs"][0].LineNumber.Should().Be(50);

        result["src/Models/User.cs"].Should().HaveCount(1);
        result["src/Models/User.cs"][0].LineNumber.Should().Be(5);

        result["src/Controllers/HomeController.cs"].Should().HaveCount(1);
        result["src/Controllers/HomeController.cs"][0].LineNumber.Should().Be(25);

        result["src/Controllers/ProductController.cs"].Should().HaveCount(1);
        result["src/Controllers/ProductController.cs"][0].LineNumber.Should().Be(15);

        result["src/Services/PaymentService.cs"].Should().HaveCount(1);
        result["src/Services/PaymentService.cs"][0].LineNumber.Should().Be(100);
    }

    [Fact]
    public void GroupByFileAndSort_WithEmptyCollection_ReturnsEmptyDictionary()
    {
        // Act
        var result = Enumerable.Empty<RuleViolation>().GroupByFileAndSort();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void GroupByFileAndSort_WithNullCollection_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((IEnumerable<RuleViolation>)null!).GroupByFileAndSort();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FilterBySeverity_WithMinimumSeverity_ReturnsFilteredList()
    {
        // Act
        var result = _testViolations.FilterBySeverity(SeverityLevel.Error);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3); // 2 Errors + 1 Critical
        result.All(v => v.Severity >= SeverityLevel.Error).Should().BeTrue();
    }

    [Fact]
    public void FilterBySeverity_WithInfoMinimum_ReturnsAllViolations()
    {
        // Act
        var result = _testViolations.FilterBySeverity(SeverityLevel.Info);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(6); // All violations
    }

    [Fact]
    public void FilterBySeverity_WithCriticalMinimum_ReturnsOnlyCritical()
    {
        // Act
        var result = _testViolations.FilterBySeverity(SeverityLevel.Critical);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1); // Only 1 Critical
        result.First().Severity.Should().Be(SeverityLevel.Critical);
    }

    [Fact]
    public void FilterBySeverity_WithNullCollection_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((IEnumerable<RuleViolation>)null!).FilterBySeverity(SeverityLevel.Info);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SummarizeByCategory_WithViolations_ReturnsCategoryCounts()
    {
        // Act
        var result = _testViolations.SummarizeByCategory();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5); // 5 categories

        result[RuleCategory.LayerDependency].Should().Be(2); // R1001 appears twice
        result[RuleCategory.NamingConvention].Should().Be(1);
        result[RuleCategory.AsyncPattern].Should().Be(1);
        result[RuleCategory.NullSafety].Should().Be(1);
        result[RuleCategory.CodeStructure].Should().Be(1);
    }

    [Fact]
    public void SummarizeByCategory_WithEmptyCollection_ReturnsEmptyDictionary()
    {
        // Act
        var result = Enumerable.Empty<RuleViolation>().SummarizeByCategory();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void SummarizeByCategory_WithNullCollection_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((IEnumerable<RuleViolation>)null!).SummarizeByCategory();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CalculateSeverityPercentages_WithViolations_ReturnsCorrectPercentages()
    {
        // Act
        var result = _testViolations.CalculateSeverityPercentages();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4); // All 4 severity levels present

        // 6 total violations: 1 Critical, 2 Error, 2 Warning, 1 Info
        result[SeverityLevel.Critical].Should().BeApproximately(16.67, 0.01); // 1/6 * 100
        result[SeverityLevel.Error].Should().BeApproximately(33.33, 0.01);   // 2/6 * 100
        result[SeverityLevel.Warning].Should().BeApproximately(33.33, 0.01); // 2/6 * 100
        result[SeverityLevel.Info].Should().BeApproximately(16.67, 0.01);   // 1/6 * 100
    }

    [Fact]
    public void CalculateSeverityPercentages_WithEmptyCollection_ReturnsEmptyDictionary()
    {
        // Act
        var result = Enumerable.Empty<RuleViolation>().CalculateSeverityPercentages();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void CalculateSeverityPercentages_WithNullCollection_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((IEnumerable<RuleViolation>)null!).CalculateSeverityPercentages();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetMostCommonRule_WithViolations_ReturnsMostFrequentRuleId()
    {
        // Act
        var result = _testViolations.GetMostCommonRule();

        // Assert
        result.Should().Be("R1001"); // Appears twice
    }

    [Fact]
    public void GetMostCommonRule_WithEmptyCollection_ReturnsNull()
    {
        // Act
        var result = Enumerable.Empty<RuleViolation>().GetMostCommonRule();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetMostCommonRule_WithNullCollection_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((IEnumerable<RuleViolation>)null!).GetMostCommonRule();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetMostProblematicFile_WithViolations_ReturnsFileWithMostViolations()
    {
        // Act
        var result = _testViolations.GetMostProblematicFile();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetMostProblematicFile_WithEmptyCollection_ReturnsNull()
    {
        // Act
        var result = Enumerable.Empty<RuleViolation>().GetMostProblematicFile();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetMostProblematicFile_WithNullCollection_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((IEnumerable<RuleViolation>)null!).GetMostProblematicFile();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExportToText_WithViolations_ReturnsFormattedString()
    {
        // Act
        var result = _testViolations.ExportToText("Test Export");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("═══ Test Export ═══");
        result.Should().Contain("Exported:");
        result.Should().Contain("[R1001]"); // Should contain violation details
        result.Should().Contain("Layer violation detected");
    }

    [Fact]
    public void ExportToText_WithEmptyCollection_ReturnsHeaderOnly()
    {
        // Act
        var result = Enumerable.Empty<RuleViolation>().ExportToText("Empty Test");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("═══ Empty Test ═══");
        result.Should().Contain("Exported:");
        // Should not contain any violation details since collection is empty
        result.Should().NotContain("["); // No violation IDs
    }


    [Fact]
    public void ExportToText_WithDefaultTitle_UsesViolationsExport()
    {
        // Act
        var result = _testViolations.ExportToText();

        // Assert
        result.Should().Contain("═══ Violations Export ═══");
    }
}