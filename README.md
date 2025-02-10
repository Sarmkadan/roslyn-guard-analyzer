# Roslyn Guard Analyzer

...

## RuleViolation

The `RuleViolation` class represents a specific violation of an architectural or coding rule detected during analysis. It captures detailed information about the violation, including its location, severity, and metadata. This class is used to report issues like incorrect code patterns, security risks, or style violations.

### Usage Example

```csharp
var violation = new RuleViolation("RS001", "NamingConvention", "Invalid method name", "Program.cs")
{
    LineNumber = 15,
    ColumnNumber = 10,
    ProjectName = "MyProject",
    Category = RuleCategory.CodeStructure
};
violation.AddMetadata("Reviewer", "JohnDoe");
var updatedViolation = violation.WithSeverity(SeverityLevel.Error);
Console.WriteLine(updatedViolation.GetFullDescription());
// Output: [RS001] Error: Invalid method name at Program.cs(15, 10)
```

## ViolationReport

The `ViolationReport` class represents a formatted report containing analysis violations and statistics. It aggregates violations into groups, tracks project metadata, and provides methods for querying and summarizing the violations. This class is used to generate comprehensive analysis reports that can be exported in different formats.

### Usage Example

```csharp
// Create a new violation report
var report = new ViolationReport("Architecture Analysis", "MySolution")
{
    Summary = "Analysis completed successfully",
    DetailedContent = "Detailed analysis results...",
    Format = ReportFormat.Html
};

// Create violation groups
var namingGroup = new ViolationGroup("Naming Conventions", "Violations of naming conventions");
var securityGroup = new ViolationGroup("Security Rules", "Security-related violations");

// Add violations to groups
namingGroup.AddViolation(new RuleViolation("RS001", "NamingConvention", "Invalid class name", "Program.cs")
{
    LineNumber = 25,
    ColumnNumber = 5,
    Severity = SeverityLevel.Error,
    ProjectName = "MySolution"
});

securityGroup.AddViolation(new RuleViolation("RS015", "SecurityRisk", "Hardcoded password", "Config.cs")
{
    LineNumber = 42,
    ColumnNumber = 15,
    Severity = SeverityLevel.Critical,
    ProjectName = "MySolution"
});

// Add groups to report
report.AddViolationGroup(namingGroup);
report.AddViolationGroup(securityGroup);

// Get statistics and summary
Console.WriteLine(report.GenerateSummary());
var violationsBySeverity = report.GetViolationsBySeverity();
var totalViolations = report.GetTotalViolationCount();
var fileViolations = report.GetViolationsFromFile("Program.cs");
```

...
