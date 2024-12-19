# DiagnosticsService
The DiagnosticsService type is designed to provide diagnostic information and track analysis metrics within the roslyn-guard-analyzer project. It offers a range of properties and methods to record and retrieve analysis data, including counts of analyses performed, violations found, and errors encountered, as well as timing information and system details.

## API
### Properties
- `AnalysisCount`: Gets the total number of analyses performed.
- `TotalViolationsFound`: Gets the total number of violations found across all analyses.
- `TotalAnalysisTimeMs`: Gets the total time spent on analyses in milliseconds.
- `ErrorCount`: Gets the total number of errors encountered.
- `RecentErrors`: Gets a list of recent errors, providing insight into the most current issues.

### Methods
- `RecordAnalysis()`: Records the completion of an analysis, updating internal metrics accordingly.
- `RecordError()`: Records an error, incrementing the error count and potentially updating recent errors.
- `GetAverageAnalysisTime()`: Returns the average time spent on analyses in milliseconds.
- `GetAnalysisCount()`: Returns the total number of analyses performed.
- `GetTotalViolationsFound()`: Returns the total number of violations found.
- `GetErrorCount()`: Returns the total number of errors encountered.
- `GetRecentErrors()`: Returns a list of recent errors.
- `GetSystemInfo()`: Returns a dictionary containing system information.
- `GenerateDiagnosticReport()`: Generates a diagnostic report based on the recorded data.
- `Reset()`: Resets the diagnostic service, clearing all recorded data.

## Usage
The following examples demonstrate how to utilize the DiagnosticsService in a C# application:
```csharp
// Example 1: Basic Usage
var diagnostics = new DiagnosticsService();
diagnostics.RecordAnalysis();
diagnostics.RecordError();
Console.WriteLine($"Analyses: {diagnostics.AnalysisCount}, Errors: {diagnostics.ErrorCount}");
```

```csharp
// Example 2: Advanced Usage
var diagnostics = new DiagnosticsService();
for (int i = 0; i < 10; i++)
{
    diagnostics.RecordAnalysis();
    if (i % 2 == 0)
    {
        diagnostics.RecordError();
    }
}
Console.WriteLine($"Average Analysis Time: {diagnostics.GetAverageAnalysisTime()}ms");
Console.WriteLine($"Recent Errors: {string.Join(", ", diagnostics.GetRecentErrors())}");
```

## Notes
- The `DiagnosticsService` is not thread-safe by default. Access to its members should be synchronized in multi-threaded environments to ensure data integrity.
- The `GetSystemInfo` method may return sensitive information and should be used judiciously, especially in production environments.
- The `Reset` method clears all recorded data. Use this method with caution, as it will erase all diagnostic information collected up to that point.
- The `GenerateDiagnosticReport` method's output format may vary based on the implementation details of the `DiagnosticsService`. Always verify the report's structure when relying on it for automated processing or logging.
