#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

using RoslynGuardAnalyzer.Core;
namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Represents an architectural rule that can be enforced during code analysis.
/// Contains the definition, configuration, and metadata about a single rule.
/// </summary>
public class AnalysisRule
{
    /// <summary>
    /// Gets or sets the unique identifier of the rule.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the rule.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the rule.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the category to which the rule belongs.
    /// </summary>
    public RuleCategory Category { get; set; }

    /// <summary>
    /// Gets or sets the default severity level of the rule.
    /// </summary>
    public SeverityLevel DefaultSeverity { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the rule is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the optional pattern used by the rule.
    /// </summary>
    public string? RulePattern { get; set; }

    /// <summary>
    /// Gets or sets the rule-specific configuration values.
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; }

    /// <summary>
    /// Gets or sets the URL for the rule documentation.
    /// </summary>
    public string? DocumentationUrl { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the rule was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the rule was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the author of the rule.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets the version of the rule.
    /// </summary>
    public Version? Version { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisRule"/> class.
    /// </summary>
    public AnalysisRule()
    {
        Id = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
        Category = RuleCategory.CodeStructure;
        DefaultSeverity = SeverityLevel.Warning;
        IsEnabled = true;
        Configuration = new Dictionary<string, object>();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisRule"/> class with the specified rule details.
    /// </summary>
    public AnalysisRule(string id, string name, string description, RuleCategory category)
        : this()
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Category = category;
    }

    /// <summary>
    /// Validates the rule configuration and ensures all required settings are present.
    /// </summary>
    /// <returns>True if the rule is valid; otherwise false.</returns>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(Name))
            return false;

        if (Id.Length < 3 || Id.Length > 10)
            return false;

        if (Name.Length < 5 || Name.Length > 100)
            return false;

        return true;
    }

    /// <summary>
    /// Gets a configuration value with a default fallback.
    /// </summary>
    public T? GetConfigurationValue<T>(string key, T? defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Configuration key cannot be null or empty.", nameof(key));

        if (Configuration.TryGetValue(key, out var value))
        {
            return value is T typedValue ? typedValue : defaultValue;
        }

        return defaultValue;
    }

    /// <summary>
    /// Sets a configuration value.
    /// </summary>
    public void SetConfigurationValue<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Configuration key cannot be null or empty.", nameof(key));

        Configuration[key] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Creates a copy of this rule with overridden severity level.
    /// </summary>
    public AnalysisRule WithSeverity(SeverityLevel severity)
    {
        var copy = new AnalysisRule
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Category = Category,
            DefaultSeverity = severity,
            IsEnabled = IsEnabled,
            RulePattern = RulePattern,
            Author = Author,
            Version = Version,
            DocumentationUrl = DocumentationUrl,
            CreatedAt = CreatedAt,
            ModifiedAt = DateTime.UtcNow
        };

        foreach (var kvp in Configuration)
        {
            copy.Configuration[kvp.Key] = kvp.Value;
        }

        return copy;
    }

    /// <summary>
    /// Marks the rule as modified and updates the timestamp.
    /// </summary>
    public void MarkAsModified()
    {
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns a string that identifies the rule by its identifier and name.
    /// </summary>
    public override string ToString() => $"{Id}: {Name}";
}
