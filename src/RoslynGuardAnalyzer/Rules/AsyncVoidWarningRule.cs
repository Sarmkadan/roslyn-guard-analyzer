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
/// Rule that flags async void methods (except event-handler signatures) with Warning severity.
/// Async void methods without proper exception handling can cause unobserved exceptions.
/// </summary>
public static class AsyncVoidWarningRule
{
    /// <summary>
    /// Creates and returns the AsyncVoidWarningRule instance.
    /// </summary>
    public static CustomAnalysisRule Create()
    {
        return CustomRuleBuilder.Create("AVW001", "Async Void Methods Should Be Avoided")
            .For(RuleCategory.AsyncPattern)
            .WithSeverity(SeverityLevel.Warning)
            .WithDescription("Detects async void methods that are not event handlers. Async void methods without proper exception handling can cause unobserved exceptions and should be avoided in favor of returning Task.")
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
        if (string.IsNullOrWhiteSpace(element.ReturnType) || !element.ReturnType.Equals("void", StringComparison.Ordinal))
            return false;

        // Check if it's an event handler (has EventHandler-related attributes)
        if (IsEventHandlerMethod(element))
            return false;

        return true;
    }

    private static bool IsEventHandlerMethod(CodeElement element)
    {
        // Common event handler attribute patterns
        var eventHandlerPatterns = new[]
        {
            "EventHandler",
            "EventArgs",
            "IEventHandler",
            "Handler",
            "Callback"
        };

        foreach (var pattern in eventHandlerPatterns)
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

        return $"Async void method '{methodName}' at {fileLocation} should be avoided. " +
               "Consider changing return type to Task or marking as event handler with appropriate attributes to prevent unobserved exceptions.";
    }
}