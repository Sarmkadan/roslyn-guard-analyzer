# AnalysisFilterBuilder
The `AnalysisFilterBuilder` type is designed to construct filters for analyzing rule violations in code. It provides a fluent API for specifying various criteria, such as severity, rule, file, and line numbers, to narrow down the set of violations to be considered. This allows for more targeted and efficient analysis of code quality issues.

## API
* `MinimumSeverity`: Sets the minimum severity for the filter. Returns the current `AnalysisFilterBuilder` instance.
* `BySeverity`: Specifies the severity for the filter. Returns the current `AnalysisFilterBuilder` instance. There are two overloads for this method.
* `ByRule`: Filters by a specific rule. Returns the current `AnalysisFilterBuilder` instance.
* `ByAnyRule`: Filters by any rule. Returns the current `AnalysisFilterBuilder` instance.
* `ByFile`: Filters by a specific file. Returns the current `AnalysisFilterBuilder` instance.
* `FromLine` and `ToLine`: Specify the line number range for the filter. Returns the current `AnalysisFilterBuilder` instance.
* `ContainsMessage`: Filters by a message containing specific text. Returns the current `AnalysisFilterBuilder` instance.
* `Where`: Applies a custom filter condition. Returns the current `AnalysisFilterBuilder` instance.
* `Build`: Returns a `Func<RuleViolation, bool>` representing the constructed filter.
* `Apply`: Applies the filter to a set of rule violations and returns the filtered `IEnumerable<RuleViolation>`. This method does not throw any exceptions based on the provided information.

## Usage
```csharp
// Example 1: Filtering by severity and rule
var filter = new AnalysisFilterBuilder()
    .MinimumSeverity(Severity.Error)
    .ByRule("CA1000")
    .Build();

var violations = GetRuleViolations();
var filteredViolations = violations.Where(filter);
```

```csharp
// Example 2: Filtering by file and line numbers
var filter = new AnalysisFilterBuilder()
    .ByFile("Program.cs")
    .FromLine(10)
    .ToLine(20)
    .ContainsMessage("unused variable")
    .Build();

var violations = GetRuleViolations();
var filteredViolations = violations.Where(filter);
```

## Notes
The `AnalysisFilterBuilder` type is designed to be used in a fluent manner, allowing for easy construction of complex filters. However, it does not provide any thread-safety guarantees, so care should be taken when using instances of this type in multi-threaded environments. Additionally, the `Build` method returns a `Func<RuleViolation, bool>`, which can be used to filter rule violations, but it does not provide any information about the filter itself. The `Apply` method can be used to apply the filter to a set of rule violations, but it does not throw any exceptions based on the provided information. Edge cases, such as an empty set of rule violations or a filter that matches no violations, should be handled by the calling code.
