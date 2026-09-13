# Formatters

The Roslyn Guard Analyzer provides multiple output formatters for analysis results, allowing users to export findings in various formats suitable for different use cases. Each formatter implements the `IOutputFormatter` interface and can be accessed through the `FormatterRegistry`.

## Available Formatters

| Formatter | Format Identifier | Best For |
|-----------|------------------|----------|
| `CsvFormatter` | `csv` | Spreadsheet import, data analysis tools |
| `HtmlFormatter` | `html` | Human-readable reports, browser viewing |
| `JsonFormatter` | `json` | Programmatic consumption, API integration |
| `SarifFormatter` | `sarif` | Static analysis tool integration, CI/CD pipelines |

## Common Interface

All formatters implement the `IOutputFormatter` interface:

```csharp
public interface IOutputFormatter
{
    /// <summary>
    /// Gets the format identifier (e.g., "json", "csv", "html").
    /// </summary>
    string Format { get; }

    /// <summary>
    /// Formats an analysis result into a formatted string.
    /// </summary>
    string FormatResult(AnalysisResult result);

    /// <summary>
    /// Formats a collection of violations into formatted output.
    /// </summary>
    string FormatViolations(IEnumerable<RuleViolation> violations);

    /// <summary>
    /// Formats a report object into formatted output.
    /// </summary>
    string FormatReport(ViolationReport report);

    /// <summary>
    /// Checks if this formatter can handle the given format identifier.
    /// </summary>
    bool CanFormat(string format);
}
```

## Usage Examples

### Using FormatterRegistry

```csharp
using RoslynGuardAnalyzer.Formatters;

// Create registry with default formatters
var registry = FormatterRegistry.CreateWithDefaults();

// Get a specific formatter
var jsonFormatter = registry.GetFormatter("json");
if (jsonFormatter != null)
{
    string jsonOutput = jsonFormatter.FormatResult(analysisResult);
}

// Check if format is supported
if (registry.IsFormatSupported("csv"))
{
    var csvFormatter = registry.GetFormatterOrThrow("csv");
    string csvOutput = csvFormatter.FormatViolations(violations);
}

// Get all supported formats
foreach (var format in registry.GetSupportedFormats())
{
    Console.WriteLine($"Supported format: {format}");
}
```

### Direct Usage

```csharp
using RoslynGuardAnalyzer.Formatters;

// CSV formatter
var csvFormatter = new CsvFormatter();
string csv = csvFormatter.FormatReport(report);

// HTML formatter
var htmlFormatter = new HtmlFormatter();
string html = htmlFormatter.FormatResult(result);

// JSON formatter
var jsonFormatter = new JsonFormatter();
string json = jsonFormatter.FormatViolations(violations);

// SARIF formatter
var sarifFormatter = new SarifFormatter();
string sarif = sarifFormatter.FormatReport(report);
```

## Format-Specific Details

### CSV Formatter (`CsvFormatter`)

**Format Identifier:** `csv`

**Output Structure:**
- Summary section with title, generation time, and total violations
- Severity summary (Critical, High, Medium, Low counts)
- Violations by rule (rule name, count, highest severity)
- Detailed violations with columns: Rule, Severity, Message, File, Line, Column, Code

**Best For:**
- Import into Excel, Google Sheets, or other spreadsheet applications
- Data analysis and filtering in tools like pandas, R, or SQL
- Archival and trend analysis over time

**Example Output:**
```
SUMMARY
Title,My Project Analysis
Generated,2026-09-13 10:30:00
Total Violations,15

SEVERITY SUMMARY
Severity,Count
Critical,2
High,5
Medium,6
Low,2

VIOLATIONS BY RULE
Rule,Count,Severity
RG001,5,Critical
RG002,4,High
RG003,3,Medium
RG004,2,Low
RG005,1,Critical

DETAILED VIOLATIONS
Rule,Severity,Message,File,Line,Column,Code
RG001,Critical,"Possible null reference","src/Services/UserService.cs",42,15,"if (user == null)"
RG002,High,"Unused using directive","src/Controllers/HomeController.cs",1,1,"using System.Web;"
```

### HTML Formatter (`HtmlFormatter`)

**Format Identifier:** `html`

**Output Structure:**
- Complete HTML document with embedded CSS styling
- Header with project information and generation timestamp
- Summary statistics (total violations, affected files)
- Violations table with severity-based row coloring
- Responsive design suitable for desktop and mobile viewing

**Best For:**
- Human-readable reports in web browsers
- Email attachments or web publishing
- Quick visual scanning of results
- Sharing with non-technical stakeholders

**Features:**
- Severity-based row coloring (Critical: red, Error: orange, Warning: yellow, Info: blue)
- Clickable file paths (when rendered in appropriate environments)
- Responsive table layout
- Clean, modern styling with CSS

### JSON Formatter (`JsonFormatter`)

**Format Identifier:** `json`

**Output Structure:**
- Minified JSON suitable for programmatic consumption
- For `FormatResult`: Project info, analysis metadata, and violations array
- For `FormatViolations`: Count and violations array
- For `FormatReport`: Report metadata, summary, severity breakdown, and violations grouped by rule

**Best For:**
- API responses and web service integration
- Programmatic processing in scripts or applications
- Storage in databases or log systems
- Integration with other development tools

**Example Output (FormatResult):**
```json
{
  "ProjectName":"MyProject",
  "ProjectPath":"/src/MyProject",
  "AnalysisSucceeded":true,
  "ErrorMessage":null,
  "TotalFilesAnalyzed":42,
  "TotalElementsAnalyzed":1250,
  "ViolationCount":15,
  "Violations":[
    {
      "RuleId":"RG001",
      "RuleName":"NullReferenceCheck",
      "Severity":"Critical",
      "Message":"Possible null reference dereference",
      "FilePath":"src/Services/UserService.cs",
      "LineNumber":42,
      "ColumnNumber":15,
      "CodeSnippet":"if (user == null)"
    }
  ],
  "TimestampUtc":"2026-09-13T10:30:00.1234567Z"
}
```

### SARIF Formatter (`SarifFormatter`)

**Format Identifier:** `sarif`

**Output Structure:**
- SARIF 2.1.0 compliant JSON output
- Standard format for static analysis tool integration
- Includes tool information, rules, invocations, and results
- Supports SARIF viewers and CI/CD platforms (GitHub Code Scanning, Azure DevOps, etc.)

**Best For:**
- Integration with CI/CD pipelines and code scanning tools
- Upload to GitHub Security tab or Azure DevOps
- Consumption by SARIF-compatible viewers and dashboards
- Standardized exchange with other static analysis tools

**Features:**
- Proper SARIF severity mapping (Critical/Error → error, Warning → warning, Info → note)
- Rule metadata with help URLs
- Code snippet inclusion when available
- GUID tracking for individual results
- Property bags for additional metadata (severity, category, project, timestamp)

## Choosing the Right Format

### For Human Consumption:
- **HTML**: Best for visual reports in browsers
- **CSV**: Best for spreadsheet analysis and filtering

### For Machine Consumption:
- **JSON**: Best for custom applications and APIs
- **SARIF**: Best for toolchain integration and standard compliance

### For Specific Use Cases:
- **Archival/Trend Analysis**: CSV (easy to parse and aggregate)
- **Dashboard Integration**: JSON or SARIF (structured data)
- **Code Scanning Platforms**: SARIF (standard format)
- **Quick Sharing**: HTML (self-contained, styled)
- **Data Processing Pipelines**: JSON (lightweight, widely supported)

## Extending Formatters

To add a new formatter:

1. Implement the `IOutputFormatter` interface
2. Register it with the `FormatterRegistry`:
   ```csharp
   registry.Register(new MyCustomFormatter());
   ```
3. Ensure the formatter handles null inputs appropriately
4. Follow the existing code style and documentation patterns

## Thread Safety

All formatters are stateless and thread-safe for concurrent read operations. However, if you maintain state in a custom formatter, ensure proper synchronization.

## Error Handling

All formatters throw `ArgumentNullException` when null arguments are passed to formatting methods. They validate inputs according to their contracts and do not suppress exceptions from underlying operations (e.g., file I/O when writing output).

## Performance Considerations

- Formatters build output strings using `StringBuilder` for efficiency
- Large violation sets may produce large output strings
- Consider streaming approaches for extremely large datasets
- JSON and SARIF formatters produce valid, parseable output
- HTML formatter includes embedded CSS for self-contained reports