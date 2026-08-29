#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// Rule that flags async void methods that are not event handlers.
/// Async void methods without proper exception handling can cause unobserved exceptions
/// and should only be used for event handlers.
/// </summary>
public static class AsyncVoidRule
{
    public const string RuleId = "AV001";
    public const string RuleTitle = "Async Void Methods Must Be Event Handlers";

    private const string VoidReturnType = "void";

    private static readonly string[] EventHandlerNamePatterns =
    {
        "EventHandler",
        "EventArgs",
        "IEventHandler",
        "Handler",
        "Callback"
    };

    /// <summary>
    /// Creates and returns the AsyncVoidRule instance.
    /// </summary>
    public static CustomAnalysisRule Create()
    {
        return CustomRuleBuilder.Create(RuleId, RuleTitle)
            .For(RuleCategory.AsyncPattern)
            .WithSeverity(SeverityLevel.Error)
            .WithDescription("Detects async void methods that are not event handlers. Async void methods without proper exception handling can cause unobserved exceptions.")
            .When(IsAsyncVoidNonEventHandler)
            .WithMessage(CreateViolationMessage)
            .Build();
    }

    private static bool IsAsyncVoidNonEventHandler(CodeElement element)
    {
        // Only check methods
        if (element.ElementType != CodeElementType.Method)
            return false;

        // Check if method is async
        if (!element.IsAsync)
            return false;

        // Check if return type is void
        if (string.IsNullOrWhiteSpace(element.ReturnType) || !element.ReturnType.Equals(VoidReturnType, StringComparison.Ordinal))
            return false;

        // Check if it's an event handler (has EventHandler-related attributes)
        if (IsEventHandlerMethod(element))
            return false;

        return true;
    }

    private static bool IsEventHandlerMethod(CodeElement element)
    {
        foreach (var pattern in EventHandlerNamePatterns)
        {
            if (element.HasAttribute(pattern))
                return true;
        }

        return false;
    }

    private static string CreateViolationMessage(CodeElement element)
    {
        var methodName = element.Name;
        var fileLocation = element.GetLocation();

        return $"Async void method '{methodName}' at {fileLocation} must be an event handler. " +
               "Consider changing return type to Task or marking as event handler with appropriate attributes.";
    }
}
