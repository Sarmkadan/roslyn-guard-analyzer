#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Integration;

/// <summary>
/// Extension methods for <see cref="HttpClientFactory"/> to provide fluent API for common HTTP operations.
/// </summary>
public static class HttpClientFactoryExtensions
{
    /// <summary>
    /// Creates a client and immediately executes a GET request with automatic retry.
    /// </summary>
    public static async Task<HttpResponseMessage> GetWithRetryAsync(
        this HttpClientFactory factory,
        string baseUrl,
        string path,
        string? clientName = null)
    {
        var client = factory.CreateClient(baseUrl, clientName);
        return await factory.ExecuteWithRetryAsync(client, async c => await c.GetAsync(path));
    }

    /// <summary>
    /// Creates a client and immediately executes a POST request with automatic retry.
    /// </summary>
    public static async Task<HttpResponseMessage> PostWithRetryAsync(
        this HttpClientFactory factory,
        string baseUrl,
        string path,
        string jsonContent,
        string? clientName = null)
    {
        var client = factory.CreateClient(baseUrl, clientName);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        return await factory.ExecuteWithRetryAsync(client, async c => await c.PostAsync(path, content));
    }

    /// <summary>
    /// Creates a client and fetches JSON content with automatic retry and error handling.
    /// </summary>
    public static async Task<string> GetJsonAsync(
        this HttpClientFactory factory,
        string baseUrl,
        string path,
        string? clientName = null)
    {
        var client = factory.CreateClient(baseUrl, clientName);
        return await factory.GetJsonAsync(client, path);
    }

    /// <summary>
    /// Creates a client and posts JSON content with automatic retry and error handling.
    /// </summary>
    public static async Task<string> PostJsonAsync(
        this HttpClientFactory factory,
        string baseUrl,
        string path,
        string jsonContent,
        string? clientName = null)
    {
        var client = factory.CreateClient(baseUrl, clientName);
        return await factory.PostJsonAsync(client, path, jsonContent);
    }
}