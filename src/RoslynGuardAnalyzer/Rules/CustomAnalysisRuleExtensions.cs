#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// Provides extension methods for <see cref="CustomAnalysisRule"/> to enhance rule configuration
/// and violation reporting capabilities.
/// </summary>
public static class CustomAnalysisRuleExtensions
{
    /// <summary>
    /// Creates a violation predicate that checks if a code element has a specific attribute.
    /// </summary>
    /// <param name="rule">The rule to configure.</param>
    /// <param name="attributeName">The attribute name to check for (case-insensitive).</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rule"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="attributeName"/> is null or empty.</exception>
    public static CustomRuleBuilder WithAttribute(this CustomRuleBuilder rule, string attributeName)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentException.ThrowIfNullOrEmpty(attributeName);

        return rule.When(element => element.HasAttribute(attributeName));
    }

    /// <summary>
    /// Creates a violation predicate that checks if a code element is in a specific namespace.
    /// </summary>
    /// <param name="rule">The rule to configure.</param>
    /// <param name="namespacePrefix">The namespace prefix to check against.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rule"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="namespacePrefix"/> is null or empty.</exception>
    public static CustomRuleBuilder WithNamespace(this CustomRuleBuilder rule, string namespacePrefix)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentException.ThrowIfNullOrEmpty(namespacePrefix);

        return rule.When(element => element.IsInNamespace(namespacePrefix));
    }

    /// <summary>
    /// Creates a violation message that includes the element's location information.
    /// </summary>
    /// <param name="rule">The rule to configure.</param>
    /// <param name="messageTemplate">The message template with placeholders {0} for element name and {1} for location.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rule"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="messageTemplate"/> is null or empty.</exception>
    public static CustomRuleBuilder WithLocationAwareMessage(this CustomRuleBuilder rule, string messageTemplate)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentException.ThrowIfNullOrEmpty(messageTemplate);

        return rule.WithMessage(element => string.Format(
            CultureInfo.InvariantCulture,
            messageTemplate,
            element.GetFullyQualifiedName(),
            element.GetLocation()));
    }

    /// <summary>
    /// Creates a violation predicate that checks if a code element is a container type (class, interface, struct, namespace).
    /// </summary>
    /// <param name="rule">The rule to configure.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rule"/> is null.</exception>
    public static CustomRuleBuilder ForContainerElements(this CustomRuleBuilder rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return rule.When(element => element.IsContainer());
    }

    /// <summary>
    /// Gets the violation predicate from a built rule.
    /// </summary>
    /// <param name="rule">The analysis rule.</param>
    /// <returns>The violation predicate function.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rule"/> is null.</exception>
    public static Func<CodeElement, bool> GetViolationPredicate(this CustomAnalysisRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        return rule.ViolationPredicate;
    }

    /// <summary>
    /// Gets the message factory from a built rule.
    /// </summary>
    /// <param name="rule">The analysis rule.</param>
    /// <returns>The message factory function.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rule"/> is null.</exception>
    public static Func<CodeElement, string> GetMessageFactory(this CustomAnalysisRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        return rule.MessageFactory;
    }

    /// <summary>
    /// Creates a violation predicate that checks if a code element has a specific complexity threshold.
    /// </summary>
    /// <param name="rule">The rule to configure.</param>
    /// <param name="maxComplexity">The maximum allowed complexity (inclusive).</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rule"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxComplexity"/> is negative.</exception>
    public static CustomRuleBuilder WithMaxComplexity(this CustomRuleBuilder rule, int maxComplexity)
    {
        ArgumentNullException.ThrowIfNull(rule);

        if (maxComplexity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxComplexity), maxComplexity, "Max complexity cannot be negative.");
        }

        return rule.When(element => element.Complexity > maxComplexity);
    }

    /// <summary>
    /// Creates a violation predicate that checks if a code element is public and not static.
    /// </summary>
    /// <param name="rule">The rule to configure.</param>
    /// <returns>The rule builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rule"/> is null.</exception>
    public static CustomRuleBuilder ForPublicNonStaticMembers(this CustomRuleBuilder rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return rule.When(element => element.IsPublic && !element.IsStatic);
    }
}