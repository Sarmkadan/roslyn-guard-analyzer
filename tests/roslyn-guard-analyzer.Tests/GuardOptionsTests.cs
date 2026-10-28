#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using RoslynGuardAnalyzer.Rules;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="GuardOptions.Parse"/> covering defaults, valid values, and validation errors.
/// </summary>
public sealed class GuardOptionsTests
{
    private static GuardOptionsParseResult ParseSingle(string key, string value)
    {
        return GuardOptions.Parse(new Dictionary<string, string> { [key] = value });
    }

    [Fact]
    public void Parse_NoValues_ReturnsDefaultsWithoutErrors()
    {
        // Arrange
        var values = new Dictionary<string, string>();

        // Act
        var result = GuardOptions.Parse(values);

        // Assert
        result.HasErrors.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.Options.NullCheckExcludePrivate.Should().BeFalse();
        result.Options.BoundsCheckMaxDepth.Should().Be(GuardOptions.DefaultBoundsCheckMaxDepth);
        result.Options.IgnoredNamespaces.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ValidValues_ParsesEveryKey()
    {
        // Arrange
        var values = new Dictionary<string, string>
        {
            [GuardOptions.NullCheckExcludePrivateKey] = "true",
            [GuardOptions.BoundsCheckMaxDepthKey] = "12",
            [GuardOptions.IgnoredNamespacesKey] = "App.Generated, Tests",
        };

        // Act
        var result = GuardOptions.Parse(values);

        // Assert
        result.HasErrors.Should().BeFalse();
        result.Options.NullCheckExcludePrivate.Should().BeTrue();
        result.Options.BoundsCheckMaxDepth.Should().Be(12);
        result.Options.IgnoredNamespaces.Should().Equal("App.Generated", "Tests");
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("TRUE", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    public void Parse_BooleanValue_IsCaseInsensitive(string value, bool expected)
    {
        // Arrange

        // Act
        var result = ParseSingle(GuardOptions.NullCheckExcludePrivateKey, value);

        // Assert
        result.HasErrors.Should().BeFalse();
        result.Options.NullCheckExcludePrivate.Should().Be(expected);
    }

    [Theory]
    [InlineData("yes")]
    [InlineData("1")]
    [InlineData("on")]
    [InlineData("")]
    public void Parse_InvalidBoolean_ReportsErrorAndUsesDefault(string value)
    {
        // Arrange

        // Act
        var result = ParseSingle(GuardOptions.NullCheckExcludePrivateKey, value);

        // Assert
        result.HasErrors.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new GuardConfigurationError(
                GuardOptions.NullCheckExcludePrivateKey,
                value,
                "expected 'true' or 'false'"));
        result.Options.NullCheckExcludePrivate.Should().BeFalse();
    }

    [Theory]
    [InlineData("1")]
    [InlineData("500")]
    [InlineData("1000")]
    public void Parse_MaxDepthWithinRange_IsAccepted(string value)
    {
        // Arrange
        var expected = int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        // Act
        var result = ParseSingle(GuardOptions.BoundsCheckMaxDepthKey, value);

        // Assert
        result.HasErrors.Should().BeFalse();
        result.Options.BoundsCheckMaxDepth.Should().Be(expected);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1001")]
    [InlineData("+5")]
    [InlineData("2.5")]
    [InlineData("1e3")]
    [InlineData("0x10")]
    [InlineData("abc")]
    [InlineData("")]
    public void Parse_MaxDepthOutOfRangeOrMalformed_ReportsErrorAndUsesDefault(string value)
    {
        // Arrange

        // Act
        var result = ParseSingle(GuardOptions.BoundsCheckMaxDepthKey, value);

        // Assert
        result.HasErrors.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Match<GuardConfigurationError>(
            e => e.Key == GuardOptions.BoundsCheckMaxDepthKey
                && e.Value == value
                && e.Reason.Contains("from 1 to 1000", StringComparison.Ordinal));
        result.Options.BoundsCheckMaxDepth.Should().Be(GuardOptions.DefaultBoundsCheckMaxDepth);
    }

    [Fact]
    public void Parse_IgnoredNamespaces_TrimsEntriesAndRemovesDuplicates()
    {
        // Arrange

        // Act
        var result = ParseSingle(GuardOptions.IgnoredNamespacesKey, " App.Generated , Tests,App.Generated ");

        // Assert
        result.HasErrors.Should().BeFalse();
        result.Options.IgnoredNamespaces.Should().Equal("App.Generated", "Tests");
    }

    [Fact]
    public void Parse_EmptyIgnoredNamespaces_IsValidAndClearsList()
    {
        // Arrange

        // Act
        var result = ParseSingle(GuardOptions.IgnoredNamespacesKey, "   ");

        // Assert
        result.HasErrors.Should().BeFalse();
        result.Options.IgnoredNamespaces.Should().BeEmpty();
    }

    [Theory]
    [InlineData("1App")]
    [InlineData("App..Generated")]
    [InlineData("App.")]
    [InlineData(".App")]
    [InlineData("App Generated")]
    [InlineData("App-Generated")]
    [InlineData("App;Tests")]
    [InlineData("App,,Tests")]
    [InlineData("App,")]
    [InlineData(",App")]
    public void Parse_InvalidNamespaceSyntax_ReportsErrorAndUsesDefault(string value)
    {
        // Arrange

        // Act
        var result = ParseSingle(GuardOptions.IgnoredNamespacesKey, value);

        // Assert
        result.HasErrors.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Key.Should().Be(GuardOptions.IgnoredNamespacesKey);
        result.Options.IgnoredNamespaces.Should().BeEmpty();
    }

    [Theory]
    [InlineData("_Internal")]
    [InlineData("App2.Models_v1")]
    [InlineData("Данные.Модели")]
    public void Parse_LegalIdentifierSegments_AreAccepted(string value)
    {
        // Arrange

        // Act
        var result = ParseSingle(GuardOptions.IgnoredNamespacesKey, value);

        // Assert
        result.HasErrors.Should().BeFalse();
        result.Options.IgnoredNamespaces.Should().Equal(value);
    }

    [Fact]
    public void Parse_InvalidNamespaceEntry_ReportsEntryInReason()
    {
        // Arrange

        // Act
        var result = ParseSingle(GuardOptions.IgnoredNamespacesKey, "Tests, App..Generated");

        // Assert
        result.Errors.Should().ContainSingle()
            .Which.Reason.Should().Be("'App..Generated' is not a valid namespace name");
    }

    [Fact]
    public void Parse_AllKeysInvalid_ReportsOneErrorPerKeyInKeyOrder()
    {
        // Arrange
        var values = new Dictionary<string, string>
        {
            [GuardOptions.IgnoredNamespacesKey] = "1App",
            [GuardOptions.BoundsCheckMaxDepthKey] = "0",
            [GuardOptions.NullCheckExcludePrivateKey] = "maybe",
        };

        // Act
        var result = GuardOptions.Parse(values);

        // Assert
        result.Errors.Select(e => e.Key).Should().Equal(
            GuardOptions.NullCheckExcludePrivateKey,
            GuardOptions.BoundsCheckMaxDepthKey,
            GuardOptions.IgnoredNamespacesKey);
        result.Options.Should().BeEquivalentTo(GuardOptions.Default);
    }

    [Fact]
    public void Parse_ValidAndInvalidKeys_KeepsValidValues()
    {
        // Arrange
        var values = new Dictionary<string, string>
        {
            [GuardOptions.NullCheckExcludePrivateKey] = "true",
            [GuardOptions.BoundsCheckMaxDepthKey] = "0",
        };

        // Act
        var result = GuardOptions.Parse(values);

        // Assert
        result.Errors.Should().ContainSingle().Which.Key.Should().Be(GuardOptions.BoundsCheckMaxDepthKey);
        result.Options.NullCheckExcludePrivate.Should().BeTrue();
        result.Options.BoundsCheckMaxDepth.Should().Be(GuardOptions.DefaultBoundsCheckMaxDepth);
    }

    [Fact]
    public void Parse_UnknownKeys_AreIgnored()
    {
        // Arrange
        var values = new Dictionary<string, string>
        {
            ["guard.unknown_option"] = "not-validated",
        };

        // Act
        var result = GuardOptions.Parse(values);

        // Assert
        result.HasErrors.Should().BeFalse();
        result.Options.Should().BeEquivalentTo(GuardOptions.Default);
    }

    [Fact]
    public void Parse_NullDictionary_ThrowsArgumentNullException()
    {
        // Arrange

        // Act
        var act = () => GuardOptions.Parse(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void KnownKeys_ContainsEveryPublicKey()
    {
        // Arrange

        // Act
        var keys = GuardOptions.KnownKeys;

        // Assert
        keys.Should().BeEquivalentTo(ImmutableArray.Create(
            GuardOptions.NullCheckExcludePrivateKey,
            GuardOptions.BoundsCheckMaxDepthKey,
            GuardOptions.IgnoredNamespacesKey,
            GuardOptions.BoundsCheckAllowedIndexerTypesKey,
            GuardOptions.BoundsCheckCheckSpansKey,
            GuardOptions.BoundsCheckSeverityKey));
    }

    // ==================== guard.bounds_check.allowed_indexer_types ====================

    [Fact]
    public void Parse_NoValues_UsesDefaultIndexerTypes()
    {
        // Arrange & Act
        var options = GuardOptions.Parse(new Dictionary<string, string>()).Options;

        // Assert
        options.BoundsCheckAllowedIndexerTypes.Should().Equal(GuardOptions.DefaultAllowedIndexerTypes);
        options.BoundsCheckCheckSpans.Should().BeTrue();
        options.BoundsCheckSeverity.Should().Be(DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Parse_IndexerTypes_TrimsEntriesAndRemovesDuplicates()
    {
        // Arrange & Act
        var result = ParseSingle(GuardOptions.BoundsCheckAllowedIndexerTypesKey, " [] , System.String,System.String ");

        // Assert
        result.HasErrors.Should().BeFalse();
        result.Options.BoundsCheckAllowedIndexerTypes.Should().Equal("[]", "System.String");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_EmptyIndexerTypes_ReportsErrorAndUsesDefault(string value)
    {
        // Arrange & Act
        var result = ParseSingle(GuardOptions.BoundsCheckAllowedIndexerTypesKey, value);

        // Assert
        result.Errors.Should().ContainSingle()
            .Which.Reason.Should().Be("must list at least one type");
        result.Options.BoundsCheckAllowedIndexerTypes.Should().Equal(GuardOptions.DefaultAllowedIndexerTypes);
    }

    [Theory]
    [InlineData("System.Collections.Generic.List<int>")]
    [InlineData("System..String")]
    [InlineData("[]?")]
    public void Parse_InvalidIndexerTypeName_ReportsErrorAndUsesDefault(string entry)
    {
        // Arrange & Act
        var result = ParseSingle(GuardOptions.BoundsCheckAllowedIndexerTypesKey, $"System.String, {entry}");

        // Assert
        result.Errors.Should().ContainSingle()
            .Which.Reason.Should().StartWith($"'{entry}' is not a valid type name");
        result.Options.BoundsCheckAllowedIndexerTypes.Should().Equal(GuardOptions.DefaultAllowedIndexerTypes);
    }

    // ==================== guard.bounds_check.check_spans ====================

    [Theory]
    [InlineData("false", false)]
    [InlineData("TRUE", true)]
    public void Parse_CheckSpans_ParsesBoolean(string value, bool expected)
    {
        // Arrange & Act
        var result = ParseSingle(GuardOptions.BoundsCheckCheckSpansKey, value);

        // Assert
        result.HasErrors.Should().BeFalse();
        result.Options.BoundsCheckCheckSpans.Should().Be(expected);
    }

    [Fact]
    public void Parse_InvalidCheckSpans_ReportsErrorAndUsesDefault()
    {
        // Arrange & Act
        var result = ParseSingle(GuardOptions.BoundsCheckCheckSpansKey, "yes");

        // Assert
        result.Errors.Should().ContainSingle()
            .Which.Reason.Should().Be("expected 'true' or 'false'");
        result.Options.BoundsCheckCheckSpans.Should().BeTrue();
    }

    // ==================== guard.bounds_check.severity ====================

    [Theory]
    [InlineData("silent", DiagnosticSeverity.Hidden)]
    [InlineData("Suggestion", DiagnosticSeverity.Info)]
    [InlineData("WARNING", DiagnosticSeverity.Warning)]
    [InlineData("error", DiagnosticSeverity.Error)]
    public void Parse_Severity_MapsKeywordToDiagnosticSeverity(string value, DiagnosticSeverity expected)
    {
        // Arrange & Act
        var result = ParseSingle(GuardOptions.BoundsCheckSeverityKey, value);

        // Assert
        result.HasErrors.Should().BeFalse();
        result.Options.BoundsCheckSeverity.Should().Be(expected);
    }

    [Theory]
    [InlineData("none")]
    [InlineData("info")]
    [InlineData("fatal")]
    public void Parse_UnsupportedSeverity_ReportsErrorAndUsesDefault(string value)
    {
        // Arrange & Act
        var result = ParseSingle(GuardOptions.BoundsCheckSeverityKey, value);

        // Assert
        result.Errors.Should().ContainSingle()
            .Which.Reason.Should().Be("expected 'silent', 'suggestion', 'warning' or 'error'");
        result.Options.BoundsCheckSeverity.Should().Be(DiagnosticSeverity.Warning);
    }
}
