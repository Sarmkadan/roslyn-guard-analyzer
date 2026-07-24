#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// =============================================================================

using System;

namespace RoslynGuardAnalyzer.Integration;

/// <summary>
/// Options to configure <see cref="HttpClientFactory"/> behavior.
/// </summary>
public sealed class HttpClientFactoryOptions
{
    /// <summary>
    /// Gets the default timeout applied to created <see cref="HttpClient"/> instances.
    /// </summary>
    public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets the maximum number of retry attempts for transient failures.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Gets the number of consecutive failures that will open the circuit breaker.
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; init; } = 5;

    /// <summary>
    /// Gets the duration for which the circuit breaker remains open before allowing new attempts.
    /// </summary>
    public TimeSpan CircuitBreakerOpenDuration { get; init; } = TimeSpan.FromSeconds(30);

/// <summary>
/// Gets the lifetime of pooled connections in the <see cref="SocketsHttpHandler"/>.
/// Defaults to 2 minutes, which balances DNS refresh with connection reuse.
/// </summary>
public TimeSpan PooledConnectionLifetime { get; init; } = TimeSpan.FromMinutes(2);

/// <summary>
/// Gets the maximum number of connections per server endpoint.
/// Defaults to 100, which is the .NET default for HttpClient.
/// </summary>
public int MaxConnectionsPerServer { get; init; } = 100;

/// <summary>
/// Gets whether to enable DNS refresh for pooled connections.
/// When true, DNS records are refreshed after <see cref="PooledConnectionLifetime"/>.
/// </summary>
public bool EnableDnsRefresh { get; init; } = true;
}
