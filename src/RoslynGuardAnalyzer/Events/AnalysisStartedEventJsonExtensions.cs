#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoslynGuardAnalyzer.Events;

/// <summary>
/// Provides System.Text.Json serialization extensions for AnalysisStartedEvent.
/// </summary>
public static class AnalysisStartedEventJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the AnalysisStartedEvent to a JSON string.
    /// </summary>
    /// <param name="value">The event to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the event.</returns>
    public static string ToJson(this AnalysisStartedEvent value, bool indented = false)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes an AnalysisStartedEvent from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized AnalysisStartedEvent, or null if parsing fails.</returns>
    public static AnalysisStartedEvent? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AnalysisStartedEvent>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize an AnalysisStartedEvent from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized AnalysisStartedEvent, or null if parsing fails.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out AnalysisStartedEvent? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<AnalysisStartedEvent>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}