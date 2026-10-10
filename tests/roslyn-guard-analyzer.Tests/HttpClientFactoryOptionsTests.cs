#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using FluentAssertions;
using RoslynGuardAnalyzer.Integration;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Provides unit tests for the <see cref="HttpClientFactoryOptions"/> class.
/// </summary>
public sealed class HttpClientFactoryOptionsTests
{
    /// <summary>
    /// Tests that <see cref="HttpClientFactoryOptions.ToString"/> lists the default option values.
    /// </summary>
    [Fact]
    public void ToString_WithDefaults_ListsDefaultValues()
    {
        // Arrange
        var options = new HttpClientFactoryOptions();

        // Act
        var result = options.ToString();

        // Assert
        result.Should().StartWith("HttpClientFactoryOptions {");
        result.Should().Contain($"DefaultTimeout={TimeSpan.FromSeconds(30)}");
        result.Should().Contain("MaxRetries=3");
        result.Should().Contain("CircuitBreakerFailureThreshold=5");
        result.Should().Contain($"CircuitBreakerOpenDuration={TimeSpan.FromSeconds(30)}");
        result.Should().Contain($"PooledConnectionLifetime={TimeSpan.FromMinutes(2)}");
        result.Should().Contain("MaxConnectionsPerServer=100");
        result.Should().Contain("EnableDnsRefresh=True");
    }

    /// <summary>
    /// Tests that <see cref="HttpClientFactoryOptions.ToString"/> reflects custom option values.
    /// </summary>
    [Fact]
    public void ToString_WithCustomValues_ReflectsValues()
    {
        // Arrange
        var options = new HttpClientFactoryOptions
        {
            DefaultTimeout = TimeSpan.FromSeconds(12),
            MaxRetries = 7,
            CircuitBreakerFailureThreshold = 9,
            CircuitBreakerOpenDuration = TimeSpan.FromSeconds(45),
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            MaxConnectionsPerServer = 20,
            EnableDnsRefresh = false
        };

        // Act
        var result = options.ToString();

        // Assert
        result.Should().Contain($"DefaultTimeout={TimeSpan.FromSeconds(12)}");
        result.Should().Contain("MaxRetries=7");
        result.Should().Contain("CircuitBreakerFailureThreshold=9");
        result.Should().Contain($"CircuitBreakerOpenDuration={TimeSpan.FromSeconds(45)}");
        result.Should().Contain($"PooledConnectionLifetime={TimeSpan.FromMinutes(10)}");
        result.Should().Contain("MaxConnectionsPerServer=20");
        result.Should().Contain("EnableDnsRefresh=False");
    }

    /// <summary>
    /// Tests that <see cref="HttpClientFactoryOptions.ToString"/> does not expose credential-like names or values.
    /// </summary>
    [Fact]
    public void ToString_Always_DoesNotContainCredentialFields()
    {
        // Arrange
        var options = new HttpClientFactoryOptions();

        // Act
        var result = options.ToString();

        // Assert
        result.Should().NotContainEquivalentOf("token");
        result.Should().NotContainEquivalentOf("apikey");
        result.Should().NotContainEquivalentOf("secret");
        result.Should().NotContainEquivalentOf("password");
    }
}
