# RoslynGuardExceptionExtensions

`RoslynGuardExceptionExtensions` provides a set of static extension methods for the `Exception` class, designed to facilitate error reporting, summary generation, and critical status checking within the `roslyn-guard-analyzer` framework. These utilities allow developers to convert raw exceptions into actionable diagnostic data and determine if an exception warrants halting the analysis pipeline.

## API

### FormatErrorReport
Generates a detailed, formatted error report string suitable for logging or user display.
*   **Parameters:** `Exception ex` - The exception to format.
*   **Return Value:** `string` - A structured, readable representation of the exception, including message, stack trace, and relevant metadata.
*   **Throws:** `ArgumentNullException` if `ex` is null.

### ToErrorSummary
Generates a concise, one-line summary of the exception.
*   **Parameters:** `Exception ex` - The exception to summarize.
*   **Return Value:** `string` - A brief description of the error, useful for high-level logging or status updates.
*   **Throws:** `ArgumentNullException` if `ex` is null.

### IsCritical
Determines whether the given exception represents a critical failure that should halt the analysis process.
*   **Parameters:** `Exception ex` - The exception to evaluate.
*   **Return Value:** `bool` - `true` if the exception is critical; otherwise, `false`.
*   **Throws:** `ArgumentNullException` if `ex` is null.

### ToPropertyDictionary
Converts exception properties into a dictionary for serialization or structured logging.
*   **Parameters:** `Exception ex` - The exception to convert.
*   **Return Value:** `Dictionary<string, object?>` - A dictionary where keys are property names and values are the corresponding property values.
*   **Throws:** `ArgumentNullException` if `ex` is null.

## Usage

### Example 1: Logging an Error During Analysis
```csharp
try
{
    // Execute analysis
    await _analysisService.AnalyzeProjectAsync(path);
}
catch (Exception ex)
{
    if (ex.IsCritical())
    {
        _logger.LogCritical("Critical analysis failure: {Summary}", ex.ToErrorSummary());
        throw; // Halt the process
    }

    _logger.LogError("Analysis error: {Report}", ex.FormatErrorReport());
}
```

### Example 2: Structured Logging of Exceptions
```csharp
try
{
    // ...
}
catch (Exception ex)
{
    var errorProps = ex.ToPropertyDictionary();
    _logger.LogError("An error occurred: {Message}", ex.Message, errorProps);
}
```

## Notes

*   **Edge Cases:** All methods perform null checks and will throw an `ArgumentNullException` if a null `Exception` object is provided.
*   **Thread Safety:** As these are static extension methods that operate purely on the provided exception instance and do not modify any shared mutable state, they are thread-safe.
*   **Compatibility:** Designed for use with standard .NET `Exception` types. If custom exception types are used within the `roslyn-guard-analyzer`, these methods rely on standard property access, so they remain effective.
