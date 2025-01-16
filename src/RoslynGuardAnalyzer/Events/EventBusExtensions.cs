using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Events
{
    /// <summary>
    /// Extension methods that make working with <see cref="IEventBus"/> more convenient.
    /// </summary>
    public static class EventBusExtensions
    {
        /// <summary>
        /// Publishes a strongly-typed event using the underlying <see cref="IEventBus.PublishAsync"/> method.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to publish.</typeparam>
        /// <param name="bus">The event bus instance.</param>
        /// <param name="event">The event instance to publish.</param>
        /// <returns>A task that completes when the publish operation finishes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bus"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/></exception>
        public static Task PublishAsync<TEvent>(this IEventBus bus, TEvent @event) where TEvent : IEvent
            => bus.PublishAsync(@event ?? throw new ArgumentNullException(nameof(@event)));

        /// <summary>
        /// Subscribes a handler for a specific event type.
        /// </summary>
        /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
        /// <param name="bus">The event bus instance.</param>
        /// <param name="handler">The handler that will be invoked when the event is published.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bus"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
        public static void Subscribe<TEvent>(this IEventBus bus, Func<TEvent, Task> handler) where TEvent : IEvent
            => bus.Subscribe(handler ?? throw new ArgumentNullException(nameof(handler)));

        /// <summary>
        /// Unsubscribes a previously registered handler for a specific event type.
        /// </summary>
        /// <typeparam name="TEvent">The event type to unsubscribe from.</typeparam>
        /// <param name="bus">The event bus instance.</param>
        /// <param name="handler">The handler to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bus"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
        public static void Unsubscribe<TEvent>(this IEventBus bus, Func<TEvent, Task> handler) where TEvent : IEvent
            => bus.Unsubscribe(handler ?? throw new ArgumentNullException(nameof(handler)));

        /// <summary>
        /// Publishes a collection of events sequentially.
        /// </summary>
        /// <param name="bus">The event bus instance.</param>
        /// <param name="events">The events to publish.</param>
        /// <returns>A task that completes when all events have been published.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bus"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is <see langword="null"/></exception>
        public static async Task PublishAllAsync(this IEventBus bus, IEnumerable<IEvent> events)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(events);

            // Publish each event one after another to preserve order.
            foreach (var ev in events)
            {
                ArgumentNullException.ThrowIfNull(ev);
                await bus.PublishAsync(ev).ConfigureAwait(false);
            }
        }
    }
}