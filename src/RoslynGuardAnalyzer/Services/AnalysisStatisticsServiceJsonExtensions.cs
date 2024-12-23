#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Provides System.Text.Json serialization extensions for AnalysisStatisticsService types.
/// </summary>
public static class AnalysisStatisticsServiceJsonExtensions
{
    /// <summary>
    /// JSON serialization options with camelCase naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Converts AnalysisStatisticsService.ViolationStatistics to JSON string.
    /// </summary>
    /// <param name="value">The violation statistics to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON representation of the statistics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static string ToJson(this AnalysisStatisticsService.ViolationStatistics value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Parses JSON string into AnalysisStatisticsService.ViolationStatistics.
    /// </summary>
    /// <param name="json">JSON string to parse.</param>
    /// <returns>The deserialized violation statistics, or null if JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized.</exception>
    public static AnalysisStatisticsService.ViolationStatistics? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<AnalysisStatisticsService.ViolationStatistics>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to parse JSON string into AnalysisStatisticsService.ViolationStatistics.
    /// </summary>
    /// <param name="json">JSON string to parse.</param>
    /// <param name="value">Receives the deserialized value if successful.</param>
    /// <returns>True if parsing succeeded; false otherwise.</returns>
    public static bool TryFromJson(string json, out AnalysisStatisticsService.ViolationStatistics? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<AnalysisStatisticsService.ViolationStatistics>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}