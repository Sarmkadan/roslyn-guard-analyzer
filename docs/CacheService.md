# CacheService

`CacheService` is a generic caching utility that stores values with associated expiration timestamps. It provides methods for setting, retrieving, and removing cached entries, supports pattern-based invalidation, and includes a dedicated property for a single required cached value. The type is designed for use cases where both a primary cached item and a collection of keyed entries need to be managed together.

## API

### `public required T Value`

Gets or sets the primary cached value. This property is required and must be initialized upon object creation. The type `T` is the type parameter of the class.

### `public required DateTime ExpiresAt`

Gets or sets the expiration time for the primary cached value. This property is required and must be initialized.

### `public CacheService()`

Initializes a new instance of the `CacheService` class. The `Value` and `ExpiresAt` properties must be set after construction (e.g., using object initializer syntax).

### `public void Set<T>()`

Overloaded. Stores a value in the cache. The generic type parameter `T` specifies the type of the value being stored. Two overloads exist, accepting different parameter combinations (e.g., a key and a value, or a value without a key).  
**Throws:** `ArgumentNullException` if a required parameter is null.

### `public bool TryGet<T>()`

Attempts to retrieve a cached value of type `T`. Returns `true` if the value exists and has not expired; otherwise `false`. The retrieved value is provided via an output parameter (not shown in the signature).  
**Throws:** `InvalidCastException` if the stored value cannot be cast to `T`.

### `public T Get<T>()`

Retrieves a cached value of type `T`.  
**Returns:** The cached value.  
**Throws:** `KeyNotFoundException` if the entry does not exist or has expired.  
**Throws:** `InvalidCastException` if the stored value cannot be cast to `T`.

### `public T? GetOrDefault<T>()`

Retrieves a cached value of type `T`, or returns `default(T)` if the entry does not exist or has expired.  
**Throws:** `InvalidCastException` if the stored value cannot be cast to `T`.

### `public async Task<T> GetOrComputeAsync<T>()`

Asynchronously retrieves a cached value of type `T`. If the value does not exist or has expired, it computes the value using a provided factory delegate (not shown in the signature) and stores it before returning.  
**Throws:** `InvalidCastException` if the stored value cannot be cast to `T`.

### `public bool Remove()`

Removes a cached entry identified by a key (parameter not shown).  
**Returns:** `true` if the entry was found and removed; otherwise `false`.

### `public void Clear()`

Removes all cached entries, including the primary `Value` property.

### `public int RemoveExpired()`

Removes all entries whose expiration time has passed.  
**Returns:** The number of entries removed.

### `public bool Contains()`

Checks whether a cached entry exists for a given key (parameter not shown).  
**Returns:** `true` if the entry exists and has not expired; otherwise `false`.

### `public IEnumerable<string> GetKeys()`

Returns a collection of all keys currently stored in the cache (excluding the primary `Value`).

### `public int InvalidateByPattern()`

Removes all entries whose keys match a specified pattern (parameter not shown).  
**Returns:** The number of entries invalidated.

## Usage

### Example 1: Basic caching with expiration

```csharp
var cache = new CacheService<string>
{
    Value = "initial",
    ExpiresAt = DateTime.UtcNow.AddMinutes(5)
};

// Set a keyed entry
cache.Set("config", "production");

// Retrieve the entry
if (cache.TryGet<string>("config", out var config))
{
    Console.WriteLine(config); // "production"
}

// Remove expired entries
cache.RemoveExpired();
```

### Example 2: Asynchronous computation with fallback

```csharp
var cache = new CacheService<WeatherData>
{
    Value = await FetchWeatherAsync(),
    ExpiresAt = DateTime.UtcNow.AddHours(1)
};

// Use GetOrComputeAsync to lazily load data
var data = await cache.GetOrComputeAsync("current", async () =>
{
    return await FetchWeatherAsync();
}, TimeSpan.FromMinutes(30));

Console.WriteLine(data.Temperature);
```

## Notes

- The primary `Value` property is separate from the keyed entries managed by `Set`, `Get`, etc. Expiration for the primary value is controlled solely by `ExpiresAt`.
- Expired entries are not automatically removed; call `RemoveExpired` periodically to reclaim memory.
- Pattern-based invalidation (`InvalidateByPattern`) uses simple wildcard matching (e.g., `"user:*"`). The exact pattern syntax is implementation-defined.
- All generic methods perform type casting at runtime. An `InvalidCastException` is thrown if the stored value does not match the requested type.
- This class is **not thread-safe**. Concurrent access from multiple threads must be synchronized externally (e.g., using a lock or `ConcurrentDictionary` wrapper).
- The `required` modifier on `Value` and `ExpiresAt` enforces initialization at construction time. Failing to set these properties results in a compile-time error when using object initializers.
