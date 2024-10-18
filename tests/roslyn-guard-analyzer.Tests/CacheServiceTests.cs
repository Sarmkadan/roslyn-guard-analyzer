// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using RoslynGuardAnalyzer.Caching;

namespace RoslynGuardAnalyzer.Tests;

public class CacheServiceTests
{
    [Fact]
    public void Set_NullKey_ThrowsArgumentException()
    {
        var cache = new CacheService();
        var act = () => cache.Set<string>(null!, "value");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Set_EmptyKey_ThrowsArgumentException()
    {
        var cache = new CacheService();
        var act = () => cache.Set("", "value");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Set_NullValue_ThrowsArgumentNullException()
    {
        var cache = new CacheService();
        var act = () => cache.Set<string>("key", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Count_EmptyCache_ReturnsZero()
    {
        var cache = new CacheService();
        cache.Count.Should().Be(0);
    }

    [Fact]
    public void Set_ValidKeyAndValue_IncrementsCount()
    {
        var cache = new CacheService();
        cache.Set("key1", "value1");
        cache.Count.Should().Be(1);
    }

    [Fact]
    public void Set_DuplicateKey_OverwritesExistingEntry()
    {
        var cache = new CacheService();
        cache.Set("key", "old");
        cache.Set("key", "new");
        cache.Count.Should().Be(1);
    }

    [Fact]
    public void Constructor_CustomExpiration_DoesNotThrow()
    {
        var act = () => new CacheService(TimeSpan.FromMinutes(5));
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_DefaultExpiration_DoesNotThrow()
    {
        var act = () => new CacheService();
        act.Should().NotThrow();
    }
}
