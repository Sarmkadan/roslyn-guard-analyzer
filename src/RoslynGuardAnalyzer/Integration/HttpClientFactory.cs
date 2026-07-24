#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Integration;

/// <summary>
/// Factory for creating and configuring HTTP clients for external integrations.
/// Manages client lifecycle, timeouts, retry policies, and a circuit‑breaker.
/// </summary>
public sealed class HttpClientFactory : IDisposable
{
    private readonly Dictionary<string, HttpClient> _clientCache = new();
    private readonly HttpClientFactoryOptions _options;
    private readonly Random _random = new();

    // Circuit‑breaker state
    private int _failureCount;
    private DateTime _circuitOpenUntil = DateTime.MinValue;

    /// <summary>
    /// Initializes a new instance of <see cref="HttpClientFactory"/>.
    /// </summary>
    /// <param name="options">
    /// Optional configuration. If <c>null</c>, default options are used.
    /// </param>
    public HttpClientFactory(HttpClientFactoryOptions? options = null)
    {
        _options = options ?? new HttpClientFactoryOptions();
    }

    /// <summary>
    /// Creates or returns a cached <see cref="HttpClient"/> for a specific endpoint.
    /// </summary>
    /// <param name="baseUrl">The base URL for the client.</param>
    /// <param name="clientName">
    /// Optional name used as the cache key. If omitted, <paramref name="baseUrl"/> is used.
    /// </param>
    /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
    /// <exception cref="ArgumentException">When <paramref name="baseUrl"/> is null or empty.</exception>
    public HttpClient CreateClient(string baseUrl, string? clientName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseUrl);

        var key = clientName ?? baseUrl;

        lock (_clientCache)
        {
            if (_clientCache.TryGetValue(key, out var existingClient))
                return existingClient;

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = _options.DefaultTimeout
            };

            // Set default headers
            client.DefaultRequestHeaders.Add("User-Agent", "RoslynGuardAnalyzer/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            _clientCache[key] = client;
            return client;
        }
    }

    /// <summary>
    /// Executes an HTTP request with automatic retry, exponential back‑off with jitter,
    /// and a circuit‑breaker that blocks calls after a configurable number of failures.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> to use.</param>
    /// <param name="request">
    /// A delegate that performs the request and returns a <see cref="HttpResponseMessage"/>.
    /// </param>
    /// <returns>The final <see cref="HttpResponseMessage"/>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="client"/> or <paramref name="request"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">When the circuit breaker is open.</exception>
    /// <exception cref="HttpRequestException">
    /// When the request fails after all retry attempts.
    /// </exception>
    public async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        HttpClient client,
        Func<HttpClient, Task<HttpResponseMessage>> request)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(request);

        // Circuit‑breaker check
        if (DateTime.UtcNow < _circuitOpenUntil)
            throw new InvalidOperationException("Circuit breaker is open; requests are temporarily blocked.");

        HttpResponseMessage? lastResponse = null;
        Exception? lastException = null;

        for (int attempt = 0; attempt <= _options.MaxRetries; attempt++)
        {
            try
            {
                lastResponse = await request(client).ConfigureAwait(false);

                // Success – reset failure count
                _failureCount = 0;

                // Retry on transient server errors (5xx) and specific status codes
                if (IsTransientFailure(lastResponse.StatusCode) && attempt < _options.MaxRetries)
                {
                    await DelayWithJitterAsync(attempt).ConfigureAwait(false);
                    continue;
                }

                return lastResponse;
            }
            catch (Exception ex) when (IsTransientException(ex) && attempt < _options.MaxRetries)
            {
                lastException = ex;
                await DelayWithJitterAsync(attempt).ConfigureAwait(false);
                // fall through to retry loop
            }

            // Increment failure count for this attempt
            _failureCount++;

            if (_failureCount >= _options.CircuitBreakerFailureThreshold)
            {
                _circuitOpenUntil = DateTime.UtcNow + _options.CircuitBreakerOpenDuration;
                break;
            }
        }

        // If we exit the loop without returning, propagate the last error
        if (lastException is not null)
            throw new HttpRequestException("Request failed after retries.", lastException);

        return lastResponse ?? throw new HttpRequestException("Request failed after retries with no response.");
    }

    /// <summary>
    /// Retrieves a JSON payload from the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> to use.</param>
    /// <param name="path">The relative path to request.</param>
    /// <returns>The response body as a string.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="client"/> or <paramref name="path"/> is <c>null</c>.</exception>
    public async Task<string> GetJsonAsync(HttpClient client, string path)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(path);

        var response = await ExecuteWithRetryAsync(client, c => c.GetAsync(path)).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a JSON payload to the specified <paramref name="path"/> and returns the response body.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> to use.</param>
    /// <param name="path">The relative path to post to.</param>
    /// <param name="jsonContent">The JSON payload.</param>
    /// <returns>The response body as a string.</returns>
    /// <exception cref="ArgumentNullException">
    /// When <paramref name="client"/>, <paramref name="path"/>, or <paramref name="jsonContent"/> is <c>null</c>.
    /// </exception>
    public async Task<string> PostJsonAsync(HttpClient client, string path, string jsonContent)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(jsonContent);

        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var response = await ExecuteWithRetryAsync(client, c => c.PostAsync(path, content)).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Clears the client cache and disposes all cached <see cref="HttpClient"/> instances.
    /// </summary>
    public void ClearCache()
    {
        lock (_clientCache)
        {
            foreach (var client in _clientCache.Values)
                client?.Dispose();

            _clientCache.Clear();
        }
    }

    /// <summary>
    /// Disposes the factory, clearing the client cache.
    /// </summary>
    public void Dispose() => ClearCache();

    // -----------------------------------------------------------------
    // Helper methods
    // -----------------------------------------------------------------

    private static bool IsTransientFailure(HttpStatusCode statusCode) =>
        statusCode switch
        {
            >= HttpStatusCode.InternalServerError => true,
            HttpStatusCode.RequestTimeout => true,
            (HttpStatusCode)429 => true, // Too Many Requests
            _ => false
        };

    private static bool IsTransientException(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException;

    private async Task DelayWithJitterAsync(int attempt)
    {
        // Exponential back‑off with jitter (0‑100 ms)
        var jitter = _random.NextDouble() * 100;
        var delayMs = Math.Pow(2, attempt) * 100 + jitter;
        await Task.Delay(TimeSpan.FromMilliseconds(delayMs)).ConfigureAwait(false);
    }
}
