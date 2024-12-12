# RuleViolation

`RuleViolation` is an immutable data class representing a violation of a static analysis rule in a C# codebase. It captures detailed diagnostic information including the rule identity, location in source code, severity, and additional metadata for reporting and remediation purposes.

## API

### `public string Id`
A unique identifier for this violation instance, typically a GUID or hash derived from the rule and location.

### `public string RuleId`
The identifier of the violated rule (e.g., "CA1822", "RGA0001").

### `public string RuleName`
A human-readable name for the violated rule (e.g., "Mark members as static").

### `public string Message`
The diagnostic message describing the violation.

### `public SeverityLevel Severity`
The severity level of the violation (e.g., Error, Warning, Info).

### `public string FilePath`
The absolute or project-relative path to the source file containing the violation.

### `public int LineNumber`
The 1-based line number where the violation occurs.

### `public int ColumnNumber`
The 1-based column number where the violation occurs.

### `public string? CodeSnippet`
An optional snippet of code surrounding the violation location, used for display in diagnostics.

### `public string? SuggestedFix`
An optional suggested code fix or correction for the violation.

### `public DateTime DetectedAt`
The timestamp when the violation was detected during analysis.

### `public string? ProjectName`
The name of the project containing the violated code, if applicable.

### `public RuleCategory Category`
The category or group to which the violated rule belongs (e.g., Design, Performance).

### `public Dictionary<string, string> Metadata`
A dictionary of additional key-value pairs associated with the violation for extensibility.

### `public RuleViolation(string id, string ruleId, string ruleName, string message, SeverityLevel severity, string filePath, int lineNumber, int columnNumber, string? codeSnippet, string? suggestedFix, DateTime detectedAt, string? projectName, RuleCategory category, Dictionary<string, string>? metadata = null)`
Constructs a new `RuleViolation` instance with the specified diagnostic details. The `metadata` parameter is optional; if `null`, an empty dictionary is used.

### `public RuleViolation WithMetadata(Dictionary<string, string> metadata)`
Returns a new `RuleViolation` with the provided metadata merged into the existing metadata. Existing keys are overwritten.

### `public string GetFormattedLocation()`
Returns a formatted string representing the violation location in the format:
`"FilePath(LineNumber,ColumnNumber)"`.
Example: `"/src/Program.cs(42,10)"`

### `public string GetFullDescription()`
Returns a concatenated description combining rule identity, message, and location:
`"[{RuleId}] {Message} at {FilePath}({LineNumber},{ColumnNumber})"`

### `public void AddMetadata(string key, string value)`
Adds or updates a metadata entry with the specified key and value.

### `public string? GetMetadata(string key)`
Retrieves the value associated with the specified metadata key, or `null` if the key does not exist.

## Usage
