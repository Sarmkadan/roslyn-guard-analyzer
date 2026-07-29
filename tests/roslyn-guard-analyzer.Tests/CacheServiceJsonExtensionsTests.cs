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
    public void FromJson_HappyPath_ReturnsCacheService()
    {
        // Arrange
        var cacheService = new CacheService();
        var json = cacheService.ToJson();

        // Act
        var result = CacheServiceJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndCacheService()
    {
        // Arrange
        var cacheService = new CacheService();
        var json = cacheService.ToJson();

        // Act
        var success = CacheServiceJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentException()
    {
        // Act and Assert
        Assert.Throws<ArgumentException>(() => CacheServiceJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_WhitespaceInput_ReturnsNull()
    {
        // Act
        var result = CacheServiceJsonExtensions.FromJson(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentException()
    {
        // Act and Assert
        Assert.Throws<ArgumentException>(() => CacheServiceJsonExtensions.TryFromJson(null, out _));
    }

    [Fact]
    public void TryFromJson_WhitespaceInput_ReturnsFalseAndNull()
    {
        // Act
        var success = CacheServiceJsonExtensions.TryFromJson(string.Empty, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }
}
