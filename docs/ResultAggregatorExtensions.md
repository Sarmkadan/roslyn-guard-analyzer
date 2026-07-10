# ResultAggregatorExtensions

Provides extension methods for aggregating and querying rule violations collected during static analysis. These methods simplify the process of collecting, grouping, and counting violations across different dimensions (files, rules, severity levels) for reporting or further processing.

## API

### `AddRange(this ResultAggregator aggregator, IEnumerable<RuleViolation> violations)`

Adds a sequence of rule violations to the aggregator.

- **Parameters**
  - `violations`: The sequence of `RuleViolation` instances to add.
- **Exceptions**
  - Throws `ArgumentNullException` if `violations` is `null`.

### `GetTotalViolations(this ResultAggregator aggregator)`

Gets the total number of violations currently aggregated.

- **Returns**
  - The total count of violations.
- **Exceptions**
  - None.

### `GetViolationsByFile(this ResultAggregator aggregator)`

Groups violations by the file path where they occurred.

- **Returns**
  - A dictionary mapping file paths to lists of violations in those files.
- **Exceptions**
  - None.

### `GetViolationsByRule(this ResultAggregator aggregator)`

Groups violations by the rule identifier that triggered them.

- **Returns**
  - A dictionary mapping rule IDs to lists of violations for those rules.
- **Exceptions**
  - None.

### `GetViolationsBySeverity(this ResultAggregator aggregator)`

Groups violations by their severity level.

- **Returns**
  - A dictionary mapping severity levels to lists of violations at those levels.
- **Exceptions**
  - None.

## Usage

```csharp
// Example 1: Aggregating violations and reporting totals
var aggregator = new ResultAggregator();
var violations = AnalyzeProject(projectPath);
aggregator.AddRange(violations);

Console.WriteLine($"Total violations: {aggregator.GetTotalViolations()}");

// Example 2: Grouping violations by file and printing per-file counts
var violationsByFile = aggregator.GetViolationsByFile();
foreach (var fileGroup in violationsByFile)
{
    Console.WriteLine($"{fileGroup.Key}: {fileGroup.Value.Count} violations");
}
```

## Notes

- The aggregator is not thread-safe; concurrent modifications or queries from multiple threads will lead to undefined behavior.
- If no violations have been added, grouping methods return empty dictionaries rather than `null`.
- The `AddRange` method does not validate individual violations; invalid or malformed violations may cause downstream issues during grouping or reporting.
