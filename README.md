# Roslyn Guard Analyzer

...

## RuleConfiguration

The `RuleConfiguration` class represents a configuration object that controls how analysis rules are applied during code analysis. It allows you to customize rule selection, define exclusions, set analysis limits, and configure reporting behavior.

### Usage Example

```csharp
// Create a new rule configuration
var config = new RuleConfiguration("Security Rules", "Configuration for security-focused analysis")
{
    Description = "Applies security best practices and vulnerability detection",
    MaxViolationsToReport = 100,
    AnalysisTimeoutSeconds = 300,
    MinimumReportedSeverity = SeverityLevel.Error,
    FailOnError = true,
    GenerateDetailedReport = true
};

// Add some rules
config.AddRule(new AnalysisRule("SG001", "PasswordStorage", "Enforces secure password storage practices"));
config.AddRule(new AnalysisRule("SG002", "Cryptography", "Validates cryptographic algorithm usage"));
config.AddRule(new AnalysisRule("SG003", "InputValidation", "Checks for proper input validation"));

// Exclude specific namespaces and files
config.ExcludeNamespace("Legacy.System");
config.ExcludeNamespace("Tests.Integration");
config.ExcludeFile("*.Designer.cs");

// Set custom settings
config.SetCustomSetting("reportFormat", "html");
config.SetCustomSetting("maxDepth", "5");

// Check if configuration is valid
if (config.IsValid())
{
    Console.WriteLine($"Configuration '{config.Name}' is valid with {config.GetEnabledRuleCount()} enabled rules");
}

// Get a specific rule
var rule = config.GetRule("SG001");
if (rule != null)
{
    Console.WriteLine($"Found rule: {rule.Name}");
}

// Remove a rule
config.RemoveRule("SG002");

// Create a copy for modification
var configCopy = config.CreateCopy();
configCopy.Name = "Security Rules - Modified";
```

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