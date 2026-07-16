# Roslyn Guard Analyzer

...

## Architecture

...

## DiagnosticsService

The `DiagnosticsService` class provides diagnostic information about the analyzer and analysis runs. It tracks performance, errors, and usage statistics.

### Usage Example

```csharp
var diagnostics = new DiagnosticsService();

// Record some analysis runs
diagnostics.RecordAnalysis(1500, 5);
diagnostics.RecordAnalysis(2000, 3);
diagnostics.RecordError("Error during analysis");

// Get diagnostic information
Console.WriteLine($"Analysis count: {diagnostics.GetAnalysisCount()}");
Console.WriteLine($"Total violations found: {diagnostics.GetTotalViolationsFound()}");
Console.WriteLine($"Total analysis time: {diagnostics.TotalAnalysisTimeMs}ms");
Console.WriteLine($"Error count: {diagnostics.GetErrorCount()}");
Console.WriteLine($"Recent errors: {string.Join(", ", diagnostics.GetRecentErrors())}");
Console.WriteLine(diagnostics.GenerateDiagnosticReport());

// Reset statistics
diagnostics.Reset();
Console.WriteLine($"Analysis count after reset: {diagnostics.GetAnalysisCount()}");
```

...
