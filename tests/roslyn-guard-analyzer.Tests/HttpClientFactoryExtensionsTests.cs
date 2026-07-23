// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using System.Net.Http;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Integration;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class HttpClientFactoryExtensionsTests
{
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
        var baseUrl = "https://example.com";
        var path = "/api/data";
        var clientName = "test-client";

        // Act
        var json = await HttpClientFactoryExtensions.GetJsonAsync(factory, baseUrl, path, clientName);

        // Assert
        Assert.NotNull(json);
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
        var baseUrl = "https://example.com";
        var path = "/api/data";
        var jsonContent = "{\"key\":\"value\"}";
        var clientName = "test-client";

        // Act
        var json = await HttpClientFactoryExtensions.PostJsonAsync(factory, baseUrl, path, jsonContent, clientName);

        // Assert
        Assert.NotNull(json);
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
}
