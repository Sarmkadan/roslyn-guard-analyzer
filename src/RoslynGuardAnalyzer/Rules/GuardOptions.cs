#nullable enable

using System.Collections.Immutable;
using System.Globalization;

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
        IgnoredNamespacesKey);

    /// <summary>
    /// Gets the options with every key at its default value.
    /// </summary>
    public static GuardOptions Default { get; } = new(
        nullCheckExcludePrivate: false,
        boundsCheckMaxDepth: DefaultBoundsCheckMaxDepth,
        ignoredNamespaces: ImmutableArray<string>.Empty);

    /// <summary>
    /// Initializes a new instance of the <see cref="GuardOptions"/> class with validated values.
    /// </summary>
    private GuardOptions(bool nullCheckExcludePrivate, int boundsCheckMaxDepth, ImmutableArray<string> ignoredNamespaces)
    {
        NullCheckExcludePrivate = nullCheckExcludePrivate;
        BoundsCheckMaxDepth = boundsCheckMaxDepth;
        IgnoredNamespaces = ignoredNamespaces;
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

        var options = new GuardOptions(nullCheckExcludePrivate, boundsCheckMaxDepth, ignoredNamespaces);
        return new GuardOptionsParseResult(options, errors.ToImmutable());
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
