using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="BackgroundTaskQueue"/>.
/// </summary>
public static class BackgroundTaskQueueJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
		WriteIndented = false
	};

	/// <summary>
	/// Serializes the <see cref="BackgroundTaskQueue"/> to a JSON string.
	/// </summary>
	/// <param name="value">The task queue to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>A JSON string representation of the task queue.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this BackgroundTaskQueue value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a <see cref="BackgroundTaskQueue"/> from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized task queue, or <see langword="null"/> if the JSON is <see langword="null"/> or empty.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="JsonException">The JSON is invalid or cannot be deserialized into a <see cref="BackgroundTaskQueue"/>.</exception>
	public static BackgroundTaskQueue? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		return string.IsNullOrEmpty(json)
			? null
			: JsonSerializer.Deserialize<BackgroundTaskQueue>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a <see cref="BackgroundTaskQueue"/> from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized task queue if successful.</param>
	/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
	public static bool TryFromJson(string json, out BackgroundTaskQueue? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		value = null;

		if (string.IsNullOrEmpty(json))
		{
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<BackgroundTaskQueue>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}