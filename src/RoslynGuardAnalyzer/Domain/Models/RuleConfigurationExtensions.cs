#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using RoslynGuardAnalyzer.Core;

namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="RuleConfiguration"/> to enhance its functionality
/// with common operations and validations.
/// </summary>
public static class RuleConfigurationExtensions
{
    /// <summary>
    /// Determines whether the configuration has any rules that match the specified severity level
    /// or higher.
    /// </summary>
    /// <param name="configuration">The rule configuration.</param>
    /// <param name="severity">The minimum severity level to check for.</param>
    /// <returns><see langword="true"/> if at least one rule has the specified severity or higher; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static bool HasRulesWithSeverityOrHigher(this RuleConfiguration configuration, SeverityLevel severity)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.EnabledRules.Any(r => r.DefaultSeverity >= severity);
    }

    /// <summary>
    /// Gets all rules that are enabled and match the specified severity level or higher.
    /// </summary>
    /// <param name="configuration">The rule configuration.</param>
    /// <param name="severity">The minimum severity level to filter by.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of enabled rules with the specified severity or higher.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<AnalysisRule> GetEnabledRulesWithSeverityOrHigher(this RuleConfiguration configuration, SeverityLevel severity)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.EnabledRules
            .Where(r => r.IsEnabled && r.DefaultSeverity >= severity)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Determines whether the configuration has any custom settings defined.
    /// </summary>
    /// <param name="configuration">The rule configuration.</param>
    /// <returns><see langword="true"/> if the configuration has one or more custom settings; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static bool HasCustomSettings(this RuleConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.CustomSettings.Count > 0;
    }

    /// <summary>
    /// Gets all custom setting keys that start with the specified prefix.
    /// </summary>
    /// <param name="configuration">The rule configuration.</param>
    /// <param name="prefix">The prefix to filter setting keys by.</param>
    /// <returns>An <see cref="IEnumerable{String}"/> of setting keys that start with the prefix.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IEnumerable<string> GetCustomSettingKeysWithPrefix(this RuleConfiguration configuration, string prefix)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(prefix);

        return configuration.CustomSettings.Keys
            .Where(key => key.StartsWith(prefix, StringComparison.Ordinal));
    }
}