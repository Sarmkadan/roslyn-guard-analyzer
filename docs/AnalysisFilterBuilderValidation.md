# AnalysisFilterBuilderValidation

`AnalysisFilterBuilderValidation` is a static utility class that provides centralized validation logic for the `AnalysisFilterBuilder` configuration pipeline. It exposes a set of pre-defined validation error messages and corresponding `Ensure*` guard methods that throw `ArgumentException` when filter parameters are invalid. This class ensures that filter criteria such as severity, rule names, file paths, line numbers, message text, and custom predicates are well-formed before they are applied to an analysis filter.

## API

### Validation Messages

- **`public static IReadOnlyList<string> Validate`**  
  Returns the list of all validation error messages that have been accumulated during filter validation. This property is populated by calls to the `EnsureValid*` methods.

- **`public static IReadOnlyList<string> ValidateSeverity`**  
  Returns the list of validation error messages specifically related to severity filter configuration.

- **`public static IReadOnlyList<string> ValidateRuleName`**  
  Returns the list of validation error messages specifically related to a single rule name filter configuration.

- **`public static IReadOnlyList<string> ValidateFilePath`**  
  Returns the list of validation error messages specifically related to file path filter configuration.

- **`public static IReadOnlyList<string> ValidateLineNumber`**  
  Returns the list of validation error messages specifically related to line number filter configuration.

- **`public static IReadOnlyList<string> ValidateMessageText`**  
  Returns the list of validation error messages specifically related to message text filter configuration.

- **`public static IReadOnlyList<string> ValidatePredicate`**  
  Returns the list of validation error messages specifically related to predicate filter configuration.

- **`public static IReadOnlyList<string> ValidateRuleNames`**  
  Returns the list of validation error messages specifically related to multiple rule names filter configuration.

### Validation State

- **`public static bool IsValid`**  
  Gets a value indicating whether the current filter configuration is valid (i.e., no validation errors have been recorded). Returns `true` if the `Validate` collection is empty; otherwise `false`.

### Validation Guard Methods

Each `EnsureValid*` method validates its corresponding parameter and, if the parameter is invalid, adds an error message to the appropriate validation collection and throws an `ArgumentException`. The methods do not return a value.

- **`public static void EnsureValid`**  
  Validates the overall filter configuration. Throws `ArgumentException` if any validation errors exist in the `Validate` collection.

- **`public static void EnsureValidSeverity`**  
  Validates the severity filter parameter. Throws `ArgumentException` if the severity value is not recognized or is otherwise invalid.

- **`public static void EnsureValidRuleName`**  
  Validates a single rule name filter parameter. Throws `ArgumentException` if the rule name is null, empty, or otherwise invalid.

- **`public static void EnsureValidFilePath`**  
  Validates the file path filter parameter. Throws `ArgumentException` if the file path is null, empty, or does not conform to expected path formats.

- **`public static void EnsureValidLineNumber`**  
  Validates the line number filter parameter. Throws `ArgumentException` if the line number is negative or otherwise out of range.

- **`public static void EnsureValidMessageText`**  
  Validates the message text filter parameter. Throws `ArgumentException` if the message text is null, empty, or otherwise invalid.

- **`public static void EnsureValidPredicate`**  
  Validates the predicate filter parameter. Throws `ArgumentException` if the predicate is null or otherwise invalid.

- **`public static void EnsureValidRuleNames`**  
  Validates the collection of rule name filter parameters. Throws `ArgumentException` if the collection is null, empty, or contains invalid entries.

## Usage

### Example 1: Building a Filter with Individual Validations

```csharp
var builder = new AnalysisFilterBuilder();

// Set filter criteria with inline validation
builder.SetSeverity(DiagnosticSeverity.Warning);
AnalysisFilterBuilderValidation.EnsureValidSeverity();

builder.SetRuleName("CS8618");
AnalysisFilterBuilderValidation.EnsureValidRuleName();

builder.SetFilePath("src/**/*.cs");
AnalysisFilterBuilderValidation.EnsureValidFilePath();

// Final validation before building
AnalysisFilterBuilderValidation.EnsureValid();

if (AnalysisFilterBuilderValidation.IsValid)
{
    var filter = builder.Build();
    // Apply filter to analysis
}
```

### Example 2: Batch Validation and Error Inspection

```csharp
var builder = new AnalysisFilterBuilder();

builder.SetSeverity(DiagnosticSeverity.Error);
builder.SetRuleNames(new[] { "CA1062", "CA1031" });
builder.SetLineNumber(42);
builder.SetPredicate(diagnostic => diagnostic.Id.StartsWith("CA"));

// Validate all configured aspects
AnalysisFilterBuilderValidation.EnsureValidSeverity();
AnalysisFilterBuilderValidation.EnsureValidRuleNames();
AnalysisFilterBuilderValidation.EnsureValidLineNumber();
AnalysisFilterBuilderValidation.EnsureValidPredicate();

// Check for any accumulated errors
if (!AnalysisFilterBuilderValidation.IsValid)
{
    foreach (var error in AnalysisFilterBuilderValidation.Validate)
    {
        Console.WriteLine($"Validation error: {error}");
    }
    return;
}

var filter = builder.Build();
```

## Notes

- All `Ensure*` methods are static and operate on shared state within the `AnalysisFilterBuilderValidation` class. They are designed to be called in sequence during a single filter-building operation.
- The validation message collections (`Validate`, `ValidateSeverity`, etc.) are populated incrementally as each `Ensure*` method is called. They are not automatically cleared between calls; consumers should manage the lifecycle of the validation context appropriately.
- The `IsValid` property reflects the state of the `Validate` collection at the time it is accessed. It does not perform any validation itself.
- Calling `EnsureValid` without first calling the specific `Ensure*` methods will only check whether any prior validations have failed; it does not retroactively validate unvalidated parameters.
- **Thread safety:** This class is not thread-safe. All methods access shared static state without synchronization. Concurrent calls from multiple threads will result in race conditions and corrupted validation state. Use only within a single-threaded context or employ external synchronization if necessary.
- The `ValidateSeverity` property is listed twice in the public API surface, suggesting an overload or duplicate exposure. Consumers should verify which instance is appropriate for their use case, as both return the same collection type but may be associated with different validation paths.
