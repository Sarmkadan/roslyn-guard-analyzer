#nullable enable
// =============================================================================
// Tests for ViolationReportJsonExtensions
// =============================================================================

using System;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class ViolationReportJsonExtensionsTests
{
    [Fact]
    public void ToJson_ReturnsNonEmptyJson()
    {
        // Arrange
        var report = new ViolationReport(); // default instance – properties are optional for serialization

        // Act
        string json = report.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        // The JSON should be deserializable back to a ViolationReport instance
        var deserialized = ViolationReportJsonExtensions.FromJson(json);
        Assert.NotNull(deserialized);
    }

    [Fact]
    public void ToJson_WithIndentation_ContainsNewLine()
    {
        // Arrange
        var report = new ViolationReport();

        // Act
        string json = report.ToJson(indented: true);

        // Assert
        // Indented JSON should contain line‑break characters
        Assert.Contains('\n', json);
    }

    [Fact]
    public void FromJson_NullOrEmpty_ThrowsArgumentException()
    {
        // Null
        Assert.Throws<ArgumentException>(() => ViolationReportJsonExtensions.FromJson(null!));

        // Empty string
        Assert.Throws<ArgumentException>(() => ViolationReportJsonExtensions.FromJson(string.Empty));

        // Whitespace only
        Assert.Throws<ArgumentException>(() => ViolationReportJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        const string invalidJson = "{ this is not valid json }";

        // Act
        bool result = ViolationReportJsonExtensions.TryFromJson(invalidJson, out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndObject()
    {
        // Arrange
        var originalReport = new ViolationReport();
        string json = originalReport.ToJson();

        // Act
        bool result = ViolationReportJsonExtensions.TryFromJson(json, out var deserialized);

        // Assert
        Assert.True(result);
        Assert.NotNull(deserialized);
    }
}
