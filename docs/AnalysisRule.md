# AnalysisRule

Represents a single analysis rule within the roslyn-guard-analyzer framework. Each rule encapsulates a unique identifier, metadata (name, description, category, severity), a pattern for matching code elements, and a mutable configuration dictionary. Instances can be created via constructors, validated, and modified through methods that support immutable transformations (e.g., `WithSeverity`) or direct mutation (e.g., `SetConfigurationValue`, `MarkAsModified`).

## API

### Properties

| Name | Type | Description |
|------|------|-------------|
| `Id` | `string` | Unique identifier for the rule. |
| `Name` | `string` | Human-readable name of the rule. |
| `Description` | `string` | Detailed explanation of what the rule checks. |
| `Category` | `RuleCategory` | The category under which the rule is grouped (e.g., Security, Performance). |
| `DefaultSeverity` | `SeverityLevel` | The default severity assigned to the rule (e.g., Warning, Error). |
| `IsEnabled` | `bool` | Indicates whether the rule is active during analysis. |
| `RulePattern` | `string?` | A pattern (e.g., a regular expression or syntax pattern) used to identify code elements the rule applies to. `null` if not set. |
| `Configuration` | `Dictionary<string, object>` | A mutable dictionary of key-value pairs for rule-specific settings. Never `null`. |
| `DocumentationUrl` | `string?` | URL pointing to external documentation for the rule. `null` if not provided. |
| `CreatedAt` | `DateTime` | Timestamp when the rule was first created. |
| `ModifiedAt` | `DateTime?` | Timestamp of the last modification, or `null` if never modified. |
| `Author` | `string?` | The author or maintainer of the rule. `null` if unknown. |
| `Version` | `Version?` | Version of the rule definition. `null` if not versioned. |

### Constructors

| Signature | Description |
|-----------|-------------|
| `AnalysisRule()` | Initializes a new instance with default values. `Id` and `Name` are set to empty strings; `Configuration` is an empty dictionary; `CreatedAt` is set to the current UTC time. |
| `AnalysisRule(string id, string name, string description, RuleCategory category, SeverityLevel defaultSeverity, bool isEnabled, string? rulePattern, string? documentationUrl, string? author, Version? version)` | Initializes a new instance with the specified values. `Configuration` is initialized as an empty dictionary; `CreatedAt` is set to the current UTC time. |

### Methods

#### `bool IsValid`

Returns `true` if the rule is considered valid. A rule is valid when `Id` is not null or empty, `Name` is not null or empty, and `Category` is a defined enum value. Does not throw.

#### `T? GetConfigurationValue<T>(string key)`

Retrieves the configuration value associated with `key` and attempts to cast it to type `T`.  
- **Parameters**: `key` – the configuration key.  
- **Returns**: The value cast to `T`, or `default(T)` if the key does not exist or the cast fails.  
- **Throws**: `ArgumentNullException` if `key` is `null`.

#### `void SetConfigurationValue<T>(string key, T value)`

Sets the configuration value for the given `key`. If the key already exists, its value is overwritten.  
- **Parameters**: `key` – the configuration key; `value` – the value to store.  
- **Throws**: `ArgumentNullException` if `key` is `null`.

#### `AnalysisRule WithSeverity(SeverityLevel severity)`

Returns a new `AnalysisRule` instance that is a copy of the current rule, but with `DefaultSeverity` set to the specified `severity`. The original instance remains unchanged.  
- **Parameters**: `severity` – the new severity level.  
- **Returns**: A new `AnalysisRule` with the updated severity.  
- **Throws**: Nothing.

#### `void MarkAsModified()`

Sets `ModifiedAt` to the current UTC time. Does nothing if `ModifiedAt` is already set to a value equal to the current time (within typical resolution).  
- **Throws**: Nothing.

## Usage

### Example 1: Creating and configuring a rule

```csharp
var rule = new AnalysisRule(
    id: "RULES001",
    name: "Avoid Magic Numbers",
    description: "Flags numeric literals other than 0 and 1.",
    category: RuleCategory.Maintainability,
    defaultSeverity: SeverityLevel.Warning,
    isEnabled: true,
    rulePattern: @"\b\d+\b",
    documentationUrl: "https://docs.example.com/RULES001",
    author: "Team A",
    version: new Version(1, 0)
);

// Add a custom configuration value
rule.SetConfigurationValue("maxAllowed", 10);

// Retrieve the value
int? max = rule.GetConfigurationValue<int>("maxAllowed");
Console.WriteLine(max); // 10

// Mark as modified
rule.MarkAsModified();
Console.WriteLine(rule.ModifiedAt); // current UTC time
```

### Example 2: Immutable severity change and validation

```csharp
var original = new AnalysisRule
{
    Id = "RULES002",
    Name = "Check Null Checks",
    Description = "Ensures null checks are present.",
    Category = RuleCategory.Reliability,
    DefaultSeverity = SeverityLevel.Hidden,
    IsEnabled = false
};

// Create a new rule with elevated severity
var elevated = original.WithSeverity(SeverityLevel.Error);

Console.WriteLine(original.DefaultSeverity); // Hidden (unchanged)
Console.WriteLine(elevated.DefaultSeverity); // Error

// Validate both
Console.WriteLine(original.IsValid); // True (Id and Name are set)
Console.WriteLine(elevated.IsValid); // True
```

## Notes

- **Thread safety**: Instances of `AnalysisRule` are not thread-safe. Concurrent reads and writes to the `Configuration` dictionary or calls to `SetConfigurationValue` / `MarkAsModified` from multiple threads may cause data corruption. External synchronization is required.
- **Configuration dictionary**: The `Configuration` property is never `null`; it is initialized as an empty dictionary in both constructors. However, it can be replaced entirely by assigning a new `Dictionary<string, object>`.
- **`GetConfigurationValue<T>`**: Uses a direct cast (`(T)value`). If the stored value is not assignable to `T`, the method returns `default(T)` without throwing. This differs from a hard cast that would throw `InvalidCastException`.
- **`IsValid`**: Does not check `RulePattern`, `Configuration`, or other optional fields. A rule with an empty `Id` or `Name` is considered invalid.
- **`MarkAsModified`**: Only updates `ModifiedAt` if the rule has not already been modified in the same tick. In practice, repeated calls within a very short time span may not change the value.
- **`WithSeverity`**: Creates a shallow copy of the current rule. The `Configuration` dictionary is shared between the original and the copy; modifications to the dictionary on one instance will affect the other. To avoid this, clone the dictionary before passing it to the new instance if isolation is required.
