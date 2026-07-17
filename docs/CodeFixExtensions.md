# CodeFixExtensions

`CodeFixExtensions` is a static utility class in the `roslyn-guard-analyzer` project that provides extension methods for analyzing and manipulating code fix metadata, including severity comparisons, prioritization logic, and file context extraction. These methods facilitate the categorization and prioritization of code fixes based on their impact, applicability, and structural properties.

## API

### `IsMoreSevereThan`

```csharp
public static bool IsMoreSevereThan(DiagnosticSeverity left, DiagnosticSeverity right)
```

Determines whether one diagnostic severity is more severe than another.

**Parameters**
- `left`: The first `DiagnosticSeverity` to compare.
- `right`: The second `DiagnosticSeverity` to compare.

**Returns**
- `true` if `left` is more severe than `right`; otherwise, `false`.

**Exceptions**
- None. Returns `false` for invalid or equal severity values.

---

### `IsLessSevereThan`

```csharp
public static bool IsLessSevereThan(DiagnosticSeverity left, DiagnosticSeverity right)
```

Determines whether one diagnostic severity is less severe than another.

**Parameters**
- `left`: The first `DiagnosticSeverity` to compare.
- `right`: The second `DiagnosticSeverity` to compare.

**Returns**
- `true` if `left` is less severe than `right`; otherwise, `false`.

**Exceptions**
- None. Returns `false` for invalid or equal severity values.

---

### `GetSeverityString`

```csharp
public static string GetSeverityString(DiagnosticSeverity severity)
```

Converts a `DiagnosticSeverity` value to its corresponding string representation.

**Parameters**
- `severity`: The `DiagnosticSeverity` to convert.

**Returns**
- A string representing the severity (e.g., "Error", "Warning", "Info").

**Exceptions**
- None. Returns an empty string for unrecognized severity values.

---

### `IsBreaking`

```csharp
public static bool IsBreaking(CodeFixProvider fix)
```

Determines whether a code fix introduces a breaking change.

**Parameters**
- `fix`: The `CodeFixProvider` to evaluate.

**Returns**
- `true` if the fix is breaking; otherwise, `false`.

**Exceptions**
- None. Returns `false` for `null` inputs.

---

### `GetDisplaySummary`

```csharp
public static string GetDisplaySummary(CodeFixProvider fix)
```

Generates a human-readable summary of a code fix's purpose.

**Parameters**
- `fix`: The `CodeFixProvider` to summarize.

**Returns**
- A string describing the fix's intent.

**Exceptions**
- None. Returns an empty string for `null` inputs.

---

### `GetAge`

```csharp
public static string GetAge(CodeFixProvider fix)
```

Retrieves the age of a code fix as a formatted string (e.g., "2 days", "1 month").

**Parameters**
- `fix`: The `CodeFixProvider` to evaluate.

**Returns**
- A string representing the time elapsed since the fix was created.

**Exceptions**
- None. Returns "Unknown" for `null` inputs or missing timestamps.

---

### `TargetsFile`

```csharp
public static bool TargetsFile(CodeFixProvider fix, string filePath)
```

Checks if a code fix applies to a specific file path.

**Parameters**
- `fix`: The `CodeFixProvider` to check.
- `filePath`: The file path to evaluate.

**Returns**
- `true` if the fix targets the specified file; otherwise, `false`.

**Exceptions**
- None. Returns `false` for `null` inputs.

---

### `GetFileExtension`

```csharp
public static string GetFileExtension(CodeFixProvider fix)
```

Extracts the file extension associated with a code fix.

**Parameters**
- `fix`: The `CodeFixProvider` to evaluate.

**Returns**
- The file extension (e.g., ".cs") or an empty string if unspecified.

**Exceptions**
- None. Returns an empty string for `null` inputs.

---

### `IsInLineRange`

```csharp
public static bool IsInLineRange(CodeFixProvider fix, int startLine, int endLine)
```

Determines whether a code fix's location falls within a specified line range.

**Parameters**
- `fix`: The `CodeFixProvider` to check.
- `startLine`: The starting line number (inclusive).
- `endLine`: The ending line number (inclusive).

**Returns**
- `true` if the fix is within the range; otherwise, `false`.

**Exceptions**
- None. Returns `false` for `null` inputs or invalid line ranges.

---

### `GetCodeContext`

```csharp
public static string GetCodeContext(CodeFixProvider fix)
```

Retrieves the surrounding code context for a code fix.

**Parameters**
- `fix`: The `CodeFixProvider` to evaluate.

**Returns**
- A string containing the relevant code snippet.

**Exceptions**
- None. Returns an empty string for `null` inputs.

---

### `ShouldPrioritize`

```csharp
public static bool ShouldPrioritize(CodeFixProvider fix)
```

Determines whether a code fix should be prioritized during application.

**Parameters**
- `fix`: The `CodeFixProvider` to evaluate.

**Returns**
- `true` if the fix should be prioritized; otherwise, `false`.

**Exceptions**
- None. Returns `false` for `null` inputs.

---

### `GetPriorityScore`

```csharp
public static int GetPriorityScore(CodeFixProvider fix)
```

Calculates a numerical priority score for a code fix.

**Parameters**
- `fix`: The `CodeFixProvider` to evaluate.

**Returns**
- An integer score (higher values indicate higher priority).

**Exceptions**
- None. Returns `0` for `null` inputs.

---

### `CanBeApplied`

```csharp
public static bool CanBeApplied(CodeFixProvider fix)
```

Checks whether a code fix can be safely applied in the current context.

**Parameters**
- `fix`: The `CodeFixProvider` to evaluate.

**Returns**
- `true` if the fix can be applied; otherwise, `false`.

**Exceptions**
- None. Returns `false` for `null` inputs.

---

### `GetFileName`

```csharp
public static string GetFileName(CodeFixProvider fix)
```

Extracts the file name from a code fix's target location.

**Parameters**
- `fix`: The `CodeFixProvider` to evaluate.

**Returns**
- The file name or an empty string if unspecified.

**Exceptions**
- None. Returns an empty string for `null` inputs.

---

### `GetDirectoryName`

```csharp
public static string GetDirectoryName(CodeFixProvider fix)
```

Extracts the directory path from a code fix's target location.

**Parameters**
- `fix`: The `CodeFixProvider` to evaluate.

**Returns**
- The directory path or an empty string if unspecified.

**Exceptions**
- None. Returns an empty string for `null` inputs.

---

## Usage

### Example 1: Comparing Severities

```csharp
var errorSeverity = DiagnosticSeverity.Error;
var warningSeverity = DiagnosticSeverity.Warning;

bool isMoreSevere = errorSeverity.IsMoreSevereThan(warningSeverity); // Returns true
string severityLabel = errorSeverity.GetSeverityString(); // Returns "Error"
```

### Example 2: Prioritizing Code Fixes

```csharp
var fix = codeFixProvider;
if (fix.ShouldPrioritize() && fix.CanBeApplied())
{
    int score = fix.GetPriorityScore();
    Console.WriteLine($"Applying high-priority fix with score {score}");
}
```

---

## Notes

- All methods are thread-safe and stateless, relying solely on input parameters.
- Methods return default values (`false`, `0`, or empty strings) for `null` inputs rather than throwing exceptions.
- Severity comparisons (`IsMoreSevereThan`, `IsLessSevereThan`) follow the standard Roslyn severity hierarchy: `Error > Warning > Info`.
- File-related methods (`TargetsFile`, `GetFileExtension`, etc.) assume the `CodeFixProvider` has valid location metadata; behavior is undefined for providers without file context.
