// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
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
/// Implements a simple publish-subscribe (observer) pattern.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event to all registered subscribers.
    /// </summary>
    Task PublishAsync(IEvent @event);

    /// <summary>
    /// Subscribes to events of a specific type.
    /// </summary>
    void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent;

    /// <summary>
    /// Unsubscribes a handler from events of a specific type.
    /// </summary>
    void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent;
}
