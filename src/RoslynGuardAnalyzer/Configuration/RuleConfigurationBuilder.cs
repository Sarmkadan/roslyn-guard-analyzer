#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Configuration;

/// <summary>
/// Fluent builder for creating rule configurations with type safety.
/// Simplifies creation of rule configurations with sensible defaults.
/// </summary>
public sealed class RuleConfigurationBuilder
{
    private readonly string _ruleName;
    private bool _enabled = true;
    private string _severity = "Medium";
    private readonly Dictionary<string, object> _parameters = [];
    private string _description = string.Empty;

    public RuleConfigurationBuilder(string ruleName)
    {
        if (string.IsNullOrWhiteSpace(ruleName))
            throw new ArgumentException("Rule name cannot be null or empty", nameof(ruleName));

        _ruleName = ruleName;
    }

    /// <summary>
    /// Sets whether the rule is enabled.
    /// </summary>
    public RuleConfigurationBuilder WithEnabled(bool enabled)
    {
        _enabled = enabled;
        return this;
    }

    /// <summary>
    /// Sets the severity level for violations.
    /// </summary>
    public RuleConfigurationBuilder WithSeverity(string severity)
    {
        var validSeverities = new[] { "Low", "Medium", "High", "Critical" };
        if (!validSeverities.Contains(severity, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Invalid severity: {severity}", nameof(severity));

        _severity = severity;
        return this;
    }

    /// <summary>
    /// Adds a configuration parameter.
    /// </summary>
    public RuleConfigurationBuilder WithParameter(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Parameter key cannot be null or empty", nameof(key));

        _parameters[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple configuration parameters at once.
    /// </summary>
    public RuleConfigurationBuilder WithParameters(Dictionary<string, object> parameters)
    {
        if (parameters is not null)
        {
            foreach (var (key, value) in parameters)
                WithParameter(key, value);
        }

        return this;
    }

    /// <summary>
    /// Sets the description of the rule.
    /// </summary>
    public RuleConfigurationBuilder WithDescription(string description)
    {
        _description = description ?? string.Empty;
        return this;
    }

    /// <summary>
    /// Builds and returns the rule configuration.
    /// </summary>
    public RuleConfiguration Build()
    {
        var config = new RuleConfiguration
        {
            RuleName = _ruleName,
            IsEnabled = _enabled,
            Severity = _severity,
            Description = _description
        };

        foreach (var (key, value) in _parameters)
            config.SetParameter(key, value);

        return config;
    }

    /// <summary>
    /// Creates a builder with common naming convention rule settings.
    /// </summary>
    public static RuleConfigurationBuilder CreateNamingConvention()
    {
        return new RuleConfigurationBuilder("NamingConvention")
            .WithSeverity("Medium")
            .WithDescription("Enforces C# naming conventions")
            .WithParameter("CheckPublicMembers", true)
            .WithParameter("CheckPrivateMembers", false)
            .WithParameter("CheckConstants", true);
    }

    /// <summary>
    /// Creates a builder with common layer dependency rule settings.
    /// </summary>
    public static RuleConfigurationBuilder CreateLayerDependency()
    {
        return new RuleConfigurationBuilder("LayerDependency")
            .WithSeverity("High")
            .WithDescription("Enforces architectural layer dependencies")
            .WithParameter("StrictMode", false);
    }

    /// <summary>
    /// Creates a builder with common async pattern rule settings.
    /// </summary>
    public static RuleConfigurationBuilder CreateAsyncPatterns()
    {
        return new RuleConfigurationBuilder("AsyncPatterns")
            .WithSeverity("Medium")
            .WithDescription("Validates async/await usage patterns")
            .WithParameter("RequireAsyncSuffix", true)
            .WithParameter("AllowBlockingCalls", false);
    }

    /// <summary>
    /// Creates a builder with common null safety rule settings.
    /// </summary>
    public static RuleConfigurationBuilder CreateNullSafety()
    {
        return new RuleConfigurationBuilder("NullSafety")
            .WithSeverity("High")
            .WithDescription("Enforces null safety patterns")
            .WithParameter("RequireNullChecks", true)
            .WithParameter("AllowNullForgiving", false);
    }
}
