# HtmlFormatter

The `HtmlFormatter` type is used to convert code analysis data into an HTML representation. After the formatting operation completes, its properties indicate whether the operation succeeded and provide the formatted output, any violations encountered, and a combined report.

## API

### CanFormat
- **Purpose:** Indicates whether the formatter was able to produce HTML output.
- **Return value:** `true` if the formatting succeeded; `false` otherwise.
- **Parameters:** None.
- **Exceptions:** None.

### FormatResult
- **Purpose:** Retrieves the formatted HTML string when formatting succeeded.
- **Return value:** The HTML‑encoded result; returns `null` or an empty string when `CanFormat` is `false`.
- **Parameters:** None.
- **Exceptions:** Throws `InvalidOperationException` if accessed before the formatting operation has completed or when `CanFormat` is `false`.

### FormatViolations
- **Purpose:** Provides a textual description of any formatting violations that were detected.
- **Return value:** A string containing violation details; empty if no violations were found.
- **Parameters:** None.
- **Exceptions:** None.

### FormatReport
- **Purpose:** Returns a combined report that includes the formatting outcome, the result (if any), and any violations.
- **Return value:** A string summarizing the operation; may be `null` if no formatting was performed.
- **Parameters:** None.
- **Exceptions:** None.

## Usage

```csharp
var formatter = new HtmlFormatter(syntaxTree, options);
bool success = formatter.CanFormat;

if (success)
{
    string html = formatter.FormatResult;   // safe to use
    // embed html in a report or web page
}
else
{
    // formatting failed; inspect why
    string violations = formatter.FormatViolations;
    Log.Error($"HtmlFormatter failed: {violations}");
}
```

```csharp
var formatter = new HtmlFormatter(document.GetSyntaxTreeAsync().Result, formatterOptions);
// Assume formatting occurs during construction or via a method not shown here.
string report = formatter.FormatReport;   // contains both result and violations
File.WriteAllText("format-report.html", report);
```

## Notes
- Accessing `FormatResult` when `CanFormat` is `false` will result in an `InvalidOperationException`; callers should check `CanFormat` first.
- The properties are populated only after the formatting operation finishes; reading them before that point may yield default values (`false`, `null`, or empty strings).
- The type does not contain any mutable state that changes after the formatting operation completes, making it safe for concurrent read‑only access by multiple threads. However, invoking the formatting operation itself from multiple threads on the same instance is not supported and may lead to undefined behavior.
