#nullable enable

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// Describes one invalid guard configuration value.
/// </summary>
/// <param name="Key">The configuration key, for example <c>guard.bounds_check.max_depth</c>.</param>
/// <param name="Value">The raw value as written in configuration.</param>
/// <param name="Reason">A short explanation of what the value must look like.</param>
public sealed record GuardConfigurationError(string Key, string Value, string Reason);
