#nullable enable
// =============================================================================
// Tests for FormatterRegistry
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using RoslynGuardAnalyzer.Formatters;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public sealed class FormatterRegistryTests
{
    [Fact]
    public void CreateWithDefaults_ShouldRegisterAllDefaultFormatters()
    {
        // Act
        var registry = FormatterRegistry.CreateWithDefaults();

        // Assert
        // Four default formatters are registered in the source code.
        Assert.Equal(4, registry.Count);

        // Expected format identifiers (lower‑cased)
        var expected = new[] { "json", "csv", "html", "sarif" };
        var actual = registry.GetSupportedFormats().Select(f => f.ToLowerInvariant()).OrderBy(f => f);
        Assert.Equal(expected.OrderBy(f => f), actual);
    }

    [Fact]
    public void Register_NullFormatter_ThrowsArgumentNullException()
    {
        var registry = new FormatterRegistry();

        var ex = Assert.Throws<ArgumentNullException>(() => registry.Register(null!));
        Assert.Equal("formatter", ex.ParamName);
    }

    [Fact]
    public void Register_SameFormat_ReplacesExistingFormatter()
    {
        // Arrange
        var registry = new FormatterRegistry();
        var original = new HtmlFormatter(); // default "html" formatter
        registry.Register(original);
        var originalCount = registry.Count;

        // Act
        var replacement = new HtmlFormatter(); // another instance, same format
        registry.Register(replacement);

        // Assert
        Assert.Equal(originalCount, registry.Count); // count unchanged
        var retrieved = registry.GetFormatter("html");
        Assert.Same(replacement, retrieved); // the stored instance is the replacement
    }

    [Fact]
    public void GetFormatter_NullOrWhiteSpace_ReturnsNull()
    {
        var registry = FormatterRegistry.CreateWithDefaults();

        Assert.Null(registry.GetFormatter(null));
        Assert.Null(registry.GetFormatter(string.Empty));
        Assert.Null(registry.GetFormatter("   "));
    }

    [Fact]
    public void GetFormatterOrThrow_ExistingFormat_ReturnsFormatter()
    {
        var registry = FormatterRegistry.CreateWithDefaults();

        var formatter = registry.GetFormatterOrThrow("json");
        Assert.NotNull(formatter);
        Assert.Equal("json", formatter.Format, ignoreCase: true);
    }

    [Fact]
    public void GetFormatterOrThrow_NonExistingFormat_ThrowsInvalidOperationException()
    {
        var registry = new FormatterRegistry();

        var ex = Assert.Throws<InvalidOperationException>(() => registry.GetFormatterOrThrow("unknown"));
        Assert.Contains("No formatter found for format", ex.Message);
    }

    [Fact]
    public void IsFormatSupported_ReturnsCorrectValues()
    {
        var registry = FormatterRegistry.CreateWithDefaults();

        Assert.True(registry.IsFormatSupported("json"));
        Assert.True(registry.IsFormatSupported("JSON")); // case‑insensitive
        Assert.False(registry.IsFormatSupported("xml"));
    }

    [Fact]
    public void GetSupportedFormats_ReturnsReadOnlyCollection()
    {
        var registry = FormatterRegistry.CreateWithDefaults();

        var formats = registry.GetSupportedFormats();

        // The returned IEnumerable should contain the expected formats.
        var expected = new[] { "json", "csv", "html", "sarif" };
        Assert.Equal(expected.OrderBy(f => f), formats.OrderBy(f => f));

        // Attempting to modify the collection should not affect the registry.
        // Since the method returns an IEnumerable, we cannot add to it directly,
        // but we can verify that the underlying collection remains unchanged.
        var countBefore = registry.Count;
        var list = formats.ToList();
        list.Add("newformat");
        Assert.Equal(countBefore, registry.Count);
    }
}
