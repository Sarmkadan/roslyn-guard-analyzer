#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for AnalysisResult.
/// Includes security hardening against DoS attacks via maliciously crafted JSON input.
/// </summary>
public static class AnalysisResultJsonExtensions
{
    // Maximum JSON depth to prevent stack overflow attacks with deeply nested structures
    private const int MaxJsonDepth = 128;

    // Maximum allowed input size in bytes to prevent memory exhaustion attacks
    private const int MaxJsonInputSize = 10 * 1024 * 1024; // 10 MB

    // Maximum allowed length for file paths to prevent path traversal and excessive memory usage
    private const int MaxPathLength = 4096;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        MaxDepth = MaxJsonDepth,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes an AnalysisResult to a JSON string.
    /// </summary>
    /// <param name="value">The AnalysisResult to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the AnalysisResult.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this AnalysisResult value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes an AnalysisResult from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized AnalysisResult, or null if JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the JSON input exceeds size limits.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static AnalysisResult? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        // Validate input size to prevent memory exhaustion attacks
        if (json.Length > MaxJsonInputSize)
        {
            throw new ArgumentException(
                $"JSON input exceeds maximum allowed size of {MaxJsonInputSize} bytes. Actual size: {json.Length} bytes.",
                nameof(json));
        }

        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<AnalysisResult>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize an AnalysisResult from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized AnalysisResult if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the JSON input exceeds size limits.</exception>
    public static bool TryFromJson(string json, out AnalysisResult? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        // Validate input size to prevent memory exhaustion attacks
        if (json.Length > MaxJsonInputSize)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<AnalysisResult>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}