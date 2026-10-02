#nullable enable

using System;
using RoslynGuardAnalyzer.Utilities;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public sealed class ValidationExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_ThrowsArgumentNullException_WhenValueIsNull()
    {
        object? nullValue = null;
        Assert.Throws<ArgumentNullException>(() => nullValue!.ToJson());
    }

    [Fact]
    public void ToJson_ThrowsArgumentException_WhenValueIsWrongType()
    {
        var wrong = new object();
        var ex = Assert.Throws<ArgumentException>(() => wrong.ToJson());
        Assert.Contains(nameof(ValidationExtensions), ex.Message);
    }

    [Fact]
    public void FromJson_ReturnsTypeMarker_WithCorrectType()
    {
        string json = "{\"type\":\"ValidationExtensions\"}";
        var result = ValidationExtensionsJsonExtensions.FromJson(json);
        Assert.NotNull(result);
        Assert.Equal("ValidationExtensions", result!.Type);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ValidationExtensionsJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_EmptyInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ValidationExtensionsJsonExtensions.FromJson(""));
    }

    [Fact]
    public void FromJson_InvalidJson_ReturnsTypeMarkerWithNullType()
    {
        // Deserializes successfully but Type property is null
        string invalidJson = "{\"invalid\":\"data\"}";
        var result = ValidationExtensionsJsonExtensions.FromJson(invalidJson);
        Assert.NotNull(result);
        Assert.Null(result!.Type);
    }

    [Fact]
    public void TryFromJson_ReturnsTrueAndOutputsValue_OnValidJson()
    {
        string json = "{\"type\":\"ValidationExtensions\"}";
        bool success = ValidationExtensionsJsonExtensions.TryFromJson(json, out var value);
        Assert.True(success);
        Assert.NotNull(value);
        Assert.Equal("ValidationExtensions", value!.Type);
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_OnInvalidJson()
    {
        string json = "{\"type\":123}";
        bool success = ValidationExtensionsJsonExtensions.TryFromJson(json, out var value);
        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ValidationExtensionsJsonExtensions.TryFromJson(null!, out _));
    }

    [Fact]
    public void TryFromJson_EmptyInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ValidationExtensionsJsonExtensions.TryFromJson("", out _));
    }
}
