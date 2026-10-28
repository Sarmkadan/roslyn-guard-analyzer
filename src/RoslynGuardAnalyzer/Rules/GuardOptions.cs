#nullable enable

using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// Options for the guard analyzers, read from <c>.editorconfig</c> keys such as <c>guard.bounds_check.max_depth</c>.
/// Raw values are validated by <see cref="Parse"/>; an invalid value is reported through
/// <see cref="GuardOptionsParseResult.Errors"/> and that key falls back to its default.
/// </summary>
public sealed class GuardOptions
{
    /// <summary>
    /// Key that excludes private members from null-check analysis. Accepts <c>true</c> or <c>false</c>.
    /// </summary>
    public const string NullCheckExcludePrivateKey = "guard.null_check.exclude_private";

    /// <summary>
    /// Key that limits how many enclosing syntax levels bounds-check analysis searches for a guard.
    /// Accepts an integer from <see cref="MinBoundsCheckMaxDepth"/> to <see cref="MaxBoundsCheckMaxDepth"/>.
    /// </summary>
    public const string BoundsCheckMaxDepthKey = "guard.bounds_check.max_depth";

    /// <summary>
    /// Key that lists namespaces, separated by commas, whose code is excluded from guard analysis.
    /// A listed namespace also covers its sub-namespaces. An empty value clears the list.
    /// </summary>
    public const string IgnoredNamespacesKey = "guard.ignored_namespaces";

    /// <summary>
    /// Key that lists the indexable types bounds-check analysis checks, separated by commas. Each entry is a
    /// namespace-qualified type name without type arguments, such as <c>System.Collections.Generic.List</c>,
    /// or <c>[]</c> for arrays. The list must not be empty.
    /// </summary>
    public const string BoundsCheckAllowedIndexerTypesKey = "guard.bounds_check.allowed_indexer_types";

    /// <summary>
    /// Key that controls whether <c>Span&lt;T&gt;</c> and <c>ReadOnlySpan&lt;T&gt;</c> are checked. Accepts <c>true</c> or <c>false</c>.
    /// </summary>
    public const string BoundsCheckCheckSpansKey = "guard.bounds_check.check_spans";

    /// <summary>
    /// Key that overrides the severity of <c>BND001</c>. Accepts <c>silent</c>, <c>suggestion</c>, <c>warning</c> or <c>error</c>.
    /// </summary>
    public const string BoundsCheckSeverityKey = "guard.bounds_check.severity";

    /// <summary>
    /// Entry in <see cref="BoundsCheckAllowedIndexerTypesKey"/> that stands for array types.
    /// </summary>
    public const string ArrayIndexerTypeEntry = "[]";

    /// <summary>
    /// Smallest accepted value for <see cref="BoundsCheckMaxDepthKey"/>.
    /// </summary>
    public const int MinBoundsCheckMaxDepth = 1;

    /// <summary>
    /// Largest accepted value for <see cref="BoundsCheckMaxDepthKey"/>.
    /// </summary>
    public const int MaxBoundsCheckMaxDepth = 1000;

    /// <summary>
    /// Default bounds-check depth. It is the maximum accepted value, so ancestor search is effectively unlimited.
    /// </summary>
    public const int DefaultBoundsCheckMaxDepth = MaxBoundsCheckMaxDepth;

    /// <summary>
    /// Gets every configuration key understood by this type.
    /// </summary>
    public static ImmutableArray<string> KnownKeys { get; } = ImmutableArray.Create(
        NullCheckExcludePrivateKey,
        BoundsCheckMaxDepthKey,
        IgnoredNamespacesKey,
        BoundsCheckAllowedIndexerTypesKey,
        BoundsCheckCheckSpansKey,
        BoundsCheckSeverityKey);

    /// <summary>
    /// Gets the indexable types checked by default. This matches the built-in set before the options existed.
    /// </summary>
    public static ImmutableArray<string> DefaultAllowedIndexerTypes { get; } = ImmutableArray.Create(
        ArrayIndexerTypeEntry,
        "System.Collections.Generic.List",
        "System.Collections.Generic.IList",
        "System.Span",
        "System.ReadOnlySpan",
        "System.Memory",
        "System.ReadOnlyMemory");

    /// <summary>
    /// Gets the options with every key at its default value.
    /// </summary>
    public static GuardOptions Default { get; } = new(
        nullCheckExcludePrivate: false,
        boundsCheckMaxDepth: DefaultBoundsCheckMaxDepth,
        ignoredNamespaces: ImmutableArray<string>.Empty,
        boundsCheckAllowedIndexerTypes: DefaultAllowedIndexerTypes,
        boundsCheckCheckSpans: true,
        boundsCheckSeverity: DiagnosticSeverity.Warning);

    /// <summary>
    /// Initializes a new instance of the <see cref="GuardOptions"/> class with validated values.
    /// </summary>
    private GuardOptions(
        bool nullCheckExcludePrivate,
        int boundsCheckMaxDepth,
        ImmutableArray<string> ignoredNamespaces,
        ImmutableArray<string> boundsCheckAllowedIndexerTypes,
        bool boundsCheckCheckSpans,
        DiagnosticSeverity boundsCheckSeverity)
    {
        NullCheckExcludePrivate = nullCheckExcludePrivate;
        BoundsCheckMaxDepth = boundsCheckMaxDepth;
        IgnoredNamespaces = ignoredNamespaces;
        BoundsCheckAllowedIndexerTypes = boundsCheckAllowedIndexerTypes;
        BoundsCheckCheckSpans = boundsCheckCheckSpans;
        BoundsCheckSeverity = boundsCheckSeverity;
    }

    /// <summary>
    /// Gets a value indicating whether private members are excluded from null-check analysis.
    /// </summary>
    public bool NullCheckExcludePrivate { get; }

    /// <summary>
    /// Gets how many enclosing syntax levels bounds-check analysis searches for a guard.
    /// </summary>
    public int BoundsCheckMaxDepth { get; }

    /// <summary>
    /// Gets the namespaces excluded from guard analysis, without duplicates, in configuration order.
    /// </summary>
    public ImmutableArray<string> IgnoredNamespaces { get; }

    /// <summary>
    /// Gets the indexable types bounds-check analysis checks, in configuration order, without duplicates.
    /// Each entry is a namespace-qualified type name without type arguments, or <see cref="ArrayIndexerTypeEntry"/>.
    /// </summary>
    public ImmutableArray<string> BoundsCheckAllowedIndexerTypes { get; }

    /// <summary>
    /// Gets a value indicating whether <c>Span&lt;T&gt;</c> and <c>ReadOnlySpan&lt;T&gt;</c> are checked.
    /// A span type is checked only when it is also in <see cref="BoundsCheckAllowedIndexerTypes"/>.
    /// </summary>
    public bool BoundsCheckCheckSpans { get; }

    /// <summary>
    /// Gets the severity that <c>BND001</c> is reported with. Compiler <c>dotnet_diagnostic</c> settings still take precedence.
    /// </summary>
    public DiagnosticSeverity BoundsCheckSeverity { get; }

    /// <summary>
    /// Validates raw configuration values and builds <see cref="GuardOptions"/>.
    /// Keys that are absent use their defaults. Each invalid value adds a <see cref="GuardConfigurationError"/>
    /// and its key uses the default. Keys other than <see cref="KnownKeys"/> are ignored.
    /// </summary>
    /// <param name="values">The raw values keyed by configuration key.</param>
    /// <returns>The parsed options and any validation errors.</returns>
    public static GuardOptionsParseResult Parse(IReadOnlyDictionary<string, string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var errors = ImmutableArray.CreateBuilder<GuardConfigurationError>();

        var nullCheckExcludePrivate = Default.NullCheckExcludePrivate;
        if (values.TryGetValue(NullCheckExcludePrivateKey, out var excludePrivate))
        {
            if (bool.TryParse(excludePrivate.Trim(), out var parsed))
            {
                nullCheckExcludePrivate = parsed;
            }
            else
            {
                errors.Add(new GuardConfigurationError(NullCheckExcludePrivateKey, excludePrivate, "expected 'true' or 'false'"));
            }
        }

        var boundsCheckMaxDepth = Default.BoundsCheckMaxDepth;
        if (values.TryGetValue(BoundsCheckMaxDepthKey, out var maxDepth))
        {
            if (TryParseMaxDepth(maxDepth.Trim(), out var parsed))
            {
                boundsCheckMaxDepth = parsed;
            }
            else
            {
                errors.Add(new GuardConfigurationError(
                    BoundsCheckMaxDepthKey,
                    maxDepth,
                    $"expected an integer from {MinBoundsCheckMaxDepth} to {MaxBoundsCheckMaxDepth}"));
            }
        }

        var ignoredNamespaces = Default.IgnoredNamespaces;
        if (values.TryGetValue(IgnoredNamespacesKey, out var namespaces))
        {
            if (TryParseNamespaces(namespaces, out var parsed, out var reason))
            {
                ignoredNamespaces = parsed;
            }
            else
            {
                errors.Add(new GuardConfigurationError(IgnoredNamespacesKey, namespaces, reason));
            }
        }

        var allowedIndexerTypes = Default.BoundsCheckAllowedIndexerTypes;
        if (values.TryGetValue(BoundsCheckAllowedIndexerTypesKey, out var indexerTypes))
        {
            if (TryParseIndexerTypes(indexerTypes, out var parsed, out var reason))
            {
                allowedIndexerTypes = parsed;
            }
            else
            {
                errors.Add(new GuardConfigurationError(BoundsCheckAllowedIndexerTypesKey, indexerTypes, reason));
            }
        }

        var checkSpans = Default.BoundsCheckCheckSpans;
        if (values.TryGetValue(BoundsCheckCheckSpansKey, out var spans))
        {
            if (bool.TryParse(spans.Trim(), out var parsed))
            {
                checkSpans = parsed;
            }
            else
            {
                errors.Add(new GuardConfigurationError(BoundsCheckCheckSpansKey, spans, "expected 'true' or 'false'"));
            }
        }

        var severity = Default.BoundsCheckSeverity;
        if (values.TryGetValue(BoundsCheckSeverityKey, out var severityValue))
        {
            if (TryParseSeverity(severityValue.Trim(), out var parsed))
            {
                severity = parsed;
            }
            else
            {
                errors.Add(new GuardConfigurationError(
                    BoundsCheckSeverityKey,
                    severityValue,
                    "expected 'silent', 'suggestion', 'warning' or 'error'"));
            }
        }

        var options = new GuardOptions(
            nullCheckExcludePrivate,
            boundsCheckMaxDepth,
            ignoredNamespaces,
            allowedIndexerTypes,
            checkSpans,
            severity);
        return new GuardOptionsParseResult(options, errors.ToImmutable());
    }

    private static bool TryParseIndexerTypes(string value, out ImmutableArray<string> types, out string reason)
    {
        types = ImmutableArray<string>.Empty;
        reason = string.Empty;

        if (value.Trim().Length == 0)
        {
            reason = "must list at least one type";
            return false;
        }

        var builder = ImmutableArray.CreateBuilder<string>();
        foreach (var entry in value.Split(','))
        {
            var name = entry.Trim();
            if (name.Length == 0)
            {
                reason = "contains an empty type entry";
                return false;
            }

            if (name != ArrayIndexerTypeEntry && !IsValidNamespaceName(name))
            {
                reason = $"'{name}' is not a valid type name (use a namespace-qualified name without type arguments, or '[]')";
                return false;
            }

            if (!builder.Contains(name))
            {
                builder.Add(name);
            }
        }

        types = builder.ToImmutable();
        return true;
    }

    private static bool TryParseSeverity(string value, out DiagnosticSeverity severity)
    {
        var parsed = value.ToLowerInvariant() switch
        {
            "silent" => DiagnosticSeverity.Hidden,
            "suggestion" => DiagnosticSeverity.Info,
            "warning" => DiagnosticSeverity.Warning,
            "error" => DiagnosticSeverity.Error,
            _ => (DiagnosticSeverity?)null,
        };

        severity = parsed ?? DiagnosticSeverity.Warning;
        return parsed.HasValue;
    }

    private static bool TryParseMaxDepth(string value, out int depth)
    {
        // NumberStyles.None rejects signs, whitespace and separators, so only plain digits are accepted.
        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out depth)
            && depth >= MinBoundsCheckMaxDepth
            && depth <= MaxBoundsCheckMaxDepth;
    }

    private static bool TryParseNamespaces(string value, out ImmutableArray<string> namespaces, out string reason)
    {
        namespaces = ImmutableArray<string>.Empty;
        reason = string.Empty;

        if (value.Trim().Length == 0)
        {
            return true;
        }

        var builder = ImmutableArray.CreateBuilder<string>();
        foreach (var entry in value.Split(','))
        {
            var name = entry.Trim();
            if (name.Length == 0)
            {
                reason = "contains an empty namespace entry";
                return false;
            }

            if (!IsValidNamespaceName(name))
            {
                reason = $"'{name}' is not a valid namespace name";
                return false;
            }

            if (!builder.Contains(name))
            {
                builder.Add(name);
            }
        }

        namespaces = builder.ToImmutable();
        return true;
    }

    private static bool IsValidNamespaceName(string name)
    {
        foreach (var segment in name.Split('.'))
        {
            if (!IsIdentifier(segment))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsIdentifier(string segment)
    {
        if (segment.Length == 0 || !(char.IsLetter(segment[0]) || segment[0] == '_'))
        {
            return false;
        }

        foreach (var character in segment)
        {
            if (!(char.IsLetterOrDigit(character) || character == '_'))
            {
                return false;
            }
        }

        return true;
    }
}
