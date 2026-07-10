#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoslynGuardAnalyzer.Cli;

/// <summary>
/// Provides System.Text.Json serialization extensions for CliArgumentParser.
/// </summary>
public static class CliArgumentParserJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the CliArgumentParser to a JSON string.
    /// </summary>
    /// <param name="value">The CliArgumentParser instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the CliArgumentParser.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this CliArgumentParser value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value.Parse(), options);
    }

    /// <summary>
    /// Deserializes a JSON string to a CliArgumentParser instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A CliArgumentParser instance if successful; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static CliArgumentParser? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var options = JsonSerializer.Deserialize<CliOptions>(json, _jsonOptions);
        return options is not null ? new CliArgumentParser([]) : null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a CliArgumentParser instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized CliArgumentParser if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out CliArgumentParser? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        try
        {
            var options = JsonSerializer.Deserialize<CliOptions>(json, _jsonOptions);
            if (options is not null)
            {
                value = new CliArgumentParser([]);
                return true;
            }

            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}