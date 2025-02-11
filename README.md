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

## AnalysisRule

The `AnalysisRule` class defines an architectural or coding rule used by the analyzer to inspect code. It encapsulates all necessary metadata, severity settings, and configuration parameters to identify violations effectively. This class allows developers to define, customize, and validate specific rules to maintain codebase integrity.

### Usage Example

```csharp
// Create a new analysis rule
var rule = new AnalysisRule("AR001", "InterfaceNaming", "Interfaces must start with 'I'", RuleCategory.CodeStructure)
{
    DefaultSeverity = SeverityLevel.Warning,
    IsEnabled = true,
    Author = "SecurityTeam"
};

// Configure the rule
rule.SetConfigurationValue("MinInterfaceNameLength", 3);
rule.MarkAsModified();

// Validate and use the rule
if (rule.IsValid())
{
    var severityOverride = rule.WithSeverity(SeverityLevel.Error);
    var minLength = rule.GetConfigurationValue<int>("MinInterfaceNameLength", 2);
    Console.WriteLine($"Rule {rule.Name} is valid. Min length: {minLength}");
}
```


## AnalysisResult

The `AnalysisResult` class contains the complete results of a code analysis execution. It includes all violations found, analysis statistics, and metadata about the analyzed project. This class serves as the primary container for analysis data and provides methods for querying violations and generating reports.

### Usage Example

```csharp
// Create a new analysis result for a project
var result = new AnalysisResult("MyWebApplication", @"/projects/MyWebApplication/MyWebApplication.csproj")
{
    AnalysisStartTime = DateTime.UtcNow.AddMinutes(-10)
};

// Add analyzed code elements
result.AddAnalyzedElement(new CodeElement("Program.cs", CodeElementType.Class, 42));
result.AddAnalyzedElement(new CodeElement("Startup.cs", CodeElementType.Class, 28));

// Add violations found during analysis
result.AddViolation(new RuleViolation("RG001", "SecurityRisk", "Hardcoded connection string", "appsettings.json")
{
    LineNumber = 15,
    ColumnNumber = 8,
    Severity = SeverityLevel.Critical,
    Category = RuleCategory.Security
});

result.AddViolation(new RuleViolation("RG005", "NamingConvention", "Method name not PascalCase", "UserService.cs")
{
    LineNumber = 42,
    ColumnNumber = 12,
    Severity = SeverityLevel.Error,
    Category = RuleCategory.CodeStructure
});

// Access analysis statistics
Console.WriteLine($"Total violations: {result.ViolationCount}");
Console.WriteLine($"Total elements analyzed: {result.TotalElementsAnalyzed}");
Console.WriteLine($"Analysis duration: {result.GetDuration().TotalSeconds} seconds");
Console.WriteLine($"Success percentage: {result.GetSuccessPercentage():F2}%");

// Get violations by severity
var criticalViolations = result.GetCriticalViolations();
var errorViolations = result.GetViolationCountBySeverity(SeverityLevel.Error);
var warningViolations = result.GetViolationCountBySeverity(SeverityLevel.Warning);

// Get violations grouped by rule
var violationsByRule = result.GetViolationsByRule();
foreach (var ruleGroup in violationsByRule)
{
    Console.WriteLine($"Rule {ruleGroup.Key}: {ruleGroup.Value.Count} violations");
}

// Get violations in specific file
var fileViolations = result.GetViolationsInFile("UserService.cs");

// Mark analysis as completed
result.MarkAsCompleted();

// Access summary statistics
Console.WriteLine($"Violations by category: {string.Join(", ", result.ViolationsByCategory.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
Console.WriteLine($"Violations by severity: {string.Join(", ", result.ViolationsBySeverity.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
```

...
