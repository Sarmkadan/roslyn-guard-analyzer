#nullable enable

using System.Collections.Immutable;

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// The outcome of <see cref="GuardOptions.Parse"/>: the resulting options and any validation errors.
/// </summary>
/// <param name="Options">The options, with defaults substituted for invalid keys.</param>
/// <param name="Errors">One entry per invalid configuration value, in key order.</param>
public sealed record GuardOptionsParseResult(GuardOptions Options, ImmutableArray<GuardConfigurationError> Errors)
{
    /// <summary>
    /// Gets a value indicating whether any configuration value was invalid.
    /// </summary>
    public bool HasErrors => Errors.Length > 0;
}
