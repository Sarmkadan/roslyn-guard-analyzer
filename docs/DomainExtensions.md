# DomainExtensions

`DomainExtensions` is a static utility class providing extension and helper methods for analyzing and processing collections of `RuleViolation` objects, as well as general-purpose utilities for string manipulation, collection processing, and severity-based filtering. It supports grouping, filtering, summarization, and export operations commonly required in static analysis tooling workflows.

## API

### `string GetDisplayName(RuleViolation violation)`
Returns a human-readable display name for a given `RuleViolation` by combining its rule ID and message in a standardized format.

- **Parameters**
  - `violation`: The `RuleViolation` instance to format.
- **Returns**
  - A string in the format `"[RuleId] Message"`.
- **Exceptions**
  - Throws `ArgumentNullException` if `violation` is `null`.

---

### `ConsoleColor GetConsoleColor(SeverityLevel severity)`
Maps a `SeverityLevel` to a corresponding `ConsoleColor` for console output styling.

- **Parameters**
  - `severity`: The severity level to map.
- **Returns**
  - A `ConsoleColor` value appropriate for the severity (e.g., `Red` for `Error`, `Yellow` for `Warning`).
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `severity` is not a defined value.

---

### `bool IsBlockingViolation(RuleViolation violation)`
Determines whether a `RuleViolation` represents a blocking issue that should halt further processing.

- **Parameters**
  - `violation`: The violation to check.
- **Returns**
  - `true` if the violation's `SeverityLevel` is `Error` or higher; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `violation` is `null`.

---

### `Dictionary<string, List<RuleViolation>> GroupByFileAndSort(List<RuleViolation> violations)`
Groups a list of violations by source file path and sorts each group by line number.

- **Parameters**
  - `violations`: The list of violations to group.
- **Returns**
  - A dictionary where keys are file paths and values are sorted lists of violations for that file.
- **Exceptions**
  - Throws `ArgumentNullException` if `violations` is `null`.

---

### `List<RuleViolation> FilterBySeverity(List<RuleViolation> violations, SeverityLevel minSeverity)`
Filters a list of violations to include only those at or above a specified severity level.

- **Parameters**
  - `violations`: The list of violations to filter.
  - `minSeverity`: The minimum severity level to include.
- **Returns**
  - A new list containing only violations with severity ≥ `minSeverity`.
- **Exceptions**
  - Throws `ArgumentNullException` if `violations` is `null`.
  - Throws `ArgumentOutOfRangeException` if `minSeverity` is invalid.

---

### `Dictionary<RuleCategory, int> SummarizeByCategory(List<RuleViolation> violations)`
Aggregates violations by their `RuleCategory`, returning a count per category.

- **Parameters**
  - `violations`: The list of violations to summarize.
- **Returns**
  - A dictionary mapping each `RuleCategory` to the number of violations in that category.
- **Exceptions**
  - Throws `ArgumentNullException` if `violations` is `null`.

---
### `Dictionary<SeverityLevel, double> CalculateSeverityPercentages(List<RuleViolation> violations)`
Calculates the percentage distribution of violations across severity levels.

- **Parameters**
  - `violations`: The list of violations to analyze.
- **Returns**
  - A dictionary mapping each `SeverityLevel` to its percentage of the total violations (0–100).
- **Exceptions**
  - Throws `ArgumentNullException` if `violations` is `null`.
  - Throws `InvalidOperationException` if the input list is empty.

---
### `string? GetMostCommonRule(List<RuleViolation> violations)`
Identifies the rule ID that appears most frequently in the list of violations.

- **Parameters**
  - `violations`: The list of violations to analyze.
- **Returns**
  - The rule ID with the highest occurrence count, or `null` if the list is empty or all rules are unique.
- **Exceptions**
  - Throws `ArgumentNullException` if `violations` is `null`.

---
### `string? GetMostProblematicFile(List<RuleViolation> violations)`
Finds the file path with the highest number of violations.

- **Parameters**
  - `violations`: The list of violations to analyze.
- **Returns**
  - The file path with the most violations, or `null` if the list is empty.
- **Exceptions**
  - Throws `ArgumentNullException` if `violations` is `null`.

---
### `string ExportToText(List<RuleViolation> violations)`
Serializes a list of violations into a human-readable text report.

- **Parameters**
  - `violations`: The list of violations to export.
- **Returns**
  - A multi-line string containing formatted violation details, grouped by file and sorted.
- **Exceptions**
  - Throws `ArgumentNullException` if `violations` is `null`.

---
### `List<RuleViolation> GetViolationsAboveSeverity(List<RuleViolation> violations, SeverityLevel threshold)`
Filters violations to include only those with severity strictly greater than a given threshold.

- **Parameters**
  - `violations`: The list of violations to filter.
  - `threshold`: The severity level to compare against.
- **Returns**
  - A new list containing violations with severity > `threshold`.
- **Exceptions**
  - Throws `ArgumentNullException` if `violations` is `null`.
  - Throws `ArgumentOutOfRangeException` if `threshold` is invalid.

---
### `List<RuleViolation> GetViolationsForRule(List<RuleViolation> violations, string ruleId)`
Filters violations to include only those matching a specific rule ID.

- **Parameters**
  - `violations`: The list of violations to filter.
  - `ruleId`: The rule ID to match (case-sensitive).
- **Returns**
  - A new list containing only violations with the specified `ruleId`.
- **Exceptions**
  - Throws `ArgumentNullException` if `violations` is `null` or `ruleId` is `null`.

---
### `string GetSummary(List<RuleViolation> violations)`
Generates a concise summary of violation statistics (total count, by severity, by category).

- **Parameters**
  - `violations`: The list of violations to summarize.
- **Returns**
  - A formatted string with summary statistics.
- **Exceptions**
  - Throws `ArgumentNullException` if `violations` is `null`.

---
### `bool IsAcceptable(List<RuleViolation> violations, SeverityLevel maxSeverity)`
Determines whether the list of violations is acceptable based on a maximum allowed severity.

- **Parameters**
  - `violations`: The list of violations to evaluate.
  - `maxSeverity`: The maximum allowed severity level.
- **Returns**
  - `true` if all violations have severity ≤ `maxSeverity`; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `violations` is `null`.
  - Throws `ArgumentOutOfRangeException` if `maxSeverity` is invalid.

---
### `IEnumerable<(int Index, T Item)> WithIndex<T>(IEnumerable<T> source)`
Enumerates a sequence while including the zero-based index of each element.

- **Parameters**
  - `source`: The sequence to enumerate.
- **Type Parameters**
  - `T`: The type of elements in the sequence.
- **Returns**
  - An enumerable of tuples `(Index, Item)` where `Index` is the position in the source sequence.
- **Exceptions**
  - Throws `ArgumentNullException` if `source` is `null`.

---
### `IEnumerable<List<T>> Batch<T>(IEnumerable<T> source, int size)`
Partitions a sequence into chunks of a specified size.

- **Parameters**
  - `source`: The sequence to partition.
  - `size`: The maximum number of items per batch (must be ≥ 1).
- **Type Parameters**
  - `T`: The type of elements in the sequence.
- **Returns**
  - An enumerable of lists, each containing up to `size` items.
- **Exceptions**
  - Throws `ArgumentNullException` if `source` is `null`.
  - Throws `ArgumentOutOfRangeException` if `size` < 1.

---
### `bool IsNullOrEmpty<T>(IEnumerable<T>? source)`
Determines whether a sequence is `null` or empty.

- **Parameters**
  - `source`: The sequence to check.
- **Type Parameters**
  - `T`: The type of elements in the sequence.
- **Returns**
  - `true` if `source` is `null` or contains no elements; otherwise, `false`.

---
### `IEnumerable<T> DistinctBy<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)`
Returns distinct elements from a sequence based on a key selector function.

- **Parameters**
  - `source`: The sequence to process.
  - `keySelector`: A function to extract the key used for uniqueness.
- **Type Parameters**
  - `T`: The type of elements in the source sequence.
  - `TKey`: The type of the key used for comparison.
- **Returns**
  - An enumerable containing only the first occurrence of each distinct key.
- **Exceptions**
  - Throws `ArgumentNullException` if `source` or `keySelector` is `null`.

---
### `string Truncate(string? input, int maxLength)`
Truncates a string to a specified maximum length, appending an ellipsis if truncated.

- **Parameters**
  - `input`: The string to truncate (may be `null`).
  - `maxLength`: The maximum allowed length (must be ≥ 0).
- **Returns**
  - The truncated string, or the original string if within `maxLength`. Returns `string.Empty` if `input` is `null`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `maxLength` < 0.

---
### `bool IsValidFilePath(string? path)`
Validates whether a string is a syntactically valid file path.

- **Parameters**
  - `path`: The path string to validate (may be `null`).
- **Returns**
  - `true` if the path is valid according to platform conventions; otherwise, `false`.

## Usage
