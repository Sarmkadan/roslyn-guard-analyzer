#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoslynGuardAnalyzer.Utilities;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="TypeNameMatcher"/>.
/// </summary>
public static class TypeNameMatcherJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="TypeNameMatcher"/> to a JSON string.
    /// </summary>
    /// <param name="value">The matcher to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the matcher.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this TypeNameMatcher value, bool indented = false) =>
        JsonSerializer.Serialize(
            new { pattern = value.Pattern },
            indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);

    /// <summary>
    /// Deserializes a <see cref="TypeNameMatcher"/> from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A new <see cref="TypeNameMatcher"/> instance, or null if the JSON is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static TypeNameMatcher? FromJson(string? json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return root.ValueKind switch
            {
                JsonValueKind.Object when root.TryGetProperty("pattern", out var patternElement) =>
                    patternElement.ValueKind == JsonValueKind.String
                        ? new TypeNameMatcher(patternElement.GetString()!)
                        : throw new JsonException("Pattern property must be a string."),

                JsonValueKind.String => new TypeNameMatcher(root.GetString()!),

                _ => throw new JsonException("Expected a JSON object with 'pattern' property or a JSON string.")
            };
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new JsonException("Failed to deserialize TypeNameMatcher from JSON.", ex);
        }
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="TypeNameMatcher"/> from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized matcher, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string? json, out TypeNameMatcher? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            value = FromJson(json);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}