using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Events
{
    /// <summary>
    /// Extension methods that make working with <see cref="EventBus"/> more convenient.
    /// </summary>
    public static class EventBusExtensions
    {
        /// <summary>
        /// Publishes a strongly‑typed event using the underlying <see cref="EventBus.PublishAsync"/> method.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to publish.</typeparam>
        /// <param name="bus">The event bus instance.</param>
        /// <param name="event">The event instance to publish.</param>
        /// <returns>A task that completes when the publish operation finishes.</returns>
        public static Task PublishAsync<TEvent>(this EventBus bus, TEvent @event)
        {
            if (bus is null) throw new ArgumentNullException(nameof(bus));
            if (@event is null) throw new ArgumentNullException(nameof(@event));

            // The original EventBus.PublishAsync method is expected to accept the event instance as an argument.
            // We forward the call directly; the compiler will resolve the correct overload.
            return bus.PublishAsync(@event);
        }

        /// <summary>
        /// Subscribes a handler for a specific event type.
        /// </summary>
        /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
        /// <param name="bus">The event bus instance.</param>
        /// <param name="handler">The handler that will be invoked when the event is published.</param>
        public static void Subscribe<TEvent>(this EventBus bus, Action<TEvent> handler)
        {
            if (bus is null) throw new ArgumentNullException(nameof(bus));
            if (handler is null) throw new ArgumentNullException(nameof(handler));

            // Forward to the original generic Subscribe method.
            bus.Subscribe<TEvent>(handler);
        }

        /// <summary>
        /// Unsubscribes a previously registered handler for a specific event type.
        /// </summary>
        /// <typeparam name="TEvent">The event type to unsubscribe from.</typeparam>
        /// <param name="bus">The event bus instance.</param>
        /// <param name="handler">The handler to remove.</param>
        public static void Unsubscribe<TEvent>(this EventBus bus, Action<TEvent> handler)
        {
            if (bus is null) throw new ArgumentNullException(nameof(bus));
            if (handler is null) throw new ArgumentNullException(nameof(handler));

            // Forward to the original generic Unsubscribe method.
            bus.Unsubscribe<TEvent>(handler);
        }

        /// <summary>
        /// Publishes a collection of events sequentially.
        /// </summary>
        /// <param name="bus">The event bus instance.</param>
        /// <param name="events">The events to publish.</param>
        /// <returns>A task that completes when all events have been published.</returns>
        public static async Task PublishAllAsync(this EventBus bus, IEnumerable<object> events)
        {
            if (bus is null) throw new ArgumentNullException(nameof(bus));
            if (events is null) throw new ArgumentNullException(nameof(events));

            // Publish each event one after another to preserve order.
            foreach (var ev in events)
            {
                if (ev is null) continue;
                await bus.PublishAsync(ev).ConfigureAwait(false);
            }
        }
    }
}
