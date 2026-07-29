using System;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Caching;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public sealed class CacheServiceTests
{
    private const string TestKey = "test-key";

    [Fact]
    public void Set_And_Get_ReturnsStoredValue()
    {
        var cache = new CacheService();
        var expected = "hello";

        cache.Set(TestKey, expected);
        var actual = cache.Get<string>(TestKey);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TryGet_ReturnsTrueAndValue_WhenEntryExists()
    {
        var cache = new CacheService();
        var expected = 42;

        cache.Set(TestKey, expected);
        var success = cache.TryGet<int>(TestKey, out var actual);

        Assert.True(success);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetOrDefault_ReturnsDefault_WhenKeyMissing()
    {
        var cache = new CacheService();

        var defaultValue = "fallback";
        var result = cache.GetOrDefault<string>("non‑existent", defaultValue);

        Assert.Equal(defaultValue, result);
    }

    [Fact]
    public async Task GetOrComputeAsync_ComputesAndCachesResult()
    {
        var cache = new CacheService();
        var computeCalls = 0;

        async Task<int> Compute()
        {
            computeCalls++;
            await Task.Delay(10);
            return 7;
        }

        // First call should compute
        var first = await cache.GetOrComputeAsync("compute-key", Compute);
        // Second call should hit cache, not recompute
        var second = await cache.GetOrComputeAsync("compute-key", Compute);

        Assert.Equal(7, first);
        Assert.Equal(7, second);
        Assert.Equal(1, computeCalls); // only computed once
    }

    [Fact]
    public void Remove_ReturnsTrue_WhenKeyExists()
    {
        var cache = new CacheService();
        cache.Set(TestKey, "value");

        var removed = cache.Remove(TestKey);
        var stillExists = cache.Contains(TestKey);

        Assert.True(removed);
        Assert.False(stillExists);
    }

    [Fact]
    public void Set_ThrowsArgumentException_WhenKeyIsNullOrWhiteSpace()
    {
        var cache = new CacheService();

        Assert.Throws<ArgumentException>(() => cache.Set<string>(null!, "value"));
        Assert.Throws<ArgumentException>(() => cache.Set<string>("   ", "value"));
    }

    [Fact]
    public void Set_ThrowsArgumentNullException_WhenValueIsNull()
    {
        var cache = new CacheService();

        Assert.Throws<ArgumentNullException>(() => cache.Set<string>(TestKey, null!));
    }

    [Fact]
    public void Entry_Expires_AfterCustomExpiration()
    {
        var cache = new CacheService();
        cache.Set(TestKey, "temp", TimeSpan.FromMilliseconds(50));

        // Immediately available
        Assert.True(cache.TryGet<string>(TestKey, out var before));
        Assert.Equal("temp", before);

        // Wait for expiration
        Task.Delay(100).Wait();

        // Should be expired and removed
        Assert.False(cache.TryGet<string>(TestKey, out _));
        Assert.False(cache.Contains(TestKey));
    }
}
