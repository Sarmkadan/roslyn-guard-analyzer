#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// Represents an analysis rule backed by predicate-based evaluation logic.
/// </summary>
public sealed class CustomAnalysisRule : AnalysisRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomAnalysisRule"/> class.
    /// </summary>
    public CustomAnalysisRule(
        string id,
        string name,
        string description,
        RuleCategory category,
        SeverityLevel severity,
        Func<CodeElement, bool> violationPredicate,
        Func<CodeElement, string> messageFactory)
        : base(id, name, description, category)
    {
        ViolationPredicate = violationPredicate ?? throw new ArgumentNullException(nameof(violationPredicate));
        MessageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
        DefaultSeverity = severity;
    }

    /// <summary>
    /// Gets the predicate used to determine whether a code element violates the rule.
    /// </summary>
    public Func<CodeElement, bool> ViolationPredicate { get; }

    /// <summary>
    /// Gets the factory used to generate the violation message for a matching code element.
    /// </summary>
    public Func<CodeElement, string> MessageFactory { get; }
}

/// <summary>
/// Fluent builder for creating <see cref="CustomAnalysisRule"/> instances.
/// </summary>
public sealed class CustomRuleBuilder
{
    private readonly string _id;
    private readonly string _name;
    private RuleCategory _category = RuleCategory.CodeStructure;
    private SeverityLevel _severity = SeverityLevel.Warning;
    private string _description = string.Empty;
    private Func<CodeElement, bool>? _predicate;
    private Func<CodeElement, string>? _messageFactory;

    private CustomRuleBuilder(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Rule identifier cannot be null or empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rule name cannot be null or empty.", nameof(name));

        _id = id;
        _name = name;
    }

    /// <summary>
    /// Starts a new custom rule definition.
    /// </summary>
    public static CustomRuleBuilder Create(string id, string name)
    {
        return new CustomRuleBuilder(id, name);
    }

    /// <summary>
    /// Sets the category for the custom rule.
    /// </summary>
    public CustomRuleBuilder For(RuleCategory category)
    {
        _category = category;
        return this;
    }

    /// <summary>
    /// Sets the severity assigned to violations created by the rule.
    /// </summary>
    public CustomRuleBuilder WithSeverity(SeverityLevel severity)
    {
        _severity = severity;
        return this;
    }

    /// <summary>
    /// Sets the rule description.
    /// </summary>
    public CustomRuleBuilder WithDescription(string description)
    {
        _description = description ?? string.Empty;
        return this;
    }

    /// <summary>
    /// Defines the predicate that identifies violating elements.
    /// </summary>
    public CustomRuleBuilder When(Func<CodeElement, bool> predicate)
    {
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        return this;
    }

    /// <summary>
    /// Uses a constant violation message for all matching elements.
    /// </summary>
    public CustomRuleBuilder WithMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));

        _messageFactory = _ => message;
        return this;
    }

    /// <summary>
    /// Uses a message factory that can customize the message per element.
    /// </summary>
    public CustomRuleBuilder WithMessage(Func<CodeElement, string> messageFactory)
    {
        _messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
        return this;
    }

    /// <summary>
    /// Builds the configured custom rule.
    /// </summary>
    public CustomAnalysisRule Build()
    {
        if (_predicate is null)
            throw new InvalidOperationException("A violation predicate must be configured before building the rule.");

        var messageFactory = _messageFactory ?? (element => $"Rule '{_name}' was violated by '{element.Name}'.");
        var description = string.IsNullOrWhiteSpace(_description) ? _name : _description;

        return new CustomAnalysisRule(_id, _name, description, _category, _severity, _predicate, messageFactory);
    }
}
