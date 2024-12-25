#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Configuration;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for <see cref="RuleConfigurationBuilder"/>.
/// </summary>
public static class RuleConfigurationBuilderJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the <see cref="RuleConfigurationBuilder"/> to a JSON string.
    /// </summary>
    /// <param name="value">The builder instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this RuleConfigurationBuilder value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var config = value.Build();

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(config, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="RuleConfigurationBuilder"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A new <see cref="RuleConfigurationBuilder"/> instance, or null if the JSON is invalid.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static RuleConfigurationBuilder? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var config = JsonSerializer.Deserialize<RuleConfiguration>(json, _jsonSerializerOptions);

            if (config is null)
            {
                return null;
            }

            var builder = new RuleConfigurationBuilder(config.Name ?? "DeserializedRule")
                .WithDescription(config.Description ?? string.Empty);

            if (config.CustomSettings.TryGetValue("Enabled", out var enabledValue) &&
                bool.TryParse(enabledValue, out var enabledBool))
            {
                builder.WithEnabled(enabledBool);
            }
            else
            {
                builder.WithEnabled(true);
            }

            if (config.CustomSettings.TryGetValue("Severity", out var severity))
            {
                builder.WithSeverity(severity);
            }

            foreach (var setting in config.CustomSettings)
            {
                if (setting.Key is "Enabled" or "Severity")
                {
                    continue;
                }

                if (setting.Value is not null)
                {
                    builder.WithParameter(setting.Key, setting.Value);
                }
            }

            return builder;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="RuleConfigurationBuilder"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized builder, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeded; otherwise false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out RuleConfigurationBuilder? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = FromJson(json);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
