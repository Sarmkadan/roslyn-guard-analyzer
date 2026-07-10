# EventBus

The `EventBus` class provides a lightweight, in-memory publish-subscribe mechanism for decoupling event producers from consumers within the `roslyn-guard-analyzer` project. It facilitates asynchronous event dispatching and strongly-typed subscription management, allowing components to react to specific event types without direct dependencies on the publishers.

## API

### `EventType`
```csharp
public required Type EventType
```
A required property that specifies the `System.Type` of the events this subscription entry handles. This property must be initialized during object construction. It serves as the key for routing published events to the correct handlers.

### `Handler`
```csharp
public required Delegate Handler
```
A required property holding the `System.Delegate` instance to be invoked when an event matching `EventType` is published. The delegate signature must be compatible with the event type being handled. This property must be initialized during object construction.

### `PublishAsync`
```csharp
public async Task PublishAsync
```
Asynchronously dispatches an event instance to all subscribed handlers matching the event's type.
*   **Parameters**: Accepts the event object to be published (inferred from usage context).
*   **Return Value**: Returns a `System.Threading.Tasks.Task` that completes when all matching handlers have been invoked.
*   **Exceptions**: May throw exceptions propagated from within the user-defined handler delegates if not handled internally by the implementation.

### `Subscribe<TEvent>`
```csharp
public void Subscribe<TEvent>
```
Registers a handler delegate for a specific event type.
*   **Parameters**: Uses the generic type parameter `TEvent` to identify the event type. The method typically accepts a `Delegate` or `Action<TEvent>`/`Func<TEvent, Task>` as an argument (signature inferred from standard patterns).
*   **Return Value**: `void`.
*   **Exceptions**: Throws if the provided handler is null or if the generic type constraint is violated.

### `Unsubscribe<TEvent>`
```csharp
public void Unsubscribe<TEvent>
```
Removes a previously registered handler for a specific event type.
*   **Parameters**: Uses the generic type parameter `TEvent` to identify the event type. Typically requires the same delegate instance that was passed to `Subscribe`.
*   **Return Value**: `void`.
*   **Exceptions**: May throw if the handler was not found or if arguments are invalid.

### `ClearSubscriptions`
```csharp
public void ClearSubscriptions
```
Removes all registered handlers for all event types, resetting the bus to an empty state.
*   **Parameters**: None.
*   **Return Value**: `void`.
*   **Exceptions**: Generally does not throw unless internal state corruption occurs.

## Usage

### Example 1: Basic Subscription and Publishing
This example demonstrates defining an event record, subscribing to it, and publishing an instance asynchronously.

```csharp
public record AnalysisCompletedEvent(string ProjectName, int ErrorCount);

// Initialization
var eventBus = new EventBus();

// Subscription
eventBus.Subscribe<AnalysisCompletedEvent>(e => 
{
    Console.WriteLine($"Analysis for {e.ProjectName} finished with {e.ErrorCount} errors.");
});

// Publishing
var evt = new AnalysisCompletedEvent("RoslynGuard", 0);
await eventBus.PublishAsync(evt);
```

### Example 2: Dynamic Subscription Management
This example shows how to manage subscription lifecycles by subscribing, performing work, and then explicitly unsubscribing or clearing all subscriptions.

```csharp
// Subscribe to a specific event
EventHandler<DiagnosticEvent> handler = (s, e) => LogDiagnostic(e);
eventBus.Subscribe<DiagnosticEvent>(handler);

// ... perform operations triggering events ...

// Unsubscribe specific handler when no longer needed
eventBus.Unsubscribe<DiagnosticEvent>(handler);

// Alternatively, clear all active subscriptions (e.g., during teardown)
eventBus.ClearSubscriptions();
```

## Notes

*   **Thread Safety**: The presence of `async Task PublishAsync` alongside synchronous `Subscribe` and `Unsubscribe` methods implies that care must be taken when modifying subscriptions concurrently with publishing. If `Subscribe` or `Unsubscribe` is called from a different thread while `PublishAsync` is iterating over handlers, race conditions may occur unless the underlying implementation utilizes concurrent collections or locking mechanisms.
*   **Handler Exceptions**: Since `PublishAsync` is asynchronous, exceptions thrown inside a handler delegate will propagate through the returned `Task`. Callers should wrap `PublishAsync` in try-catch blocks to prevent unobserved task exceptions from crashing the application.
*   **Type Matching**: The `EventType` property and `Subscribe<TEvent>` generic parameter rely on exact type matching. Events published will not trigger handlers subscribed to base classes or interfaces unless the implementation explicitly supports covariance, which is not indicated by the strict `Type` property requirement.
*   **Memory Leaks**: If handlers capture strong references to long-lived objects (e.g., UI controls or static contexts), failing to call `Unsubscribe` or `ClearSubscriptions` may prevent garbage collection of those objects.
