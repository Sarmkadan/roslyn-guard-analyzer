#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Events;

/// <summary>
/// In-memory implementation of event bus using publish-subscribe pattern.
/// Maintains a registry of subscribers and dispatches events to them.
/// </summary>
public sealed class EventBus : IEventBus
{
    private sealed class Subscription : IDisposable
    {
        public required Type EventType { get; init; }
        public required Delegate Handler { get; init; }
        public required Action UnsubscribeAction { get; init; }

        public void Dispose() => UnsubscribeAction();
    }

    private readonly List<Subscription> _subscriptions = [];
    private readonly object _lockObject = new();

    /// <summary>
    /// Publishes an event to all registered subscribers asynchronously.
    /// </summary>
    /// <param name="event">The event to publish.</param>
    /// <returns>A task that completes when the event has been published to all subscribers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/></exception>
    /// <exception cref="AggregateException">Thrown if any subscribers throw exceptions. The <see cref="AggregateException.InnerExceptions"/>
    /// contains all individual exceptions from subscribers.</exception>
    public async Task PublishAsync(IEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var exceptions = new List<Exception>();
        List<Subscription> matchingSubscriptions;

        // Take a snapshot of subscriptions to prevent issues when unsubscribing during dispatch
        lock (_lockObject)
        {
            matchingSubscriptions = _subscriptions
                .Where(s => s.EventType.IsAssignableFrom(@event.GetType()))
                .ToList();
        }

        if (matchingSubscriptions.Count == 0)
            return; // No subscribers for this event type

        // Execute handlers outside the lock and collect exceptions
        foreach (var subscription in matchingSubscriptions)
        {
            try
            {
                if (subscription.Handler is Delegate handler)
                {
                    var task = (Task?)handler.DynamicInvoke(@event);
                    if (task is not null)
                        await task;
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        // Throw aggregated exceptions if any occurred
        if (exceptions.Count > 0)
        {
            throw new AggregateException(
                $"One or more subscribers failed while handling event {@event.EventType}",
                exceptions);
        }
    }

    /// <summary>
    /// Publishes an event to all registered subscribers asynchronously with cancellation support.
    /// </summary>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">A cancellation token to observe while publishing.</param>
    /// <returns>A task that completes when the event has been published to all subscribers or when cancelled.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/></exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled via <paramref name="cancellationToken"/>.</exception>
    /// <exception cref="AggregateException">Thrown if any subscribers throw exceptions. The <see cref="AggregateException.InnerExceptions"/>
    /// contains all individual exceptions from subscribers.</exception>
    public async Task PublishAsync(IEvent @event, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        cancellationToken.ThrowIfCancellationRequested();

        var exceptions = new List<Exception>();
        List<Subscription> matchingSubscriptions;

        // Take a snapshot of subscriptions to prevent issues when unsubscribing during dispatch
        lock (_lockObject)
        {
            matchingSubscriptions = _subscriptions
                .Where(s => s.EventType.IsAssignableFrom(@event.GetType()))
                .ToList();
        }

        if (matchingSubscriptions.Count == 0)
            return; // No subscribers for this event type

        // Execute handlers outside the lock and collect exceptions
        foreach (var subscription in matchingSubscriptions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (subscription.Handler is Delegate handler)
                {
                    var task = (Task?)handler.DynamicInvoke(@event);
                    if (task is not null)
                        await task;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        // Throw aggregated exceptions if any occurred
        if (exceptions.Count > 0)
        {
            throw new AggregateException(
                $"One or more subscribers failed while handling event {@event.EventType}",
                exceptions);
        }
    }

    /// <summary>
    /// Subscribes to events of a specific type.
    /// Multiple handlers can subscribe to the same event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
    /// <param name="handler">The handler that will be invoked when the event is published.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe the handler.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    public IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        Subscription subscription;
        lock (_lockObject)
        {
            subscription = new Subscription
            {
                EventType = typeof(TEvent),
                Handler = handler,
                UnsubscribeAction = () => Unsubscribe(handler)
            };
            _subscriptions.Add(subscription);
        }

        return subscription;
    }

    /// <summary>
    /// Subscribes to events of a specific type with cancellation support.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
    /// <param name="handler">The handler that will be invoked when the event is published.</param>
    /// <param name="cancellationToken">A cancellation token to observe while subscribing.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe the handler.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled via <paramref name="cancellationToken"/>.</exception>
    public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);
        cancellationToken.ThrowIfCancellationRequested();

        Subscription subscription;
        lock (_lockObject)
        {
            subscription = new Subscription
            {
                EventType = typeof(TEvent),
                Handler = handler,
                UnsubscribeAction = () => Unsubscribe(handler, cancellationToken)
            };
            _subscriptions.Add(subscription);
        }

        return subscription;
    }

    /// <summary>
    /// Unsubscribes a handler from events of a specific type.
    /// Removes all matching subscriptions for the handler.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to unsubscribe from.</typeparam>
    /// <param name="handler">The handler to remove.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_lockObject)
        {
            _subscriptions.RemoveAll(s =>
                s.EventType == typeof(TEvent) &&
                s.Handler == (Delegate)(object)handler);
        }
    }

    /// <summary>
    /// Unsubscribes a handler from events of a specific type with cancellation support.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to unsubscribe from.</typeparam>
    /// <param name="handler">The handler to remove.</param>
    /// <param name="cancellationToken">A cancellation token to observe while unsubscribing.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled via <paramref name="cancellationToken"/>.</exception>
    public void Unsubscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);
        cancellationToken.ThrowIfCancellationRequested();

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

}