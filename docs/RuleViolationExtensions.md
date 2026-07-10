# RuleViolationExtensions

Extension methods for `RuleViolation` that provide fluent-style modification and inspection capabilities for rule violation instances.

## API

### `public static RuleViolation WithMessage(this RuleViolation violation, string message)`

Creates a new `RuleViolation` instance with the specified message while preserving all other properties from the original violation.

- **Parameters**:
  - `message`: The new message to associate with the violation. Must not be null.
- **Return value**: A new `RuleViolation` instance with the updated message.
- **Throws**: `ArgumentNullException` if `message` is null.

---

### `public static RuleViolation WithLocation(this RuleViolation violation, Location location)`

Creates a new `RuleViolation` instance with the specified location while preserving all other properties from the original violation.

- **Parameters**:
  - `location`: The new location to associate with the violation. Must not be null.
- **Return value**: A new `RuleViolation` instance with the updated location.
- **Throws**: `ArgumentNullException` if `location` is null.

---
### `public static RuleViolation WithSeverity(this RuleViolation violation, DiagnosticSeverity severity)`

Creates a new `RuleViolation` instance with the specified severity while preserving all other properties from the original violation.

- **Parameters**:
  - `severity`: The new severity level to associate with the violation.
- **Return value**: A new `RuleViolation` instance with the updated severity.

---
### `public static RuleViolation WithMetadata(this RuleViolation violation, ImmutableDictionary<string, string> metadata)`

Creates a new `RuleViolation` instance with the specified metadata while preserving all other properties from the original violation.

- **Parameters**:
  - `metadata`: The new metadata dictionary to associate with the violation. Must not be null.
- **Return value**: A new `RuleViolation` instance with the updated metadata.
- **Throws**: `ArgumentNullException` if `metadata` is null.

---
### `public static RuleViolation WithDetectedAt(this RuleViolation violation, DateTimeOffset detectedAt)`

Creates a new `RuleViolation` instance with the specified detection timestamp while preserving all other properties from the original violation.

- **Parameters**:
  - `detectedAt`: The new timestamp indicating when the violation was detected.
- **Return value**: A new `RuleViolation` instance with the updated timestamp.

---
### `public static bool HasCategory(this RuleViolation violation, string category)`

Determines whether the violation has the specified category.

- **Parameters**:
  - `category`: The category to check for. Must not be null.
- **Return value**: `true` if the violation's categories contain the specified category; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `category` is null.

---
### `public static bool HasAnyCategory(this RuleViolation violation, IEnumerable<string> categories)`

Determines whether the violation has any of the specified categories.

- **Parameters**:
  - `categories`: The collection of categories to check against. Must not be null.
- **Return value**: `true` if the violation's categories intersect with the specified collection; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `categories` is null.

---
### `public static string? GetFormattedCodeSnippet(this RuleViolation violation)`

Generates a formatted code snippet from the violation's associated location, if available.

- **Return value**: A formatted string representing the code snippet, or `null` if no location or snippet is available.

## Usage
