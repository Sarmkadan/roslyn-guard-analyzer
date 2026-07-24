#nullable enable

using System;
using System.Collections.Generic;
using FluentAssertions;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="BaselineViolation"/> equality and hashcode behavior.
/// Tests edge cases for IEquatable implementation and hash-based collections.
/// </summary>
public class BaselineViolationTests
{
    #region Equality with null

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var violation = new BaselineViolation("CA1822", "src/Program.cs", 42, "hash123");

        // Act
        var result = violation.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_ObjectOverload_WithNull_ReturnsFalse()
    {
        // Arrange
        var violation = new BaselineViolation("CA1822", "src/Program.cs", 42, "hash123");
        object? nullObj = null;

        // Act
        var result = violation.Equals(nullObj);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Case sensitivity in file paths

    [Fact]
    public void Equals_WithDifferentCaseFilePaths_UsesCaseInsensitiveComparison()
    {
        // Arrange
        var violation1 = new BaselineViolation("CA1822", "src/Program.cs", 42, "hash123");
        var violation2 = new BaselineViolation("CA1822", "SRC/PROGRAM.CS", 42, "hash123");

        // Act
        var result = violation1.Equals(violation2);

        // Assert
        // PathNormalizer uses CaseInsensitive mode by default, so paths should be considered equal
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCaseFilePaths_WithBackslashes_UsesCaseInsensitiveComparison()
    {
        // Arrange
        var violation1 = new BaselineViolation("CA1822", "src\\Program.cs", 42, "hash123");
        var violation2 = new BaselineViolation("CA1822", "SRC\\PROGRAM.CS", 42, "hash123");

        // Act
        var result = violation1.Equals(violation2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithMixedSlashes_DifferentCase_UsesCaseInsensitiveComparison()
    {
        // Arrange
        var violation1 = new BaselineViolation("CA1822", "src/Program.cs", 42, "hash123");
        var violation2 = new BaselineViolation("CA1822", "SRC\\PROGRAM.CS", 42, "hash123");

        // Act
        var result = violation1.Equals(violation2);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region HashCode consistency


    [Fact]
    public void GetHashCode_WhenTwoInstancesAreEqual_ReturnsSameHashCode()
    {
        // Arrange
        var violation1 = new BaselineViolation("CA1822", "src/Program.cs", 42, "hash123");
        var violation2 = new BaselineViolation("CA1822", "src/Program.cs", 42, "hash123");

        // Act
        var hash1 = violation1.GetHashCode();
        var hash2 = violation2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_WhenTwoInstancesAreEqual_WithDifferentCasePaths_ReturnsSameHashCode()
    {
        // Arrange
        var violation1 = new BaselineViolation("CA1822", "src/Program.cs", 42, "hash123");
        var violation2 = new BaselineViolation("CA1822", "SRC/PROGRAM.CS", 42, "hash123");

        // Act
        var hash1 = violation1.GetHashCode();
        var hash2 = violation2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_WhenTwoInstancesAreNotEqual_ReturnsDifferentHashCodes()
    {
        // Arrange
        var violation1 = new BaselineViolation("CA1822", "src/Program.cs", 42, "hash123");
        var violation2 = new BaselineViolation("CA1051", "src/Program.cs", 42, "hash456");

        // Act
        var hash1 = violation1.GetHashCode();
        var hash2 = violation2.GetHashCode();

        // Assert
        hash1.Should().NotBe(hash2);
    }

    #endregion

    #region HashSet and Dictionary usage

    [Fact]
    public void HashSet_Add_WithEqualViolations_DetectsDuplicate()
    {
        // Arrange
        var violation1 = new BaselineViolation("CA1822", "src/Program.cs", 42, "hash123");
        var violation2 = new BaselineViolation("CA1822", "src/Program.cs", 42, "hash123");

        // Note: These will have different IDs since IDs are generated in constructor
        // But with the fixed Equals implementation, they should be considered equal
        // because they have the same RuleId, FilePath, and ContentHash

        var set = new HashSet<BaselineViolation>();

        // Act
        var added1 = set.Add(violation1);
        var added2 = set.Add(violation2);

        // Assert
        added1.Should().BeTrue("First violation should be added");
        added2.Should().BeFalse("Second violation with same content should NOT be added (duplicate detection)");
        set.Should().HaveCount(1, "Equal violations should be detected as duplicates in HashSet");
    }

    [Fact]
    public void Dictionary_KeyLookup_WithEqualViolations_WorksCorrectly()
    {
        // Arrange
        var violation1 = new BaselineViolation("CA1822", "src/Program.cs", 42, "hash123");
        var violation2 = new BaselineViolation("CA1822", "src/Program.cs", 42, "hash123");

        var dict = new Dictionary<BaselineViolation, string>();
        dict[violation1] = "First";

        // Act
        var found = dict.TryGetValue(violation2, out var value);

        // Assert
        found.Should().BeTrue("Equal violations should be found in Dictionary");
        value.Should().Be("First", "Should retrieve the correct value for the equal key");
    }

    #endregion

    #region Serialization round-trip

    [Fact]
    public void SerializationRoundTrip_PreservesEqualitySemantics()
    {
        // Arrange
        var original = new BaselineViolation("CA1822", "src/Program.cs", 42, "hash123");

        // Serialize to JSON
        var json = System.Text.Json.JsonSerializer.Serialize(original);

        // Deserialize back
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<BaselineViolation>(json);

        // Act
        var areEqual = original.Equals(deserialized);
        var hash1 = original.GetHashCode();
        var hash2 = deserialized?.GetHashCode() ?? 0;

        // Assert
        areEqual.Should().BeTrue("Deserialized object should be equal to original");
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void SerializationRoundTrip_WithDifferentCasePaths_PreservesEqualitySemantics()
    {
        // Arrange
        var original = new BaselineViolation("CA1822", "src/Program.cs", 42, "hash123");
        var json = System.Text.Json.JsonSerializer.Serialize(original);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<BaselineViolation>(json);

        // Create a violation with different case path
        var differentCase = new BaselineViolation("CA1822", "SRC/PROGRAM.CS", 42, "hash123");

        // Act
        var areEqual = original.Equals(differentCase);
        var hash1 = original.GetHashCode();
        var hash2 = differentCase.GetHashCode();

        // Assert
        areEqual.Should().BeTrue("Different case paths should be equal");
        hash1.Should().Be(hash2);
    }

    #endregion

    #region Matches method behavior (for comparison with Equals)

    [Fact]
    public void Matches_Method_ComparesRelevantProperties()
    {
        // Arrange
        var baseline = new BaselineViolation("CA1822", "src/Program.cs", 42, "abc123");
        var ruleViolation = new RuleViolation("CA1822", "Test Rule", "Test message", "src/Program.cs")
        {
            LineNumber = 42,
            CodeSnippet = "var x = 1;"
        };

        // Act
        var result = baseline.Matches(ruleViolation);

        // Assert
        result.Should().BeTrue("Should match on rule ID, file path, and line number");
    }

    [Fact]
    public void Matches_Method_WithDifferentCasePaths_ReturnsTrue()
    {
        // Arrange
        var baseline = new BaselineViolation("CA1822", "src/Program.cs", 42, "abc123");
        var ruleViolation = new RuleViolation("CA1822", "Test Rule", "Test message", "SRC/PROGRAM.CS")  // Different case
        {
            LineNumber = 42,
            CodeSnippet = "var x = 1;"
        };

        // Act
        var result = baseline.Matches(ruleViolation);

        // Assert
        result.Should().BeTrue("PathNormalizer should handle case-insensitive comparison");
    }

    #endregion

    #region Edge cases with empty/null values

    [Fact]
    public void Equals_WithEmptyStrings_HandlesCorrectly()
    {
        // Arrange
        var violation1 = new BaselineViolation("CA1822", "", 42, "hash123");
        var violation2 = new BaselineViolation("CA1822", "", 42, "hash123");

        // Act
        var result = violation1.Equals(violation2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithEmptyStrings_ReturnsConsistentHash()
    {
        // Arrange
        var violation1 = new BaselineViolation("CA1822", "", 42, "hash123");
        var violation2 = new BaselineViolation("CA1822", "", 42, "hash123");

        // Act
        var hash1 = violation1.GetHashCode();
        var hash2 = violation2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    #endregion
}