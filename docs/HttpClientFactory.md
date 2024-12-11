# HttpClientFactory

A managed wrapper around `HttpClient` that provides centralized configuration, automatic lifetime management, and built-in resilience patterns. It abstracts away the complexities of socket exhaustion and transient fault handling by internally pooling and recycling handlers, offering retry logic, and exposing simplified methods for common JSON operations.

## API

### `public HttpClientFactory`

Creates a new instance of the factory. The underlying handler pool and default request configuration are initialized according to the implementation’s settings. No external `HttpClient` instances should be created manually when using this factory.

### `public HttpClient CreateClient()`

Returns a ready-to-use `HttpClient` backed by the factory’s managed handler pipeline. The returned client must not be disposed by the caller—its lifetime is controlled by the factory. Use this when you need to perform custom request composition that falls outside the provided convenience methods.

**Returns:** A configured `HttpClient` instance.

### `public async Task<HttpResponseMessage> ExecuteWithRetryAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)`

Sends the given `HttpRequestMessage` through the managed pipeline with automatic retries on transient failures. The retry policy (count, backoff strategy, and which status codes or exceptions trigger a retry) is defined by the factory’s configuration.

**Parameters:**
- `request` — The fully composed `HttpRequestMessage` to send.
- `cancellationToken` — Optional token to cancel the operation and any pending retries.

**Returns:** The final `HttpResponseMessage` after all successful retries, or the last received response if retries are exhausted.

**Throws:**
- `HttpRequestException` — When all retry attempts are exhausted and the last attempt threw a transport-level exception.
- `TaskCanceledException` — When the operation is canceled via the token.
- `ArgumentNullException` — When `request` is `null`.

### `public async Task<string> GetJsonAsync(string requestUri, CancellationToken cancellationToken = default)`

Performs a GET request to the specified URI and deserializes the response body as a JSON string. Retry logic applies as configured.

**Parameters:**
- `requestUri` — The target URI or relative path.
- `cancellationToken` — Optional token to cancel the operation.

**Returns:** The raw JSON string from the response body.

**Throws:**
- `HttpRequestException` — On non-success status codes or transport failures after retries are exhausted.
- `TaskCanceledException` — When canceled.
- `ArgumentNullException` — When `requestUri` is `null`.

### `public async Task<string> PostJsonAsync(string requestUri, string jsonContent, CancellationToken cancellationToken = default)`

Performs a POST request to the specified URI with a JSON payload. The content is sent with the `application/json` media type. Retry logic applies as configured.

**Parameters:**
- `requestUri` — The target URI or relative path.
- `jsonContent` — The pre-serialized JSON string to send as the request body.
- `cancellationToken` — Optional token to cancel the operation.

**Returns:** The raw JSON string from the response body.

**Throws:**
- `HttpRequestException` — On non-success status codes or transport failures after retries are exhausted.
- `TaskCanceledException` — When canceled.
- `ArgumentNullException` — When `requestUri` or `jsonContent` is `null`.

### `public void ClearCache()`

Discards any internally cached state, such as reused handler pipelines or pooled connections, forcing fresh allocation on the next request. Useful when configuration changes at runtime or when connection stickiness must be broken.

### `public void Dispose()`

Releases all managed handler resources and disposes the underlying connection pool. After disposal, any further use of clients obtained from this factory will throw `ObjectDisposedException`. Call this once at application shutdown.

## Usage

### Example 1: Basic GET with automatic retry

```csharp
using var factory = new HttpClientFactory();

try
{
    string json = await factory.GetJsonAsync("https://api.example.com/data");
    Console.WriteLine(json);
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Request failed after retries: {ex.Message}");
}
```

### Example 2: Custom request with retry and cancellation

```csharp
using var factory = new HttpClientFactory();
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/submit")
{
    Content = new StringContent("{\"key\":\"value\"}", Encoding.UTF8, "application/json")
};

try
{
    HttpResponseMessage response = await factory.ExecuteWithRetryAsync(request, cts.Token);
    response.EnsureSuccessStatusCode();
    string body = await response.Content.ReadAsStringAsync();
    Console.WriteLine(body);
}
catch (TaskCanceledException)
{
    Console.WriteLine("Operation timed out.");
}
```

## Notes

- **Client lifetime:** Clients returned by `CreateClient()` are owned by the factory. Disposing them individually has no effect and may lead to unexpected behavior if the factory later reuses the underlying handler.
- **Thread safety:** All public methods are safe to call concurrently from multiple threads. The internal handler pool and retry state are synchronized appropriately.
- **Retry semantics:** `ExecuteWithRetryAsync`, `GetJsonAsync`, and `PostJsonAsync` apply the same retry policy. Non-success HTTP status codes (e.g., 429, 503) and specific transport exceptions trigger retries. Idempotency of the operation is the caller’s responsibility—POST requests are retried unconditionally, which may cause duplicate side effects on the server.
- **ClearCache behavior:** Calling `ClearCache()` while requests are in flight does not interrupt them; it only affects subsequent requests. Existing clients remain usable until the factory is disposed.
- **Disposal:** Once `Dispose()` is invoked, all outstanding clients become invalid. Ensure no pending asynchronous operations are using the factory at the point of disposal to avoid `ObjectDisposedException`.
