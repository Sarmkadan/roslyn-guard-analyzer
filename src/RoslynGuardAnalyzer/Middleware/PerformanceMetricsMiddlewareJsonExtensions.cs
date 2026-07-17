#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoslynGuardAnalyzer.Middleware;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="PerformanceMetricsMiddleware.PerformanceMetrics"/>
/// class using System.Text.Json.
/// </summary>
/// <example>
/// <code>
/// var middleware = new PerformanceMetricsMiddleware();
/// var json = middleware.ToJson();
/// var deserialized = PerformanceMetricsMiddlewareJsonExtensions.FromJson(json);
/// </code>
/// </example>
public static class PerformanceMetricsMiddlewareJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the <see cref="PerformanceMetricsMiddleware.PerformanceMetrics"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The performance metrics to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the performance metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this PerformanceMetricsMiddleware.PerformanceMetrics value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="PerformanceMetricsMiddleware.PerformanceMetrics"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized performance metrics, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized.</exception>
    public static PerformanceMetricsMiddleware.PerformanceMetrics? FromJson(string json) =>
        string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<PerformanceMetricsMiddleware.PerformanceMetrics>(json, _jsonSerializerOptions);

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="PerformanceMetricsMiddleware.PerformanceMetrics"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized performance metrics if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <example>
    /// <code>
    /// if (PerformanceMetricsMiddlewareJsonExtensions.TryFromJson(json, out var metrics))
    /// {
    ///     Console.WriteLine($"Total time: {metrics.TotalMilliseconds}ms");
    /// }
    /// </code>
    /// </example>
    public static bool TryFromJson(string json, out PerformanceMetricsMiddleware.PerformanceMetrics? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<PerformanceMetricsMiddleware.PerformanceMetrics>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
