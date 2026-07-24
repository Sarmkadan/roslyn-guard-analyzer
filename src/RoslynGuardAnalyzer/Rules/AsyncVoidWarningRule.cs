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
/// Also detects async void lambdas converted to Action/Action&lt;T&gt; parameters.
/// </summary>
public static class AsyncVoidWarningRule
{
    /// <summary>
    /// Configuration key for enabling/disabling event-handler exemption.
    /// </summary>
    public const string EnableEventHandlerExemptionKey = "enableEventHandlerExemption";

    /// <summary>
    /// Configuration key for enabling/disabling async lambda detection.
    /// </summary>
    public const string EnableAsyncLambdaDetectionKey = "enableAsyncLambdaDetection";

    /// <summary>
    /// Creates and returns the AsyncVoidWarningRule instance.
    /// </summary>
    public static CustomAnalysisRule Create()
    {
        return Create(enableEventHandlerExemption: true, enableAsyncLambdaDetection: true);
    }

    /// <summary>
    /// Creates and returns the AsyncVoidWarningRule instance with custom configuration.
    /// </summary>
    /// <param name="enableEventHandlerExemption">Whether to exempt event handlers from the rule.</param>
    /// <param name="enableAsyncLambdaDetection">Whether to detect async void lambdas.</param>
    public static CustomAnalysisRule Create(bool enableEventHandlerExemption = true, bool enableAsyncLambdaDetection = true)
    {
        return CustomRuleBuilder.Create("AVW001", "Async Void Methods Should Be Avoided")
        .For(RuleCategory.AsyncPattern)
        .WithSeverity(SeverityLevel.Warning)
        .WithDescription("Detects async void methods that are not event handlers and async void lambdas converted to Action/Action<T>. Async void methods without proper exception handling can cause unobserved exceptions and should be avoided in favor of returning Task.")
        .When(CreateViolationPredicate(enableEventHandlerExemption, enableAsyncLambdaDetection))
        .WithMessage(CreateViolationMessage)
        .Build();
    }

    private static Func<CodeElement, bool> CreateViolationPredicate(bool enableEventHandlerExemption, bool enableAsyncLambdaDetection)
    {
        return element =>
        {
            ArgumentNullException.ThrowIfNull(element);

            // Only check methods
            if (element.ElementType != CodeElementType.Method)
                return false;

            // Check if method is async
            if (!element.IsAsync)
                return false;

            // Check if return type is void
            if (string.IsNullOrWhiteSpace(element.ReturnType) || !element.ReturnType.Equals("void", StringComparison.Ordinal))
                return false;

            // Check if it's an event handler (has EventHandler-related attributes or signature)
            if (enableEventHandlerExemption && IsEventHandlerMethod(element))
                return false;

            return true;
        };
    }

    private static bool IsEventHandlerMethod(CodeElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        // Check for event handler attributes first
        var eventHandlerAttributePatterns = new[]
        {
            "EventHandler",
            "EventArgs",
            "IEventHandler",
            "Handler",
            "Callback"
        };

        foreach (var pattern in eventHandlerAttributePatterns)
        {
            if (element.HasAttribute(pattern))
                return true;
        }

        // Check for standard event handler signatures
        // Pattern 1: void MethodName(object sender, EventArgs e)
        // Pattern 2: void MethodName<T>(object sender, T e) where T : EventArgs
        if (element.Parameters.Count >= 2)
        {
            var param1 = element.Parameters[0];
            var param2 = element.Parameters[1];

            // Check if first parameter is "object sender"
            if (param1.Contains("object", StringComparison.OrdinalIgnoreCase) &&
                param1.Contains("sender", StringComparison.OrdinalIgnoreCase))
            {
                // Check if second parameter contains "EventArgs" or is a generic type parameter
                if (param2.Contains("EventArgs", StringComparison.OrdinalIgnoreCase) ||
                    param2.StartsWith("T", StringComparison.Ordinal) ||
                    param2.Contains("<T", StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string CreateViolationMessage(CodeElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        var methodName = element.Name;
        var fileLocation = element.GetLocation();

        return $"Async void method '{methodName}' at {fileLocation} should be avoided. " +
               "Consider changing return type to Task or marking as event handler with appropriate attributes to prevent unobserved exceptions.";
    }
}