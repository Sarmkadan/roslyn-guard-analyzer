# Roslyn Guard Analyzer

...

## DomainExtensions

The `DomainExtensions` class provides a set of extension methods for domain models and common types. It offers various utility methods for working with `RuleViolation` objects, such as grouping by file, filtering by severity, and exporting to text.

### Usage Example
```csharp
var violations = new List<RuleViolation>
{
    new RuleViolation { Severity = SeverityLevel.Error, FilePath = "path/to/file1.cs", LineNumber = 10 },
    new RuleViolation { Severity = SeverityLevel.Warning, FilePath = "path/to/file2.cs", LineNumber = 20 },
    new RuleViolation { Severity = SeverityLevel.Error, FilePath = "path/to/file1.cs", LineNumber = 30 },
};

var groupedViolations = violations.GroupByFileAndSort();
foreach (var fileViolations in groupedViolations)
{
    Console.WriteLine($"File: {fileViolations.Key}");
    foreach (var violation in fileViolations.Value)
    {
        Console.WriteLine($"  Severity: {violation.Severity.GetDisplayName()}, Line: {violation.LineNumber}");
    }
}

var filteredViolations = violations.FilterBySeverity(SeverityLevel.Error);
foreach (var violation in filteredViolations)
{
    Console.WriteLine($"Severity: {violation.Severity.GetDisplayName()}, Line: {violation.LineNumber}");
}

var summary = violations.SummarizeByCategory();
foreach (var category in summary)
{
    Console.WriteLine($"Category: {category.Key}, Count: {category.Value}");
}

var percentages = violations.CalculateSeverityPercentages();
foreach (var severity in percentages)
{
    Console.WriteLine($"Severity: {severity.Key}, Percentage: {severity.Value:F2}%");
}

var mostCommonRule = violations.GetMostCommonRule();
Console.WriteLine($"Most Common Rule: {mostCommonRule}");

var mostProblematicFile = violations.GetMostProblematicFile();
Console.WriteLine($"Most Problematic File: {mostProblematicFile}");

var exportText = violations.ExportToText("Violations Export");
Console.WriteLine(exportText);
```

...
```