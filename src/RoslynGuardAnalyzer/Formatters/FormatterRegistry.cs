// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace RoslynGuardAnalyzer.Formatters;

/// <summary>
/// Registry for output formatters supporting multiple output formats.
/// Allows dynamic registration and lookup of formatters by format identifier.
/// </summary>
public sealed class FormatterRegistry
{
    private readonly Dictionary<string, IOutputFormatter> _formatters = [];

    /// <summary>
    /// Creates a registry with default formatters pre-registered.
    /// </summary>
    public static FormatterRegistry CreateWithDefaults()
    {
        var registry = new FormatterRegistry();
        registry.Register(new JsonFormatter());
        registry.Register(new CsvFormatter());
        registry.Register(new HtmlFormatter());
        return registry;
    }

    /// <summary>
    /// Registers a formatter for a specific format.
    /// </summary>
    public void Register(IOutputFormatter formatter)
    {
        if (formatter == null)
            throw new ArgumentNullException(nameof(formatter));

        _formatters[formatter.Format.ToLowerInvariant()] = formatter;
    }

    /// <summary>
    /// Gets a formatter by format identifier.
    /// </summary>
    public IOutputFormatter? GetFormatter(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
            return null;

        var key = format.ToLowerInvariant();

        return _formatters.TryGetValue(key, out var formatter)
            ? formatter
            : null;
    }

    /// <summary>
    /// Checks if a format is supported.
    /// </summary>
    public bool IsFormatSupported(string format)
    {
        return GetFormatter(format) != null;
    }

    /// <summary>
    /// Gets all supported format identifiers.
    /// </summary>
    public IEnumerable<string> GetSupportedFormats()
    {
        return _formatters.Keys.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the count of registered formatters.
    /// </summary>
    public int Count => _formatters.Count;

    /// <summary>
    /// Gets a formatter or throws an exception if not found.
    /// </summary>
    public IOutputFormatter GetFormatterOrThrow(string format)
    {
        var formatter = GetFormatter(format);
        if (formatter == null)
            throw new InvalidOperationException($"No formatter found for format: {format}");

        return formatter;
    }
}
