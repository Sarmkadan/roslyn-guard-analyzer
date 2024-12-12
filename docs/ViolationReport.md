# ViolationReport

The `ViolationReport` class serves as the primary data container and orchestration point for static analysis results within the `roslyn-guard-analyzer` project. It aggregates individual rule violations into logical groups, calculates aggregate statistics, and provides mechanisms to generate both human-readable summaries and structured detailed content. This type encapsulates the entire lifecycle of a report generation session, from the initial collection of violations to the final serialization or display of findings, ensuring consistent metadata tracking such as generation timestamps and project context.

## API

### Properties

*   **`public string Id`**
    *   **Purpose:** Gets the unique identifier for this specific report instance.
    *   **Return Value:** A string representing the report ID.
    *   **Remarks:** Used for tracking and correlating reports in logs or external systems.

*   **`public string Title`**
    *   **Purpose:** Gets the display title of the report.
    *   **Return Value:** A string containing the report title.

*   **`public string ProjectName`**
    *   **Purpose:** Gets the name of the analyzed project associated with this report.
    *   **Return Value:** A string representing the project name.

*   **`public DateTime GeneratedAt`**
    *   **Purpose:** Gets the timestamp indicating when the report was generated.
    *   **Return Value:** A `DateTime` object representing the creation time (usually UTC).

*   **`public List<ViolationGroup> ViolationGroups`**
    *   **Purpose:** Gets the collection of violation groups contained in this report.
    *   **Return Value:** A `List<ViolationGroup>` containing categorized violations.
    *   **Remarks:** This list is mutable; groups can be added via `AddViolationGroup`.

*   **`public ReportStatistics Statistics`**
    *   **Purpose:** Gets the aggregated statistical data for the report (e.g., counts by severity).
    *   **Return Value:** A `ReportStatistics` object.
    *   **Remarks:** Typically updated automatically when violations are added.

*   **`public string Summary`**
    *   **Purpose:** Gets a high-level textual summary of the report findings.
    *   **Return Value:** A string containing the summary.
    *   **Remarks:** May be populated lazily or via the `GenerateSummary` method.

*   **`public string DetailedContent`**
    *   **Purpose:** Gets the full, detailed content of the report, often formatted for display or export.
    *   **Return Value:** A string containing the detailed report body.

*   **`public ReportFormat Format`**
    *   **Purpose:** Gets the format type of the report (e.g., Markdown, JSON, HTML).
    *   **Return Value:** A `ReportFormat` enumeration value.

*   **`public string Name`**
    *   **Purpose:** Gets the name associated with the report or a specific entity within the report context.
    *   **Return Value:** A string representing the name.

*   **`public string Description`**
    *   **Purpose:** Gets the description providing context for the report or a specific section.
    *   **Return Value:** A string containing the description.

*   **`public List<RuleViolation> Violations`**
    *   **Purpose:** Gets a flat list of all rule violations contained directly within this context.
    *   **Return Value:** A `List<RuleViolation>`.

### Constructors

*   **`public ViolationReport()`**
    *   **Purpose:** Initializes a new instance of the `ViolationReport` class with default values.
    *   **Parameters:** None.
    *   **Remarks:** Sets `GeneratedAt` to the current time and initializes internal collections.

*   **`public ViolationReport(string projectName, string id)`**
    *   **Purpose:** Initializes a new instance of the `ViolationReport` class with specific project context.
    *   **Parameters:**
        *   `projectName`: The name of the project being analyzed.
        *   `id`: The unique identifier for the report.
    *   **Remarks:** Validates that neither parameter is null or empty.

### Methods

*   **`public void AddViolationGroup(ViolationGroup group)`**
    *   **Purpose:** Adds a new group of violations to the report.
    *   **Parameters:**
        *   `group`: The `ViolationGroup` to add.
    *   **Return Value:** None.
    *   **Exceptions:** Throws `ArgumentNullException` if `group` is null. Updates `Statistics` upon successful addition.

*   **`public Dictionary<SeverityLevel, List<RuleViolation>> GetViolationsBySeverity()`**
    *   **Purpose:** Aggregates all violations in the report grouped by their severity level.
    *   **Parameters:** None.
    *   **Return Value:** A `Dictionary` where the key is `SeverityLevel` and the value is a list of `RuleViolation` objects matching that severity.
    *   **Remarks:** Iterates through all `ViolationGroups` to compile the result.

*   **`public int GetTotalViolationCount()`**
    *   **Purpose:** Calculates the total number of individual violations across all groups.
    *   **Parameters:** None.
    *   **Return Value:** An integer representing the total count.
    *   **Remarks:** Returns 0 if no groups or violations exist.

*   **`public List<RuleViolation> GetViolationsFromFile(string filePath)`**
    *   **Purpose:** Retrieves a list of violations associated with a specific source file path.
    *   **Parameters:**
        *   `filePath`: The path of the source file to filter by.
    *   **Return Value:** A `List<RuleViolation>` containing only violations located in the specified file.
    *   **Remarks:** Performs a case-sensitive or culture-invariant match depending on implementation details of the path comparison. Returns an empty list if no matches are found.

*   **`public string GenerateSummary()`**
    *   **Purpose:** Generates and returns a textual summary of the report findings.
    *   **Parameters:** None.
    *   **Return Value:** A formatted string summarizing the key statistics and critical violations.
    *   **Remarks:** Updates the `Summary` property internally before returning.

## Usage

### Example 1: Creating and Populating a Report
This example demonstrates initializing a report, adding violation groups, and retrieving aggregate statistics.

```csharp
using RoslynGuardAnalyzer.Models;
using System;
using System.Collections.Generic;

// Initialize the report with project context
var report = new ViolationReport("MyCoreLibrary", "rpt_20231027_001");

// Create a sample violation group
var securityGroup = new ViolationGroup
{
    Category = "Security",
    Violations = new List<RuleViolation>
    {
        new RuleViolation { RuleId = "SEC001", Message = "Hardcoded credential detected", Severity = SeverityLevel.Critical }
    }
};

// Add the group to the report
report.AddViolationGroup(securityGroup);

// Retrieve statistics
int totalCount = report.GetTotalViolationCount();
var bySeverity = report.GetViolationsBySeverity();

Console.WriteLine($"Report '{report.Title}' generated at {report.GeneratedAt}");
Console.WriteLine($"Total Violations: {totalCount}");
Console.WriteLine($"Critical Issues: {bySeverity[SeverityLevel.Critical].Count}");
```

### Example 2: Filtering and Summarizing
This example shows how to filter violations by a specific file and generate a final summary string.

```csharp
using System;
using System.Linq;

// Assume 'report' is an existing populated ViolationReport instance
string targetFile = @"src/Core/AuthManager.cs";

// Extract violations specific to a file
var fileViolations = report.GetViolationsFromFile(targetFile);

if (fileViolations.Any())
{
    Console.WriteLine($"Found {fileViolations.Count} issues in {targetFile}");
    
    // Generate the full summary
    string summaryContent = report.GenerateSummary();
    
    // Access detailed content if needed for export
    if (report.Format == ReportFormat.Markdown)
    {
        Console.WriteLine(report.DetailedContent);
    }
}
else
{
    Console.WriteLine($"No violations found in {targetFile}");
}
```

## Notes

*   **Thread Safety:** The `ViolationReport` class is not thread-safe. Concurrent calls to `AddViolationGroup` while enumerating collections (e.g., inside `GetViolationsBySeverity` or `GetTotalViolationCount`) may result in `InvalidOperationException`. External synchronization is required if the instance is accessed by multiple threads.
*   **Null Handling:** While constructors validate critical parameters like `projectName` and `id`, methods like `GetViolationsFromFile` generally return empty collections rather than throwing exceptions if no data matches the criteria. However, passing `null` to `AddViolationGroup` will explicitly throw an `ArgumentNullException`.
*   **Data Consistency:** The `Statistics` property is designed to reflect the current state of `ViolationGroups`. If `ViolationGroups` is modified directly (e.g., `report.ViolationGroups.Add(...)`) instead of using `AddViolationGroup`, the `Statistics` object may become stale and inconsistent with the actual data. Always use the provided mutation methods to maintain integrity.
*   **Summary Generation:** The `Summary` property may return null or an empty string if `GenerateSummary()` has not been invoked yet, depending on whether the implementation uses lazy loading. It is recommended to call `GenerateSummary()` explicitly before accessing the `Summary` property for reliable output.
