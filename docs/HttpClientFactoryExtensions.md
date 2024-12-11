# HttpClientFactoryExtensions
The `HttpClientFactoryExtensions` class provides a set of extension methods for `IHttpClientFactory` that enable robust and fault-tolerant HTTP requests. These methods offer built-in retry mechanisms and support for JSON data exchange, making it easier to handle common web service interactions.

## API
* `GetWithRetryAsync`: Sends a GET request to the specified URI with retry logic. Parameters: none (extension method). Returns: `HttpResponseMessage`. Throws: exceptions related to HTTP requests, such as `HttpRequestException`.
* `PostWithRetryAsync`: Sends a POST request to the specified URI with retry logic. Parameters: none (extension method). Returns: `HttpResponseMessage`. Throws: exceptions related to HTTP requests, such as `HttpRequestException`.
* `GetJsonAsync`: Sends a GET request to the specified URI and deserializes the response as JSON. Parameters: none (extension method). Returns: `string` containing the JSON response. Throws: exceptions related to HTTP requests or JSON deserialization, such as `HttpRequestException` or `JsonException`.
* `PostJsonAsync`: Sends a POST request to the specified URI with a JSON payload. Parameters: none (extension method). Returns: `string` containing the JSON response. Throws: exceptions related to HTTP requests or JSON serialization, such as `HttpRequestException` or `JsonException`.

## Usage
```csharp
// Example 1: Using GetWithRetryAsync to fetch data
var httpClientFactory = new HttpClientFactory();
var client = httpClientFactory.CreateClient();
var response = await client.GetWithRetryAsync("https://example.com/api/data");
if (response.IsSuccessStatusCode)
{
    var jsonData = await response.Content.ReadAsStringAsync();
    Console.WriteLine(jsonData);
}

// Example 2: Using PostJsonAsync to send data
var httpClientFactory = new HttpClientFactory();
var client = httpClientFactory.CreateClient();
var jsonData = "{\"name\":\"John\",\"age\":30}";
var response = await client.PostJsonAsync("https://example.com/api/create", jsonData);
if (response.IsSuccessStatusCode)
{
    Console.WriteLine("Data sent successfully");
}
```

## Notes
When using these extension methods, consider the following edge cases:
* If the retry mechanism is triggered, the methods will throw an exception after the maximum number of retries is exceeded.
* The `GetJsonAsync` and `PostJsonAsync` methods assume that the response or request body is valid JSON. If the JSON is malformed, a `JsonException` will be thrown.
* These methods are thread-safe, as they rely on the `IHttpClientFactory` instance, which is designed to be thread-safe. However, the underlying `HttpClient` instances created by the factory may not be thread-safe, so it's essential to use them correctly to avoid issues like socket exhaustion.
