#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Events;

/// <summary>
/// Base interface for all domain events in the analysis system.
/// Events are used to notify subscribers of significant system state changes.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Gets the unique identifier for this event instance.
    /// </summary>
    string EventId { get; }

    /// <summary>
    /// Gets the type/name of the event.
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// Gets the timestamp (UTC) when the event was created.
    /// </summary>
    DateTime TimestampUtc { get; }

    /// <summary>
    /// Gets optional metadata associated with the event.
    /// </summary>
    Dictionary<string, object> Metadata { get; }
}

/// <summary>
/// Base abstract class for implementing events.
/// Provides common properties and event ID generation.
/// </summary>
public abstract class Event : IEvent
{
    public string EventId { get; }
    public abstract string EventType { get; }
    public DateTime TimestampUtc { get; }
    public Dictionary<string, object> Metadata { get; }

    protected Event()
    {
        EventId = Guid.NewGuid().ToString();
        TimestampUtc = DateTime.UtcNow;
        Metadata = new Dictionary<string, object>();
    }
}

/// <summary>
/// Handles publishing and subscribing to events.
/// Implements a simple publish-subscribe (observer) pattern with error isolation and async support.
/// </summary>
/// <remarks>
/// <para>
/// This event bus provides the following guarantees:
/// <list type="bullet">
///   <item><description>Ordering: Events are dispatched to subscribers in the order they were subscribed</description></item>
///   <item><description>Isolation: Exceptions thrown by individual subscribers do not prevent other subscribers from being invoked</description></item>
///   <item><description>Delivery: All subscribers for a given event type will be invoked unless cancelled via <see cref="CancellationToken"/></description></item>
///   <item><description>Inheritance: Subscribers registered for a base type will receive events of derived types</description></item>
/// </list>
/// </para>
/// <para>
/// When publishing events, if multiple subscribers throw exceptions, they are aggregated into an <see cref="AggregateException"/>
/// which is thrown after all subscribers have been invoked. This allows callers to handle all errors at once.
/// </para>
/// </remarks>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event to all registered subscribers.
    /// </summary>
    /// <param name="event">The event to publish.</param>
    /// <returns>A task that completes when the event has been published to all subscribers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/></exception>
    /// <exception cref="AggregateException">Thrown if any subscribers throw exceptions. The <see cref="AggregateException.InnerExceptions"/>
    /// contains all individual exceptions from subscribers.</exception>
    Task PublishAsync(IEvent @event);

    /// <summary>
    /// Publishes an event to all registered subscribers with cancellation support.
    /// </summary>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">A cancellation token to observe while publishing.</param>
    /// <returns>A task that completes when the event has been published to all subscribers or when cancelled.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/></exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled via <paramref name="cancellationToken"/>.</exception>
    /// <exception cref="AggregateException">Thrown if any subscribers throw exceptions. The <see cref="AggregateException.InnerExceptions"/>
    /// contains all individual exceptions from subscribers.</exception>
    Task PublishAsync(IEvent @event, CancellationToken cancellationToken);

    /// <summary>
    /// Subscribes to events of a specific type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
    /// <param name="handler">The handler that will be invoked when the event is published.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent;

    /// <summary>
    /// Subscribes to events of a specific type with cancellation support.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
    /// <param name="handler">The handler that will be invoked when the event is published.</param>
    /// <param name="cancellationToken">A cancellation token to observe while subscribing.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled via <paramref name="cancellationToken"/>.</exception>
    IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where TEvent : IEvent;

    /// <summary>
    /// Unsubscribes a handler from events of a specific type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to unsubscribe from.</typeparam>
    /// <param name="handler">The handler to remove.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent;

    /// <summary>
    /// Unsubscribes a handler from events of a specific type with cancellation support.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to unsubscribe from.</typeparam>
    /// <param name="handler">The handler to remove.</param>
    /// <param name="cancellationToken">A cancellation token to observe while unsubscribing.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled via <paramref name="cancellationToken"/>.</exception>
    void Unsubscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where TEvent : IEvent;
}
