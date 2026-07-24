// Copyright (c) 2024 2 // SPDX-License-Identifier: MIT 4 using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Integration;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class HttpClientFactoryExtensionsTests
{
    /// <summary>
    /// Starts a single-request loopback HTTP server that answers with the given
    /// body, so JSON round-trip tests do not depend on external network hosts.
    /// </summary>
    private static (HttpListener Listener, string BaseUrl, Task Handler) StartLoopbackServer(string responseBody)
    {
        var port = GetFreeTcpPort();
        var baseUrl = $"http://127.0.0.1:{port}";
        var listener = new HttpListener();
        listener.Prefixes.Add(baseUrl + "/");
        listener.Start();

        var handler = Task.Run(async () =>
        {
            var context = await listener.GetContextAsync();
            var buffer = Encoding.UTF8.GetBytes(responseBody);
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer);
            context.Response.Close();
        });

        return (listener, baseUrl, handler);
    }

    private static int GetFreeTcpPort()
    {
        using var socket = new TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        var port = ((IPEndPoint)socket.LocalEndpoint).Port;
        socket.Stop();
        return port;
    }

    [Fact]
    public async Task GetWithRetryAsync_Returns_HttpResponseMessage()
    {
        // Arrange
        var factory = new HttpClientFactory();
        var baseUrl = "https://example.com";
        var path = "/api/data";
        var clientName = "test-client";

        // Act
        var response = await HttpClientFactoryExtensions.GetWithRetryAsync(factory, baseUrl, path, clientName);

        // Assert
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetWithRetryAsync_Throws_ArgumentNullException_When_Factory_Is_Null()
    {
        // Arrange
        var baseUrl = "https://example.com";
        var path = "/api/data";
        var clientName = "test-client";

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => HttpClientFactoryExtensions.GetWithRetryAsync(null, baseUrl, path, clientName));
    }

    [Fact]
    public async Task GetWithRetryAsync_Throws_ArgumentException_When_BaseUrl_Is_Empty()
    {
        // Arrange
        var factory = new HttpClientFactory();
        var path = "/api/data";
        var clientName = "test-client";

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(() => HttpClientFactoryExtensions.GetWithRetryAsync(factory, string.Empty, path, clientName));
    }

    [Fact]
    public async Task PostWithRetryAsync_Returns_HttpResponseMessage()
    {
        // Arrange
        var factory = new HttpClientFactory();
        var baseUrl = "https://example.com";
        var path = "/api/data";
        var jsonContent = "{\"key\":\"value\"}";
        var clientName = "test-client";

        // Act
        var response = await HttpClientFactoryExtensions.PostWithRetryAsync(factory, baseUrl, path, jsonContent, clientName);

        // Assert
        Assert.NotNull(response);
    }

    [Fact]
    public async Task PostWithRetryAsync_Throws_ArgumentNullException_When_Factory_Is_Null()
    {
        // Arrange
        var baseUrl = "https://example.com";
        var path = "/api/data";
        var jsonContent = "{\"key\":\"value\"}";
        var clientName = "test-client";

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => HttpClientFactoryExtensions.PostWithRetryAsync(null, baseUrl, path, jsonContent, clientName));
    }

    [Fact]
    public async Task PostWithRetryAsync_Throws_ArgumentException_When_BaseUrl_Is_Empty()
    {
        // Arrange
        var factory = new HttpClientFactory();
        var path = "/api/data";
        var jsonContent = "{\"key\":\"value\"}";
        var clientName = "test-client";

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(() => HttpClientFactoryExtensions.PostWithRetryAsync(factory, string.Empty, path, jsonContent, clientName));
    }

    [Fact]
    public async Task GetJsonAsync_Returns_String()
    {
        // Arrange
        var factory = new HttpClientFactory();
        var (listener, baseUrl, handler) = StartLoopbackServer("{\"status\":\"ok\"}");
        try
        {
            var path = "/api/data";
            var clientName = "test-client";

            // Act
            var json = await HttpClientFactoryExtensions.GetJsonAsync(factory, baseUrl, path, clientName);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("ok", json);
            await handler;
        }
        finally
        {
            listener.Close();
        }
    }

    [Fact]
    public async Task GetJsonAsync_Throws_ArgumentNullException_When_Factory_Is_Null()
    {
        // Arrange
        var baseUrl = "https://example.com";
        var path = "/api/data";
        var clientName = "test-client";

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => HttpClientFactoryExtensions.GetJsonAsync(null, baseUrl, path, clientName));
    }

    [Fact]
    public async Task GetJsonAsync_Throws_ArgumentException_When_BaseUrl_Is_Empty()
    {
        // Arrange
        var factory = new HttpClientFactory();
        var path = "/api/data";
        var clientName = "test-client";

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(() => HttpClientFactoryExtensions.GetJsonAsync(factory, string.Empty, path, clientName));
    }

    [Fact]
    public async Task PostJsonAsync_Returns_String()
    {
        // Arrange
        var factory = new HttpClientFactory();
        var (listener, baseUrl, handler) = StartLoopbackServer("{\"received\":true}");
        try
        {
            var path = "/api/data";
            var jsonContent = "{\"key\":\"value\"}";
            var clientName = "test-client";

            // Act
            var json = await HttpClientFactoryExtensions.PostJsonAsync(factory, baseUrl, path, jsonContent, clientName);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("received", json);
            await handler;
        }
        finally
        {
            listener.Close();
        }
    }

    [Fact]
    public async Task PostJsonAsync_Throws_ArgumentNullException_When_Factory_Is_Null()
    {
        // Arrange
        var baseUrl = "https://example.com";
        var path = "/api/data";
        var jsonContent = "{\"key\":\"value\"}";
        var clientName = "test-client";

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => HttpClientFactoryExtensions.PostJsonAsync(null, baseUrl, path, jsonContent, clientName));
    }

    [Fact]
    public async Task PostJsonAsync_Throws_ArgumentException_When_BaseUrl_Is_Empty()
    {
        // Arrange
        var factory = new HttpClientFactory();
        var path = "/api/data";
        var jsonContent = "{\"key\":\"value\"}";
        var clientName = "test-client";

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(() => HttpClientFactoryExtensions.PostJsonAsync(factory, string.Empty, path, jsonContent, clientName));
    }

    [Fact]
    public void CreateClient_ReusesSameHandler_ForSameBaseUrl()
    {
        // Arrange
        var factory = new HttpClientFactory();
        var baseUrl = "https://example.com";

        // Act - create multiple clients for the same base URL
        var client1 = factory.CreateClient(baseUrl);
        var client2 = factory.CreateClient(baseUrl);

        // Assert - both clients should work correctly
        Assert.NotNull(client1);
        Assert.NotNull(client2);
        Assert.Equal(baseUrl + "/", client1.BaseAddress?.AbsoluteUri);
        Assert.Equal(baseUrl + "/", client2.BaseAddress?.AbsoluteUri);

        client1.Dispose();
        client2.Dispose();
    }

    [Fact]
    public void CreateClient_CreatesDifferentClients_ForDifferentBaseUrls()
    {
        // Arrange
        var factory = new HttpClientFactory();
        var baseUrl1 = "https://api.example.com";
        var baseUrl2 = "https://api2.example.com";

        // Act
        var client1 = factory.CreateClient(baseUrl1);
        var client2 = factory.CreateClient(baseUrl2);

        // Assert
        Assert.NotNull(client1);
        Assert.NotNull(client2);
        Assert.Equal(baseUrl1 + "/", client1.BaseAddress?.AbsoluteUri);
        Assert.Equal(baseUrl2 + "/", client2.BaseAddress?.AbsoluteUri);

        client1.Dispose();
        client2.Dispose();
    }

    [Fact]
    public void CreateClient_WithCustomName_ReusesClient()
    {
        // Arrange
        var factory = new HttpClientFactory();
        var baseUrl = "https://example.com";
        var clientName = "my-custom-client";

        // Act - create multiple clients with the same custom name
        var client1 = factory.CreateClient(baseUrl, clientName);
        var client2 = factory.CreateClient(baseUrl, clientName);

        // Assert - both clients should work correctly
        Assert.NotNull(client1);
        Assert.NotNull(client2);
        Assert.Equal(baseUrl + "/", client1.BaseAddress?.AbsoluteUri);
        Assert.Equal(baseUrl + "/", client2.BaseAddress?.AbsoluteUri);

        client1.Dispose();
        client2.Dispose();
    }
}