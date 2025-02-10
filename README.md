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

...
