# JsonFormatter
The `JsonFormatter` type is designed to handle the formatting of JSON data in the context of the roslyn-guard-analyzer project. It provides properties to access formatted results, violations, and reports, which can be utilized to present data in a structured and readable manner.

## API
* `public bool CanFormat`: Indicates whether formatting is possible. This property does not take any parameters and returns a boolean value. It does not throw any exceptions.
* `public string FormatResult`: Provides the formatted result as a string. This property does not take any parameters and returns a string value. It does not throw any exceptions.
* `public string FormatViolations`: Returns the formatted violations as a string. This property does not take any parameters and returns a string value. It does not throw any exceptions.
* `public string FormatReport`: Gives the formatted report as a string. This property does not take any parameters and returns a string value. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `JsonFormatter` type:
```csharp
// Example 1: Basic usage
JsonFormatter formatter = new JsonFormatter();
if (formatter.CanFormat)
{
    string result = formatter.FormatResult;
    string violations = formatter.FormatViolations;
    string report = formatter.FormatReport;
    Console.WriteLine(result);
    Console.WriteLine(violations);
    Console.WriteLine(report);
}

// Example 2: Using formatted data in a web application
JsonFormatter jsonFormatter = new JsonFormatter();
string formattedResult = jsonFormatter.FormatResult;
string formattedViolations = jsonFormatter.FormatViolations;
string formattedReport = jsonFormatter.FormatReport;
// Use the formatted strings to display data in a web page
```

## Notes
When using the `JsonFormatter` type, consider the following:
- The `CanFormat` property should be checked before attempting to access the formatted data to avoid potential issues.
- The formatted strings returned by `FormatResult`, `FormatViolations`, and `FormatReport` can be quite large, so memory usage should be taken into account when handling these values.
- The `JsonFormatter` type does not appear to have any inherent thread-safety issues, as it does not seem to maintain any internal state that could be modified by multiple threads. However, the usage of its properties in a multithreaded environment should still be carefully considered to avoid any potential synchronization issues.
