#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoslynGuardAnalyzer.Utilities;

/// <summary>
/// Provides a consistent JSON I/O contract for serialization and deserialization across the application.
/// This helper standardizes error handling, security constraints, and serialization options.
/// </summary>
internal static class JsonIoHelper
{
    /// <summary>
    /// Default JSON serialization options used throughout the application.
    /// Uses camelCase naming policy, ignores null values when writing, and handles enums with camelCase naming.
    /// </summary>
    internal static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Default JSON serialization options with reference handling configured to ignore cycles.
    /// </summary>
    internal static readonly JsonSerializerOptions DefaultOptionsWithReferenceHandling = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Maximum JSON depth to prevent stack overflow attacks with deeply nested structures.
    /// </summary>
    internal const int MaxJsonDepth = 128;

    /// <summary>
    /// Maximum allowed input size in bytes to prevent memory exhaustion attacks.
    /// </summary>
    internal const int MaxJsonInputSize = 10 * 1024 * 1024; // 10 MB

    /// <summary>
    /// Maximum allowed length for file paths to prevent path traversal and excessive memory usage.
    /// </summary>
    internal const int MaxPathLength = 4096;

    /// <summary>
    /// Validates JSON input size to prevent memory exhaustion attacks.
    /// </summary>
    /// <param name="json">The JSON string to validate.</param>
    /// <param name="paramName">The name of the parameter for error reporting.</param>
    /// <exception cref="ArgumentException">Thrown when the JSON input exceeds size limits.</exception>
    internal static void ValidateJsonSize(string json, string paramName)
    {
        if (json.Length > MaxJsonInputSize)
        {
            throw new ArgumentException(
                $"JSON input exceeds maximum allowed size of {MaxJsonInputSize} bytes. Actual size: {json.Length} bytes.",
                paramName);
        }
    }

    /// <summary>
    /// Validates that a string is not null or whitespace.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The name of the parameter for error reporting.</param>
    /// <exception cref="ArgumentException">Thrown when the value is null or whitespace.</exception>
    internal static void ValidateNotNullOrWhiteSpace(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Validates that a string is not null.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The name of the parameter for error reporting.</param>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    internal static void ValidateNotNull(string? value, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value);
    }

    /// <summary>
    /// Creates a serialization options instance with the specified indentation setting.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <param name="withReferenceHandling">Whether to include reference handling configuration.</param>
    /// <returns>A configured JsonSerializerOptions instance.</returns>
    internal static JsonSerializerOptions CreateOptions(bool indented, bool withReferenceHandling = false)
    {
        var options = withReferenceHandling
            ? new JsonSerializerOptions(DefaultOptionsWithReferenceHandling)
            : new JsonSerializerOptions(DefaultOptions);

        if (indented)
        {
            options.WriteIndented = true;
        }

        return options;
    }
}