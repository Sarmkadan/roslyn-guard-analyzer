# EventBusExtensions

Provides extension methods for publishing and subscribing to events through an `IEventBus` abstraction, enabling asynchronous event-driven communication within the analyzer.

## API

### PublishAsync<TEvent>
```csharp
public static Task PublishAsync<TEvent>(this IEventBus bus, TEvent @event)
```
Publishes a single event of type `TEvent` asynchronously.  
- **Parameters**  
  - `bus`: The event bus instance on which to publish.  
  - `@event`: The event instance to publish.  
- **Return value**  
  A `Task` that completes when the event has been delivered to all subscribed handlers.  
- **Exceptions**  
  - `ArgumentNullException` if `bus` or `@event` is `null`.  
  - `InvalidOperationException` if the underlying bus has not been started or is disposed.

### Subscribe<TEvent>
```csharp
public static void Subscribe<TEvent>(this IEventBus bus, Func<TEvent, Task> handler)
```
Registers an asynchronous handler for events of type `TEvent`.  
- **Parameters**  
  - `bus`: The event bus instance to subscribe on.  
  - `handler`: The delegate invoked when an event of type `TEvent` is published.  
- **Return value**  
  None.  
- **Exceptions**  
  - `ArgumentNullException` if `bus` or `handler` is `null`.  
  - `InvalidOperationException` if the bus is disposed.

### Unsubscribe<TEvent>
```csharp
public static void Unsubscribe<TEvent>(this IEventBus bus, Func<TEvent, Task> handler)
```
Removes a previously registered asynchronous handler for events of type `TEvent`.  
- **Parameters**  
  - `bus`: The event bus instance to unsubscribe from.  
  - `handler`: The handler delegate that was previously supplied to `Subscribe`.  
- **Return value**  
  None.  
- **Exceptions**  
  - `ArgumentNullException` if `bus` or `handler` is `null`.  
  - `InvalidOperationException` if the bus is disposed or the handler was not found.

### PublishAllAsync
```csharp
public static Task PublishAllAsync(this IEventBus bus, IEnumerable<object> events)
```
Publishes a collection of events asynchronously. Each event is delivered to its respective subscribers.  
- **Parameters**  
  - `bus`: The event bus instance on which to publish.  
  - `events`: The sequence of event instances to publish.  
- **Return value**  
  A `Task` that completes when all events have been processed.  
- **Exceptions**  
  - `ArgumentNullException` if `bus` or `events` is `null`.  
  - `ArgumentException` if `events` contains a `null` element.  
  - `InvalidOperationException` if the bus is disposed.

## Usage

```csharp
// Example 1: Subscribe to and publish a custom event
public record UserLoggedIn(string UserId);

var bus = new InMemoryEventBus(); // assumes an IEventBus implementation
bus.Subscribe<UserLoggedIn>(async e =>
{
    await Logger.LogAsync($"User {e.UserId} logged in.");
});

await bus.PublishAsync(new UserLoggedIn("alice"));
```

```csharp
// Example 2: Publish a batch of events
var events = new List<object>
{
    new UserLoggedIn("bob"),
    new PermissionGranted("bob", "Read")
};

await bus.PublishAllAsync(events);
```

## Notes
- The extension methods themselves are stateless and thread‑safe; however, thread safety of the underlying `IEventBus` implementation determines the safety of concurrent calls to `PublishAsync`, `PublishAllAsync`, `Subscribe`, and `Unsubscribe`.  
- Subscribing and unsubscribing from the same handler multiple times may result in the handler being invoked multiple times or not being removed, depending on the bus implementation.  
- `PublishAllAsync` does not guarantee ordering of delivery across different event types; ordering is preserved only for events of the same type as they appear in the input sequence.  
- Passing a `null` event or handler will always throw; callers should validate arguments before invoking these extensions.  
- If the bus is disposed, any further call to these methods will throw `InvalidOperationException`; ensure the caller‑derived exceptions as defined by the bus implementation.
