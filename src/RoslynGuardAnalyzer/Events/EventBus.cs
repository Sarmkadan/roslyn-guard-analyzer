// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Events;

/// <summary>
/// In-memory implementation of event bus using publish-subscribe pattern.
/// Maintains a registry of subscribers and dispatches events to them.
/// </summary>
public sealed class EventBus : IEventBus
{
    private sealed class Subscription
    {
        public required Type EventType { get; init; }
        public required Delegate Handler { get; init; }
    }

    private readonly List<Subscription> _subscriptions = [];
    private readonly object _lockObject = new();

    /// <summary>
    /// Publishes an event to all registered subscribers asynchronously.
    /// Exceptions from subscribers are logged but don't prevent other subscribers from running.
    /// </summary>
    public async Task PublishAsync(IEvent @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        lock (_lockObject)
        {
            var matching = _subscriptions
                .Where(s => s.EventType.IsAssignableFrom(@event.GetType()))
                .ToList();

            if (matching.Count == 0)
                return; // No subscribers for this event type
        }

        // Execute handlers outside the lock
        foreach (var subscription in GetMatchingSubscriptions(@event.GetType()))
        {
            try
            {
                if (subscription.Handler is Delegate handler)
                {
                    var task = (Task?)handler.DynamicInvoke(@event);
                    if (task != null)
                        await task;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error handling event {subscription.EventType.Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Subscribes to events of a specific type.
    /// Multiple handlers can subscribe to the same event type.
    /// </summary>
    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        lock (_lockObject)
        {
            _subscriptions.Add(new Subscription
            {
                EventType = typeof(TEvent),
                Handler = handler
            });
        }
    }

    /// <summary>
    /// Unsubscribes a handler from events of a specific type.
    /// Removes all matching subscriptions for the handler.
    /// </summary>
    public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        lock (_lockObject)
        {
            _subscriptions.RemoveAll(s =>
                s.EventType == typeof(TEvent) &&
                s.Handler == (Delegate)(object)handler);
        }
    }

    /// <summary>
    /// Gets the count of subscriptions (useful for testing).
    /// </summary>
    public int SubscriptionCount
    {
        get
        {
            lock (_lockObject)
            {
                return _subscriptions.Count;
            }
        }
    }

    /// <summary>
    /// Clears all subscriptions.
    /// </summary>
    public void ClearSubscriptions()
    {
        lock (_lockObject)
        {
            _subscriptions.Clear();
        }
    }

    /// <summary>
    /// Gets subscriptions matching a specific event type.
    /// </summary>
    private List<Subscription> GetMatchingSubscriptions(Type eventType)
    {
        lock (_lockObject)
        {
            return _subscriptions
                .Where(s => s.EventType.IsAssignableFrom(eventType))
                .ToList();
        }
    }
}
