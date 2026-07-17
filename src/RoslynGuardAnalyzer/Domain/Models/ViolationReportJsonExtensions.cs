#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ViolationReport"/>.
/// </summary>
public static class ViolationReportJsonExtensions
{

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Converts a <see cref="ViolationReport"/> instance to its JSON representation.
    /// </summary>
    /// <param name="value">The violation report to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the violation report.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this ViolationReport value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Parses a JSON string into a <see cref="ViolationReport"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="ViolationReport"/> instance, or <see langword="null"/> if parsing fails.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static ViolationReport? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException(
                "JSON string cannot consist only of whitespace characters.",
                nameof(json));
        }

        try
        {
            return JsonSerializer.Deserialize<ViolationReport>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to parse a JSON string into a <see cref="ViolationReport"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized <see cref="ViolationReport"/> instance if successful.</param>
    /// <returns><see langword="true"/> if parsing succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static bool TryFromJson(string json, out ViolationReport? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException(
                "JSON string cannot consist only of whitespace characters.",
                nameof(json));
        }

        try
        {
            value = JsonSerializer.Deserialize<ViolationReport>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}