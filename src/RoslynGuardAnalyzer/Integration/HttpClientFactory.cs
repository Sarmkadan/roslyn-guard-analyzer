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
/// Manages client lifecycle, timeouts, and retry policies.
/// </summary>
public sealed class HttpClientFactory
{
    private readonly Dictionary<string, HttpClient> _clientCache = [];
    private readonly TimeSpan _defaultTimeout;
    private readonly int _maxRetries;

    public HttpClientFactory(TimeSpan? defaultTimeout = null, int maxRetries = 3)
    {
        _defaultTimeout = defaultTimeout ?? TimeSpan.FromSeconds(30);
        _maxRetries = Math.Max(0, maxRetries);
    }

    /// <summary>
    /// Creates or returns a cached HTTP client for a specific endpoint.
    /// </summary>
    public HttpClient CreateClient(string baseUrl, string? clientName = null)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));

        var key = clientName ?? baseUrl;

        if (_clientCache.TryGetValue(key, out var existingClient))
            return existingClient;

        var client = new HttpClient()
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = _defaultTimeout
        };

        // Set default headers
        client.DefaultRequestHeaders.Add("User-Agent", "RoslynGuardAnalyzer/1.0");
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _clientCache[key] = client;
        return client;
    }

    /// <summary>
    /// Executes an HTTP request with automatic retry logic.
    /// </summary>
    public async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        HttpClient client,
        Func<HttpClient, Task<HttpResponseMessage>> request)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));

        if (request is null)
            throw new ArgumentNullException(nameof(request));

        HttpResponseMessage? lastResponse = null;
        Exception? lastException = null;

        for (int attempt = 0; attempt <= _maxRetries; attempt++)
        {
            try
            {
                lastResponse = await request(client);

                // Retry on server errors (5xx)
                if ((int)lastResponse.StatusCode >= 500 && attempt < _maxRetries)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100));
                    continue;
                }

                return lastResponse;
            }
            catch (HttpRequestException ex) when (attempt < _maxRetries)
            {
                lastException = ex;
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100));
            }
            catch (TaskCanceledException ex) when (attempt < _maxRetries)
            {
                lastException = ex;
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100));
            }
        }

        if (lastException is not null)
            throw lastException;

        return lastResponse ?? throw new HttpRequestException("Request failed after all retries");
    }

    /// <summary>
    /// Gets a JSON response as a string.
    /// </summary>
    public async Task<string> GetJsonAsync(HttpClient client, string path)
    {
        var response = await ExecuteWithRetryAsync(client, async c =>
            await c.GetAsync(path));

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Posts a JSON payload and gets the response.
    /// </summary>
    public async Task<string> PostJsonAsync(HttpClient client, string path, string jsonContent)
    {
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await ExecuteWithRetryAsync(client, async c =>
            await c.PostAsync(path, content));

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Clears the client cache.
    /// </summary>
    public void ClearCache()
    {
        foreach (var client in _clientCache.Values)
        {
            client?.Dispose();
        }

        _clientCache.Clear();
    }

    /// <summary>
    /// Disposes all cached clients.
    /// </summary>
    public void Dispose()
    {
        ClearCache();
    }
}
