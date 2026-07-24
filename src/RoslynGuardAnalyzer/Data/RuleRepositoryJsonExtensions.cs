#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Text.Json;

namespace RoslynGuardAnalyzer.Data;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for <see cref="RuleRepository"/>.
/// Includes security hardening against DoS attacks via maliciously crafted JSON input.
/// </summary>
public static class RuleRepositoryJsonExtensions
{
    // Maximum JSON depth to prevent stack overflow attacks with deeply nested structures
    private const int MaxJsonDepth = 128;

    // Maximum allowed input size in bytes to prevent memory exhaustion attacks
    private const int MaxJsonInputSize = 10 * 1024 * 1024; // 10 MB

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        MaxDepth = MaxJsonDepth
    };

    /// <summary>
    /// Serializes the <see cref="RuleRepository"/> to a JSON string.
    /// </summary>
    /// <param name="value">The repository to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the repository.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this RuleRepository value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value.GetAll(), options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="RuleRepository"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="RuleRepository"/> instance populated with the deserialized data, or <see langword="null"/> if the JSON is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the JSON input exceeds size limits.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static RuleRepository? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        // Validate input size to prevent memory exhaustion attacks
        if (json.Length > MaxJsonInputSize)
        {
            throw new ArgumentException(
                $"JSON input exceeds maximum allowed size of {MaxJsonInputSize} bytes. Actual size: {json.Length} bytes.",
                nameof(json));
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var rules = JsonSerializer.Deserialize<System.Collections.Generic.List<RoslynGuardAnalyzer.Domain.Models.AnalysisRule>>(json, _jsonSerializerOptions);

            var repository = new RuleRepository();

            if (rules is not null)
            {
                foreach (var rule in rules)
                {
                    repository.Add(rule.Id, rule);
                }
            }

            return repository;
        }
        catch (JsonException ex)
        {
            throw new JsonException("Failed to parse rule repository JSON. Ensure the JSON is valid and all required properties are present.", ex);
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="RuleRepository"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized repository, or <see langword="null"/> if parsing fails.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the JSON input exceeds size limits.</exception>
    public static bool TryFromJson(string json, out RuleRepository? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        try
        {
            // Validate input size to prevent memory exhaustion attacks
            if (json.Length > MaxJsonInputSize)
            {
                return false;
            }

            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}