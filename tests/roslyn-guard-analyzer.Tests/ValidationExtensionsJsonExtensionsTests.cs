#nullable enable

using System;
using RoslynGuardAnalyzer.Utilities;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public sealed class ValidationExtensionsJsonExtensionsTests
{
    // Helper to obtain a valid instance of the marker type.
    private static object GetValidMarkerInstance()
    {
        // The project defines a type named ValidationExtensions.
        // It is expected to be a non‑static class that can be instantiated.
        // If the type is static, this method will need to be adjusted accordingly.
        return Activator.CreateInstance(typeof(ValidationExtensions))!;
    }

    [Fact]
    public void ToJson_ReturnsExpectedJson_WhenCalledWithDefaultIndentation()
    {
        // Arrange
        var marker = GetValidMarkerInstance();

        // Act
        string json = marker.ToJson();

        // Assert
        // The JSON should contain the camel‑cased property name "type"
        // and the value "ValidationExtensions".
        Assert.Equal("{\"type\":\"ValidationExtensions\"}", json);
    }

    [Fact]
    public void ToJson_ReturnsIndentedJson_WhenIndentionRequested()
    {
        // Arrange
        var marker = GetValidMarkerInstance();

        // Act
        string json = marker.ToJson(indented: true);

        // Assert
        // Indented JSON contains line breaks; we verify that at least one newline exists.
        Assert.Contains(Environment.NewLine, json);
        // The content (ignoring whitespace) should still represent the same object.
        string compact = json.Replace(Environment.NewLine, string.Empty).Replace(" ", string.Empty);
        Assert.Equal("{\"type\":\"ValidationExtensions\"}", compact);
    }

    [Fact]
    public void ToJson_ThrowsArgumentNullException_WhenValueIsNull()
    {
        // Arrange
        object? nullValue = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullValue!.ToJson());
    }

    [Fact]
    public void ToJson_ThrowsArgumentException_WhenValueIsWrongType()
    {
        // Arrange
        var wrong = new object();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => wrong.ToJson());
        Assert.Contains(nameof(ValidationExtensions), ex.Message);
    }

    [Fact]
    public void FromJson_ReturnsTypeMarker_WithCorrectType()
    {
        // Arrange
        string json = "{\"type\":\"ValidationExtensions\"}";

        // Act
        var result = ValidationExtensionsJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ValidationExtensions", result!.Type);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void FromJson_ThrowsArgumentException_OnNullOrEmpty(string json)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ValidationExtensionsJsonExtensions.FromJson(json!));
    }

    [Fact]
    public void FromJson_ReturnsNull_OnInvalidJson()
    {
        // Arrange
        string invalidJson = "{\"invalid\":\"data\"}";

        // Act
        var result = ValidationExtensionsJsonExtensions.FromJson(invalidJson);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_ReturnsTrueAndOutputsValue_OnValidJson()
    {
        // Arrange
        string json = "{\"type\":\"ValidationExtensions\"}";

        // Act
        bool success = ValidationExtensionsJsonExtensions.TryFromJson(json, out var value);

        // Assert
        Assert.True(success);
        Assert.NotNull(value);
        Assert.Equal("ValidationExtensions", value!.Type);
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_OnInvalidJson()
    {
        // Arrange
        string json = "{\"type\":123}"; // type is not a string, deserialization fails

        // Act
        bool success = ValidationExtensionsJsonExtensions.TryFromJson(json, out var value);

        // Assert
        Assert.False(success);
        Assert.Null(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void TryFromJson_ThrowsArgumentException_OnNullOrEmpty(string json)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ValidationExtensionsJsonExtensions.TryFromJson(json!, out _));
    }
}
