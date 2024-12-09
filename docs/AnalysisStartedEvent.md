# AnalysisStartedEvent

The `AnalysisStartedEvent` class represents the event payload raised when an analysis run begins. It carries metadata about the project being analyzed, a unique identifier for the analysis session, and an optional reference to the configuration file that was loaded. This event is typically used to log or track the start of an analysis and to correlate subsequent completion or failure events.

## API

### Properties

- **`ProjectPath`** (`required string`)  
  Gets the absolute file path of the project being analyzed. This property is required and must be assigned before the event is used.

- **`AnalysisId`** (`required string`)  
  Gets a unique identifier for this analysis session. This value is typically a GUID or another token that allows start and completion events to be correlated.

- **`ConfigFilePath`** (`string?`)  
  Gets the optional path to the configuration file that was loaded for this analysis. May be `null` if no configuration file was used.

### Constructor

- **`AnalysisStartedEvent()`**  
  Initializes a new instance of the `AnalysisStartedEvent` class. The constructor does not throw exceptions. All required properties must be set after construction (e.g., via an object initializer) before the instance is used.

## Usage

### Example 1: Creating and raising the event

```csharp
var startedEvent = new AnalysisStartedEvent
{
    ProjectPath = @"C:\Projects\MyApp\MyApp.csproj",
    AnalysisId = Guid.NewGuid().ToString(),
    ConfigFilePath = @"C:\Projects\MyApp\.roslynator.config"
};

// Raise the event through an event aggregator, logger, or custom dispatcher
EventBus.Publish(startedEvent);
```

### Example 2: Handling the event in a subscriber

```csharp
void OnAnalysisStarted(object sender, AnalysisStartedEvent e)
{
    Console.WriteLine($"Analysis started for project: {e.ProjectPath}");
    Console.WriteLine($"Session ID: {e.AnalysisId}");

    if (e.ConfigFilePath is not null)
    {
        Console.WriteLine($"Using configuration file: {e.ConfigFilePath}");
    }
}
```

## Notes

- **Required members**: The `ProjectPath` and `AnalysisId` properties are marked `required`. If they are not assigned before the instance is used, the runtime throws a `RequiredMemberNotSetException`. Always ensure these properties are set.
- **Nullable configuration**: `ConfigFilePath` may be `null`. Consumers should check for `null` before using the value to avoid `NullReferenceException`.
- **Mutability**: All properties have public setters. While the instance is mutable after construction, it is strongly recommended to treat an `AnalysisStartedEvent` as immutable once it has been raised. Modifying properties after the event has been dispatched can lead to inconsistent state in subscribers.
- **Thread safety**: This type is not thread-safe for concurrent writes. If multiple threads need to access the same instance, external synchronization (e.g., a lock) is required. Typically, instances are created on a single thread and then passed to event handlers, which only read the properties.
- **Constructor validation**: The parameterless constructor performs no input validation. Callers are responsible for ensuring that `ProjectPath` and `AnalysisId` contain meaningful values.
