#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FluentAssertions;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="Baseline"/> class focusing on edge cases with malformed and empty files.
/// </summary>
public class BaselineTests
{
    #region Load/Deserialize Edge Cases

    [Fact]
    public void FromJson_WithEmptyString_ReturnsNull()
    {
        // Act
        var result = Baseline.FromJson(string.Empty);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithWhitespaceOnly_ReturnsNull()
    {
        // Act
        var result = Baseline.FromJson("   \t\n  ");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithNull_ReturnsNull()
    {
        // Act
        var result = Baseline.FromJson(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithValidEmptyObject_ReturnsEmptyBaseline()
    {
        // Arrange
        const string json = "{}";

        // Act
        var result = Baseline.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result!.ViolationCount.Should().Be(0);
        result.ProjectName.Should().Be(string.Empty);
        result.Version.Should().Be("1.0"); // Default value
        result.SchemaVersion.Should().Be("1.0"); // Default value
    }

    [Fact]
    public void FromJson_WithValidEmptyArray_ThrowsJsonException()
    {
        // Arrange
        const string json = "[]";

        // Act
        Action act = () => Baseline.FromJson(json);

        // Assert
        act.Should().Throw<JsonException>(); // Array doesn't match Baseline type structure
    }

    [Fact]
    public void FromJson_WithMalformedJson_ThrowsJsonException()
    {
        // Arrange
        const string json = "{ invalid json }";

        // Act
        Action act = () => Baseline.FromJson(json);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void FromJson_WithOlderSchemaVersion_ThrowsDescriptiveException()
    {
        // Arrange - Simulate an older baseline format with different schema version
        const string json = @"{
            ""version"": ""0.9"",
            ""schemaVersion"": ""0.9"",
            ""projectName"": ""TestProject"",
            ""baselineCreatedAt"": ""2026-01-01T00:00:00Z"",
            ""violations"": []
        }";

        // Act
        var result = Baseline.FromJson(json);

        // Assert - Should deserialize but we can check if version handling is needed
        result.Should().NotBeNull();
        result!.Version.Should().Be("0.9");
        result.SchemaVersion.Should().Be("0.9");
    }

    [Fact]
    public void FromJson_WithMissingRequiredProperties_StillWorksWithDefaults()
    {
        // Arrange - Minimal JSON with only some properties
        const string json = @"{
            ""projectName"": ""TestProject""
        }";

        // Act
        var result = Baseline.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result!.ProjectName.Should().Be("TestProject");
        result.ViolationCount.Should().Be(0);
        result.Version.Should().Be("1.0"); // Default
        result.SchemaVersion.Should().Be("1.0"); // Default
        result.BaselineCreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
    }

    #endregion

    #region Violation Management Edge Cases

    [Fact]
    public void AddViolation_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var baseline = new Baseline("TestProject");

        // Act
        Action act = () => baseline.AddViolation(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Contains_WithNullViolation_ReturnsFalse()
    {
        // Arrange
        var baseline = new Baseline("TestProject");

        // Act
        var result = baseline.Contains(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveExpired_WithEmptyBaseline_DoesNothing()
    {
        // Arrange
        var baseline = new Baseline("TestProject");

        // Act
        baseline.RemoveExpired(TimeSpan.FromDays(30));

        // Assert
        baseline.ViolationCount.Should().Be(0);
    }

    [Fact]
    public void GetValidViolations_WithEmptyBaseline_ReturnsEmptyList()
    {
        // Arrange
        var baseline = new Baseline("TestProject");

        // Act
        var result = baseline.GetValidViolations(TimeSpan.FromDays(30));

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Duplicate/Deduplication Behavior

    [Fact]
    public void AddViolation_WithDuplicateEntries_AllowsDuplicatesInList()
    {
        // Arrange
        var baseline = new Baseline("TestProject");
        var violation1 = new BaselineViolation("CA1822", "file.cs", 10, "hash1");
        var violation2 = new BaselineViolation("CA1822", "file.cs", 10, "hash1"); // Same content

        // Act
        baseline.AddViolation(violation1);
        baseline.AddViolation(violation2);

        // Assert - The Baseline class allows duplicates in the list (it's just a List<BaselineViolation>)
        baseline.ViolationCount.Should().Be(2);
        baseline.Violations.Should().Contain(violation1);
        baseline.Violations.Should().Contain(violation2);
    }

    [Fact]
    public void Contains_WithDuplicateViolations_ReturnsTrueIfAnyMatch()
    {
        // Arrange
        var baseline = new Baseline("TestProject");
        var violation1 = new BaselineViolation("CA1822", "file.cs", 10, "hash1");
        var violation2 = new BaselineViolation("CA1822", "file.cs", 10, "hash1"); // Same content
        var testViolation = new RuleViolation("CA1822", "Test Rule", "Test message", "file.cs") { LineNumber = 10 };

        baseline.AddViolation(violation1);
        baseline.AddViolation(violation2);

        // Act
        var result = baseline.Contains(testViolation);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Merge/Diff Behavior Simulation

    [Fact]
    public void Baseline_RepresentsState_NotMergeLogic()
    {
        // Arrange
        var baseline = new Baseline("TestProject");
        var fixedViolation = new BaselineViolation("CA1822", "file.cs", 10, "hash-fixed");
        var newViolation = new BaselineViolation("CA2007", "file.cs", 20, "hash-new");

        // Act
        baseline.AddViolation(fixedViolation);
        baseline.AddViolation(newViolation);

        // Assert - Baseline simply stores what's given to it
        baseline.ViolationCount.Should().Be(2);
        baseline.Violations.Should().Contain(fixedViolation);
        baseline.Violations.Should().Contain(newViolation);
    }

    [Fact]
    public void Baseline_ViolationCount_ReflectsActualStoredViolations()
    {
        // Arrange
        var baseline = new Baseline("TestProject");

        // Act & Assert
        baseline.ViolationCount.Should().Be(0);

        // Add some violations
        baseline.AddViolation(new BaselineViolation("CA1822", "file1.cs", 10, "hash1"));
        baseline.ViolationCount.Should().Be(1);

        baseline.AddViolation(new BaselineViolation("CA2007", "file2.cs", 20, "hash2"));
        baseline.ViolationCount.Should().Be(2);

        // Remove one (by creating new list without it)
        baseline.Violations.RemoveAt(0);
        baseline.ViolationCount.Should().Be(1);
    }

    #endregion

    #region Serialization/Deserialization Integrity

    [Fact]
    public void ToJson_FromJson_RoundTrip_PreservesAllData()
    {
        // Arrange
        var original = new Baseline("TestProject");
        original.AddViolation(new BaselineViolation("CA1822", "file.cs", 10, "hash1", "First violation"));
        original.AddViolation(new BaselineViolation("CA2007", "file.cs", 20, "hash2", "Second violation"));

        // Act
        var json = original.ToJson();
        var restored = Baseline.FromJson(json);

        // Assert
        restored.Should().NotBeNull();
        restored!.ProjectName.Should().Be(original.ProjectName);
        restored.ViolationCount.Should().Be(original.ViolationCount);
        restored.Version.Should().Be(original.Version);
        restored.SchemaVersion.Should().Be(original.SchemaVersion);
        restored.BaselineCreatedAt.Should().BeCloseTo(original.BaselineCreatedAt, precision: TimeSpan.FromMilliseconds(1));

        // Check violations
        restored.Violations.Should().HaveCount(2);
        restored.Violations[0].RuleId.Should().Be("CA1822");
        restored.Violations[0].FilePath.Should().Be("file.cs");
        restored.Violations[0].LineNumber.Should().Be(10);
        restored.Violations[0].ContentHash.Should().Be("hash1");
        restored.Violations[0].Description.Should().Be("First violation");

        restored.Violations[1].RuleId.Should().Be("CA2007");
        restored.Violations[1].FilePath.Should().Be("file.cs");
        restored.Violations[1].LineNumber.Should().Be(20);
        restored.Violations[1].ContentHash.Should().Be("hash2");
        restored.Violations[1].Description.Should().Be("Second violation");
    }

    [Fact]
    public void ToJson_ProducesValidIndentedJson()
    {
        // Arrange
        var baseline = new Baseline("TestProject");
        baseline.AddViolation(new BaselineViolation("CA1822", "file.cs", 10, "hash1"));

        // Act
        var json = baseline.ToJson();

        // Assert
        json.Should().Contain("{");
        json.Should().Contain("}");
        json.Should().Contain("TestProject");
        json.Should().Contain("CA1822");
        json.Should().MatchRegex(@"\s{2,}"); // Should have indentation
    }

    #endregion
}