# Roslyn Guard Analyzer

...

## HttpClientFactory

The `HttpClientFactory` class is a utility for creating and managing HTTP clients with built-in retry logic and caching. It allows for the creation of clients with specific timeouts and retry policies, and provides methods for making GET and POST requests with automatic retries.

### Usage Example
```csharp
var factory = new HttpClientFactory(defaultTimeout: TimeSpan.FromSeconds(30), maxRetries: 3);
var client = factory.CreateClient("https://api.example.com");

var json = await factory.GetJsonAsync(client, "/users");
Console.WriteLine(json);

var response = await factory.PostJsonAsync(client, "/users", "{\"name\":\"John\"}");
Console.WriteLine(response);

// Clear cache
factory.ClearCache();

// Dispose factory
factory.Dispose();
```

...
