# IMiddleware

The `IMiddleware` interface defines the contract for middleware components within the `roslyn-guard-analyzer` pipeline, providing a standardized context for executing analysis tasks. It exposes essential metadata such as project paths and analysis identifiers, manages state through a flexible items dictionary, tracks execution timing and cancellation status, and defines the delegate signature used to invoke the next component in the chain.

## API

### MiddlewareDelegate
```csharp
public delegate Task MiddlewareDelegate
```
Defines the signature for the next middleware component in the execution pipeline. Implementations invoke this delegate to pass control to the subsequent stage. It returns a `Task` representing the asynchronous operation of the remaining pipeline.

### ProjectPath
```csharp
public required string ProjectPath
```
Gets the absolute file system path to the project being analyzed. This property is mandatory and must be set during the initialization of the middleware context. It serves as the primary reference point for locating source files and project assets.

### AnalysisId
```csharp
public required string AnalysisId
```
Gets the unique identifier assigned to the current analysis run. This property is mandatory and ensures that logs, telemetry, and cached results can be correlated to a specific execution instance.

### Items
```csharp
public Dictionary<string, object> Items
```
Gets the dictionary used for storing arbitrary state data shared across middleware components. Keys are strings, and values are objects. This collection allows middleware to pass complex data structures without modifying the interface signature.

### StartTimeMilliseconds
```csharp
public long StartTimeMilliseconds
```
Gets the timestamp, in milliseconds relative to system startup, when the analysis pipeline began execution. This value is used to calculate total duration and enforce timeouts.

### EndTimeMilliseconds
```csharp
public long EndTimeMilliseconds
```
Gets the timestamp, in milliseconds relative to system startup, when the analysis pipeline completed execution. If the pipeline is still running, this value may be zero or undefined depending on the implementation state.

### ErrorMessage
```csharp
public string? ErrorMessage
```
Gets the error message if the pipeline terminated due to an exception or fatal error. Returns `null` if the execution completed successfully or was cancelled without error.

### IsCancelled
```csharp
public bool IsCancelled
```
Gets a value indicating whether the analysis operation has been requested to cancel. Middleware components should check this property periodically to abort long-running operations gracefully.

### GetElapsedMilliseconds
```csharp
public long GetElapsedMilliseconds<T>()
```
Calculates and returns the elapsed time in milliseconds since the start of the analysis.
*   **Returns**: A `long` representing the duration.
*   **Remarks**: The generic type parameter `T` appears in the signature but typically serves as a marker or constraint in specific implementations; logically, this method computes `EndTimeMilliseconds - StartTimeMilliseconds` or the delta from `StartTimeMilliseconds` to the current time if ongoing.

### GetItem<T>
```csharp
public T? GetItem<T>(string key)
```
Retrieves a strongly-typed value from the `Items` dictionary.
*   **Parameters**:
    *   `key`: The string key associated with the value.
*   **Returns**: The value cast to type `T`, or `default(T)` (usually `null` for reference types) if the key does not exist or the value cannot be cast to `T`.
*   **Throws**: No exceptions are thrown for missing keys or invalid casts; it returns the default value instead.

### SetItem<T>
```csharp
public void SetItem<T>(string key, T value)
```
Stores a strongly-typed value into the `Items` dictionary.
*   **Parameters**:
    *   `key`: The string key to associate with the value.
    *   `value`: The value to store.
*   **Returns**: `void`.
*   **Throws**: May throw `ArgumentNullException` if `key` is null, depending on the underlying dictionary implementation.

## Usage

### Example 1: Basic Middleware Execution and State Sharing
This example demonstrates a middleware component that validates the project path, stores a custom configuration object in the context, and invokes the next delegate.

```csharp
public class ValidationMiddleware : IMiddleware
{
    // Interface properties implementation omitted for brevity
    public string ProjectPath { get; set; } = string.Empty;
    public string AnalysisId { get; set; } = string.Empty;
    public Dictionary<string, object> Items { get; set; } = new();
    public long StartTimeMilliseconds { get; set; }
    public long EndTimeMilliseconds { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsCancelled { get; set; }

    public async Task InvokeAsync(MiddlewareDelegate next)
    {
        if (string.IsNullOrEmpty(ProjectPath))
        {
            ErrorMessage = "Project path is missing.";
            return;
        }

        // Store complex state for downstream middleware
        var config = new AnalysisConfig { Depth = 5, StrictMode = true };
        SetItem("analysis_config", config);

        if (!IsCancelled)
        {
            await next();
        }
    }

    // Helper methods utilizing the interface generics
    private T? GetItem<T>(string key) => /* implementation */ default(T);
    private void SetItem<T>(string key, T value) => Items[key] = value!;
    public long GetElapsedMilliseconds<T>() => EndTimeMilliseconds - StartTimeMilliseconds;
}
```

### Example 2: Timing and Cancellation Handling
This example illustrates how to measure execution time and respect cancellation tokens within a middleware component.

```csharp
public class TimingMiddleware : IMiddleware
{
    // Properties implementation omitted
    public bool IsCancelled { get; set; }
    public long StartTimeMilliseconds { get; set; }
    
    public async Task InvokeAsync(MiddlewareDelegate next)
    {
        var start = StartTimeMilliseconds;
        
        try 
        {
            await next();
        }
        catch (OperationCanceledException)
        {
            IsCancelled = true;
            throw;
        }
        finally
        {
            // Calculate elapsed time using the generic method signature
            // Note: In a concrete class, T might be inferred or specified based on implementation details
            var elapsed = GetElapsedMilliseconds<object>(); 
            
            Console.WriteLine($"Analysis {AnalysisId} completed in {elapsed}ms");
        }
    }

    public long GetElapsedMilliseconds<T>() => System.Environment.TickCount64 - StartTimeMilliseconds;
    // Other members omitted
}
```

## Notes

*   **Thread Safety**: The `Items` dictionary is exposed as a public property. If the pipeline executes middleware concurrently or if background tasks access `Items`, external synchronization (e.g., locking) is required as `Dictionary<TKey, TValue>` is not thread-safe for concurrent writes.
*   **Cancellation Semantics**: The `IsCancelled` property is a flag. Setting it to `true` does not automatically abort running tasks; middleware implementations must explicitly poll this property and exit early.
*   **Generic Type Parameters**: The methods `GetElapsedMilliseconds<T>`, `GetItem<T>`, and `SetItem<T>` utilize generic type parameters. For `GetItem` and `SetItem`, `T` represents the data type being stored. For `GetElapsedMilliseconds`, the presence of `<T>` in the signature suggests it may be used for type-specific timing strategies or interface variance, though logically it returns a scalar time value. Callers should ensure the type provided matches the expected usage pattern of the specific analyzer implementation.
*   **Required Properties**: `ProjectPath` and `AnalysisId` are marked as `required`. Consumers must ensure these are initialized before invoking any middleware logic to avoid runtime initialization errors.
*   **Error Handling**: The `ErrorMessage` property is intended for capturing high-level failure messages. It does not replace exception throwing; unhandled exceptions will likely propagate up the call stack unless caught by the pipeline host.
