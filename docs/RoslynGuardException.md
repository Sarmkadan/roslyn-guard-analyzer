# RoslynGuardException

The `RoslynGuardException` serves as the base class for all custom exception types within the `roslyn-guard-analyzer` project, providing a standardized mechanism for reporting errors that occur during analysis, configuration, or file-processing phases. It encapsulates essential diagnostic information, including a unique error code, the timestamp of occurrence, and an extensible list of contextual details, facilitating structured error handling and logging across the analyzer's components.

## API

### RoslynGuardException (Base Class)

*   `string ErrorCode`: Gets or sets the unique code associated with the error type.
*   `DateTime OccurredAt`: Gets the UTC timestamp when the exception was initialized.
*   `List<string> Details`: Gets a collection of supplemental diagnostic messages associated with this exception.
*   `void AddDetail(string detail)`: Adds a new message to the `Details` collection.
*   `override string ToString()`: Returns a string representation of the exception, including the error code, timestamp, and details.

### Derived Exception Classes

*   `RuleNotFoundException`: Indicates that a specified rule could not be located in the registry.
    *   `string RuleId`: The identifier of the missing rule.
*   `AnalysisException`: Represents errors encountered during the core analysis process.
    *   `string? ProjectPath`: The path to the project file currently under analysis, if applicable.
*   `ConfigurationException`: Indicates an issue within the configuration file or settings.
    *   `string? ConfigKey`: The specific configuration key that caused the error, if applicable.
*   `FileAccessException`: Represents errors related to reading or accessing files.
    *   `string FilePath`: The path to the file that could not be accessed.
*   `ParseException`: Indicates an error encountered while parsing a file, often occurring during syntax validation.

## Usage

### Handling Base Exceptions
```csharp
try
{
    // ... analysis logic
}
catch (RoslynGuardException ex)
{
    ex.AddDetail($"Failed at: {DateTime.UtcNow}");
    Logger.LogError($"Error {ex.ErrorCode} at {ex.OccurredAt}: {ex.Message}");
}
```

### Throwing Derived Exceptions
```csharp
if (!ruleRegistry.Contains(ruleId))
{
    throw new RuleNotFoundException("Rule not found")
    {
        ErrorCode = "RG001",
        RuleId = ruleId
    };
}
```

## Notes

*   **Thread Safety**: `RoslynGuardException` and its derived classes are not inherently thread-safe. While the exception objects themselves are typically immutable once thrown, the `Details` list is not protected against concurrent modifications; `AddDetail` should not be called from multiple threads simultaneously.
*   **Extensibility**: Developers should derive from the appropriate specific exception class rather than `RoslynGuardException` directly, when possible, to allow for more granular `catch` blocks.
*   **Serialization**: If using custom serialization for these exceptions, ensure that `ErrorCode`, `OccurredAt`, and all derived class properties are explicitly serialized to maintain diagnostic context across process boundaries.
