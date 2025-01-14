# JsonFormatterExtensions

The `JsonFormatterExtensions` class provides a set of static extension methods designed to serialize code analysis violation data into structured JSON strings. Located within the `roslyn-guard-analyzer` project, this utility facilitates the consistent formatting of diagnostic results, enabling seamless integration with reporting tools, CI/CD pipelines, and external dashboards that consume JSON payloads. It supports various aggregation strategies, including individual violation formatting, grouping by severity or rule ID, and generating high-level summaries.

## API

### `FormatViolation`
Generates a JSON representation of a single code analysis violation.
*   **Purpose**: Converts an individual violation object into a standardized JSON string.
*   **Parameters**: Accepts the specific violation instance to be serialized (typically an object containing diagnostic details such as location, message, and rule ID).
*   **Return Value**: Returns a `string` containing the JSON representation of the single violation.
*   **Exceptions**: Throws an exception if the input violation object is null or if the serialization process encounters an invalid data state within the violation object.

### `FormatViolationsBySeverity`
Generates a JSON object grouping a collection of violations by their severity level (e.g., Error, Warning, Info).
*   **Purpose**: Aggregates a list of violations and organizes them into a JSON structure where keys represent severity levels and values are arrays of corresponding violations.
*   **Parameters**: Accepts an enumerable collection of violations.
*   **Return Value**: Returns a `string` containing the grouped JSON structure.
*   **Exceptions**: Throws an exception if the input collection is null.

### `FormatViolationsByRule`
Generates a JSON object grouping a collection of violations by their diagnostic rule identifier.
*   **Purpose**: Aggregates a list of violations and organizes them into a JSON structure where keys represent rule IDs and values are arrays of violations associated with that rule.
*   **Parameters**: Accepts an enumerable collection of violations.
*   **Return Value**: Returns a `string` containing the grouped JSON structure.
*   **Exceptions**: Throws an exception if the input collection is null.

### `FormatViolationSummary`
Generates a JSON summary containing statistical metadata about a collection of violations without listing every individual instance.
*   **Purpose**: Produces a lightweight JSON report containing counts and aggregate metrics (e.g., total count, counts per severity) for a given set of violations.
*   **Parameters**: Accepts an enumerable collection of violations.
*   **Return Value**: Returns a `string` containing the summary JSON object.
*   **Exceptions**: Throws an exception if the input collection is null.

## Usage

The following example demonstrates how to format a single diagnostic violation for logging or immediate processing.

```csharp
using RoslynGuardAnalyzer.Extensions;

// Assume 'violation' is an instance of a diagnostic result object
var violation = GetLatestDiagnostic();

if (violation != null)
{
    // Serialize the single violation to JSON
    string jsonOutput = JsonFormatterExtensions.FormatViolation(violation);
    
    System.Console.WriteLine(jsonOutput);
}
```

The following example illustrates aggregating a full list of analysis results by severity before exporting to a file.

```csharp
using RoslynGuardAnalyzer.Extensions;
using System.Collections.Generic;
using System.IO;

// Assume 'allViolations' is a list of diagnostic results from an analysis run
List<Violation> allViolations = RunAnalysis();

// Group and format the violations by severity (Error, Warning, etc.)
string groupedJson = JsonFormatterExtensions.FormatViolationsBySeverity(allViolations);

// Write the formatted JSON to a report file
File.WriteAllText("analysis_report_by_severity.json", groupedJson);
```

## Notes

*   **Null Handling**: All methods expect valid input objects. Passing a `null` collection to the aggregation methods (`FormatViolationsBySeverity`, `FormatViolationsByRule`, `FormatViolationSummary`) or a `null` instance to `FormatViolation` will result in an exception. Callers should ensure data validity prior to invocation.
*   **Thread Safety**: As this class consists entirely of static methods that operate on provided input parameters without maintaining internal mutable state, it is thread-safe. Multiple threads may safely call these methods concurrently with different data sets.
*   **Empty Collections**: Passing an empty collection to the aggregation or summary methods will result in a valid JSON structure representing an empty set (e.g., an empty object `{}` or an object with zero counts), rather than throwing an exception.
*   **Serialization Format**: The output JSON adheres to standard formatting conventions suitable for machine parsing. Property naming and structure are deterministic based on the input violation schema.
