// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Caching;

/// <summary>
/// In-memory caching service for analysis results and derived data.
/// Supports expiration policies and cache invalidation.
/// </summary>
public sealed class CacheService
{
    private class CacheEntry<T>
    {
        public required T Value { get; init; }
        public required DateTime ExpiresAt { get; init; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }

    private readonly Dictionary<string, object> _cache = [];
    private readonly TimeSpan _defaultExpiration;

    public CacheService(TimeSpan? defaultExpiration = null)
    {
        _defaultExpiration = defaultExpiration ?? TimeSpan.FromHours(1);
    }

    /// <summary>
    /// Gets the number of items currently in the cache.
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Sets a value in the cache with the default expiration time.
    /// </summary>
    public void Set<T>(string key, T value)
    {
        Set(key, value, _defaultExpiration);
    }

    /// <summary>
    /// Sets a value in the cache with a custom expiration time.
    /// </summary>
    public void Set<T>(string key, T value, TimeSpan expiration)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var entry = new CacheEntry<T>
        {
            Value = value,
            ExpiresAt = DateTime.UtcNow.Add(expiration)
        };

        _cache[key] = entry;
    }

    /// <summary>
    /// Tries to get a value from the cache, returning false if not found or expired.
    /// </summary>
    public bool TryGet<T>(string key, out T? value)
    {
        value = default;

        if (string.IsNullOrWhiteSpace(key) || !_cache.TryGetValue(key, out var entry))
            return false;

        if (entry is CacheEntry<T> typedEntry)
        {
            if (typedEntry.IsExpired)
            {
                _cache.Remove(key);
                return false;
            }

            value = typedEntry.Value;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets a value from the cache, throwing an exception if not found or expired.
    /// </summary>
    public T Get<T>(string key)
    {
        if (!TryGet(key, out T? value))
            throw new KeyNotFoundException($"Cache key '{key}' not found or expired");

        return value!;
    }

    /// <summary>
    /// Gets a value from the cache, or returns a default value if not found.
    /// </summary>
    public T? GetOrDefault<T>(string key, T? defaultValue = default)
    {
        return TryGet(key, out T? value) ? value : defaultValue;
    }

    /// <summary>
    /// Gets a value from the cache, computing and caching it if not found.
    /// </summary>
    public async Task<T> GetOrComputeAsync<T>(string key, Func<Task<T>> computeAsync)
    {
        if (TryGet(key, out T? value))
            return value!;

        var computed = await computeAsync();
        Set(key, computed);

        return computed;
    }

    /// <summary>
    /// Removes a specific key from the cache.
    /// </summary>
    public bool Remove(string key)
    {
        return _cache.Remove(key);
    }

    /// <summary>
    /// Clears all items from the cache.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Removes all expired items from the cache.
    /// </summary>
    public int RemoveExpired()
    {
        var expiredKeys = _cache
            .Where(kvp => kvp.Value is System.Reflection.IReflect && IsExpired(kvp.Value))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
            _cache.Remove(key);

        return expiredKeys.Count;
    }

    /// <summary>
    /// Checks if a specific key exists in the cache.
    /// </summary>
    public bool Contains(string key)
    {
        return TryGet<object>(key, out _);
    }

    /// <summary>
    /// Gets all keys in the cache that haven't expired.
    /// </summary>
    public IEnumerable<string> GetKeys()
    {
        RemoveExpired();
        return _cache.Keys.ToList();
    }

    /// <summary>
    /// Invalidates the cache for all keys matching a pattern (prefix match).
    /// </summary>
    public int InvalidateByPattern(string pattern)
    {
        var keysToRemove = _cache.Keys
            .Where(k => k.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
            _cache.Remove(key);

        return keysToRemove.Count;
    }

    /// <summary>
    /// Checks if a cache entry is expired (used internally).
    /// </summary>
    private static bool IsExpired(object entry)
    {
        var type = entry.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition().Name.Contains("CacheEntry"))
        {
            var expiresAtProperty = type.GetProperty("ExpiresAt");
            if (expiresAtProperty != null)
            {
                var expiresAt = (DateTime)expiresAtProperty.GetValue(entry)!;
                return DateTime.UtcNow > expiresAt;
            }
        }

        return false;
    }
}
