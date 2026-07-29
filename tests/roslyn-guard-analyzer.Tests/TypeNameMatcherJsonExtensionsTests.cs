using Xunit;
using System;
using System.Text.Json;
using RoslynGuardAnalyzer.Utilities;

namespace roslyn_guard_analyzer.Tests;

public class TypeNameMatcherJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var typeNameMatcher = new TypeNameMatcher("System.String");

        // Act
        var json = TypeNameMatcherJsonExtensions.ToJson(typeNameMatcher);

        // Assert
        Assert.NotNull(json);
        var jsonDoc = JsonDocument.Parse(json);
        Assert.True(jsonDoc.RootElement.TryGetProperty("pattern", out _));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsTypeNameMatcher()
    {
        // Arrange
        var json = "{\"pattern\":\"System.String\"}";

        // Act
        var typeNameMatcher = TypeNameMatcherJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(typeNameMatcher);
        Assert.Equal("System.String", typeNameMatcher.Pattern);
    }

    [Fact]
    public void FromJson_NullInput_ReturnsNull()
    {
        // Act
        var typeNameMatcher = TypeNameMatcherJsonExtensions.FromJson(null);

        // Assert
        Assert.Null(typeNameMatcher);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndTypeNameMatcher()
    {
        // Arrange
        var json = "{\"pattern\":\"System.String\"}";

        // Act
        var success = TypeNameMatcherJsonExtensions.TryFromJson(json, out var typeNameMatcher);

        // Assert
        Assert.True(success);
        Assert.NotNull(typeNameMatcher);
        Assert.Equal("System.String", typeNameMatcher.Pattern);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalseAndNull()
    {
        // Act
        var success = TypeNameMatcherJsonExtensions.TryFromJson(null, out var typeNameMatcher);

        // Assert
        Assert.False(success);
        Assert.Null(typeNameMatcher);
    }
}
