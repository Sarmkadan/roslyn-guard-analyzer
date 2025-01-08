# CsvFormatter

The `CsvFormatter` is a utility class designed to convert Roslyn analyzer violation reports and results into CSV-formatted strings. It provides methods to format analysis outcomes, violations, and summary reports into a structured, machine-readable format suitable for logging, reporting, or further processing.

## API

### `public bool CanFormat`

Determines whether the formatter can produce a valid CSV output for the given input.

- **Return value**
  Returns `true` if the formatter can generate a CSV string; otherwise, returns `false`.
- **Remarks**
  This method does not throw exceptions under normal operation. It evaluates internal state or input validity to determine formatting capability.

---

### `public string FormatResult`

Formats the analyzer execution result into a CSV string.

- **Return value**
  A CSV-formatted string representing the analyzer result, including metadata such as success status, duration, or summary counts.
- **Remarks**
  Returns an empty string if formatting is not possible (see `CanFormat`). The output includes a header row and one or more data rows depending on the result content.

---

### `public string FormatViolations`

Converts a collection of diagnostic violations into a CSV-formatted string.

- **Return value**
  A CSV-formatted string containing violation details such as rule ID, severity, location, message, and file path.
- **Remarks**
  Returns an empty string if no violations are provided or if formatting is not supported. The output includes a header row with columns: `RuleId`, `Severity`, `FilePath`, `Line`, `Column`, `Message`.

---
### `public string FormatReport`

Generates a summary report of analyzer execution in CSV format.

- **Return value**
  A CSV-formatted string summarizing the analysis, including total violations, files analyzed, and other aggregate metrics.
- **Remarks**
  Returns an empty string if no data is available for reporting. The output includes a header row and a single data row with summary statistics.

## Usage
