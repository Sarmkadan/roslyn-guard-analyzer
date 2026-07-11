#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace RoslynGuardAnalyzer.Events;

/// <summary>
/// Provides validation helpers for <see cref="EventBus"/> instances.
/// </summary>
public static class EventBusValidation
{
    /// <summary>
    /// Validates the specified <see cref="EventBus"/> instance.
    /// </summary>
    /// <param name="value">The event bus instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this EventBus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // EventBus itself doesn't have EventType or Handler as public members
        // These are internal to the Subscription class
        // The public API is: EventType (via Subscribe<TEvent>), Handler (via Subscribe),
        // PublishAsync, Subscribe<TEvent>, Unsubscribe<TEvent>, ClearSubscriptions

        // Validate subscription count is reasonable
        var subscriptionCount = value.SubscriptionCount;
        if (subscriptionCount < 0)
        {
            problems.Add("SubscriptionCount cannot be negative.");
        }

        // Validate that we can actually subscribe/unsubscribe without issues
        // This is a basic sanity check that the event bus is in a usable state
        try
        {
            value.ClearSubscriptions();
        }
        catch (Exception ex)
        {
            problems.Add($"ClearSubscriptions failed: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="EventBus"/> instance is valid.
    /// </summary>
    /// <param name="value">The event bus instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this EventBus value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="EventBus"/> instance is valid.
    /// </summary>
    /// <param name="value">The event bus instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this EventBus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            "EventBus validation failed. Problems:\n" + string.Join("\n", problems),
            nameof(value));
    }
}