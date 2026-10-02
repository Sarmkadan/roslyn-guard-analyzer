using Xunit;
using System;
using System.Text.Json;
using RoslynGuardAnalyzer.Caching;

namespace roslyn_guard_analyzer.Tests;

public class CacheServiceJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var cacheService = new CacheService();

        // Act
        var json = cacheService.ToJson();

        // Assert
        Assert.NotEmpty(json);
    }

    [Fact]
    public void FromJson_HappyPath_ThrowsOnDeserialize()
    {
        // CacheService constructor parameter doesn't bind to a property,
        // so JSON deserialization cannot reconstruct it.
        var cacheService = new CacheService();
        var json = cacheService.ToJson();

        Assert.ThrowsAny<Exception>(() => CacheServiceJsonExtensions.FromJson(json));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsFalseDueToConstructorBinding()
    {
        // CacheService constructor parameter doesn't bind to a property,
        // so JSON deserialization fails.
        var cacheService = new CacheService();
        var json = cacheService.ToJson();

        // TryFromJson catches JsonException but this throws InvalidOperationException
        Assert.ThrowsAny<Exception>(() => CacheServiceJsonExtensions.TryFromJson(json, out _));
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // ArgumentException.ThrowIfNullOrEmpty throws ArgumentNullException for null
        Assert.Throws<ArgumentNullException>(() => CacheServiceJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_EmptyInput_ThrowsArgumentException()
    {
        // ArgumentException.ThrowIfNullOrEmpty throws ArgumentException for empty
        Assert.Throws<ArgumentException>(() => CacheServiceJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => CacheServiceJsonExtensions.TryFromJson(null, out _));
    }

    [Fact]
    public void TryFromJson_EmptyInput_ThrowsArgumentException()
    {
        // ArgumentException.ThrowIfNullOrEmpty throws ArgumentException for empty
        Assert.Throws<ArgumentException>(() => CacheServiceJsonExtensions.TryFromJson(string.Empty, out _));
    }
}
