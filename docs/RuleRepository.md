# RuleRepository

The `RuleRepository` class provides a centralized store for `AnalysisRule` objects used by the roslyn-guard-analyzer. It supports loading and saving rules to persistent storage, querying rules by category, severity, or creation date, enabling or disabling individual rules, and exporting/importing rule sets. The repository also exposes aggregate statistics about the rules it contains.

## API

### `public RuleRepository()`

Initializes a new empty instance of the `RuleRepository`. No rules are loaded until `LoadAsync` is called or rules are added via import.

### `public IReadOnlyList<AnalysisRule> GetByCategory(string category)`

Returns all rules whose `Category` property matches the specified `category` string.  
**Parameters:**  
- `category` – The category to filter by.  
**Returns:** A read-only list of matching `AnalysisRule` objects. If no rules match, an empty list is returned.  
**Throws:** `ArgumentNullException` if `category` is `null`.

### `public IReadOnlyList<AnalysisRule> GetEnabledRules()`

Returns all rules that are currently enabled.  
**Returns:** A read-only list of enabled `AnalysisRule` objects. If no rules are enabled, an empty list is returned.

### `public IReadOnlyList<AnalysisRule> GetBySeverity(Severity severity)`

Returns all rules whose `Severity` property matches the specified `severity`.  
**Parameters:**  
- `severity` – The severity level to filter by (e.g., `Warning`, `Error`).  
**Returns:** A read-only list of matching `AnalysisRule` objects. If no rules match, an empty list is returned.

### `public IReadOnlyList<AnalysisRule> GetCreatedAfter(DateTime cutoff)`

Returns all rules whose `CreatedAt` property is later than the specified `cutoff` date and time.  
**Parameters:**  
- `cutoff` – The date/time threshold (in UTC).  
**Returns:** A read-only list of `AnalysisRule` objects created after the cutoff. If no rules match, an empty list is returned.

### `public bool DisableRule(string ruleId)`

Disables the rule with the given identifier.  
**Parameters:**  
- `ruleId` – The unique identifier of the rule to disable.  
**Returns:** `true` if the rule was found and disabled; `false` if no rule with that identifier exists.  
**Throws:** `ArgumentNullException` if `ruleId` is `null`.

### `public bool EnableRule(string ruleId)`

Enables the rule with the given identifier.  
**Parameters:**  
- `ruleId` – The unique identifier of the rule to enable.  
**Returns:** `true` if the rule was found and enabled; `false` if no rule with that identifier exists.  
**Throws:** `ArgumentNullException` if `ruleId` is `null`.

### `public async Task SaveAsync()`

Persists the current state of the repository (including enabled/disabled status) to the default storage location.  
**Throws:** `InvalidOperationException` if the data directory is not accessible or the storage format is invalid.

### `public async Task LoadAsync()`

Loads rules and their states from the default storage location. Any existing rules in the repository are replaced.  
**Throws:** `InvalidOperationException` if the data directory is missing or the storage file is corrupt.

### `public async Task ExportAsync(string filePath)`

Exports the current set of rules (including their enabled/disabled states) to a file at the specified path.  
**Parameters:**  
- `filePath` – The full path where the exported data will be written.  
**Throws:** `ArgumentNullException` if `filePath` is `null`; `IOException` if the file cannot be written.

### `public async Task ImportAsync(string filePath)`

Imports rules from a file previously created by `ExportAsync`. The imported rules are merged into the repository, overwriting any existing rules with the same identifier.  
**Parameters:**  
- `filePath` – The full path to the import file.  
**Throws:** `ArgumentNullException` if `filePath` is `null`; `FileNotFoundException` if the file does not exist; `InvalidDataException` if the file format is unrecognized.

### `public string GetDataDirectory()`

Returns the absolute path of the directory used for persistent storage of rules.  
**Returns:** A string containing the directory path.

### `public RuleRepositoryStatistics GetStatistics()`

Returns a snapshot of current repository statistics, including counts of total, enabled, and disabled rules, and the enabled percentage.  
**Returns:** A `RuleRepositoryStatistics` object with the current values.

### `public int TotalRules { get; }`

Gets the total number of rules currently in the repository.

### `public int EnabledRules { get; }`

Gets the number of rules that are currently enabled.

### `public int DisabledRules { get; }`

Gets the number of rules that are currently disabled.

### `public Dictionary<string, int> RulesByCategory { get; }`

Gets a dictionary mapping each category name to the number of rules in that category.

### `public double GetEnabledPercentage()`

Returns the percentage of rules that are enabled, calculated as `(EnabledRules / TotalRules) * 100`.  
**Returns:** A value between 0.0 and 100.0. Returns 0.0 if `TotalRules` is 0.

## Usage

### Example 1: Load rules, query by category, and disable a rule

```csharp
var repo = new RuleRepository();
await repo.LoadAsync();

// Get all rules in the "Security" category
var securityRules = repo.GetByCategory("Security");
foreach (var rule in securityRules)
{
    Console.WriteLine($"Rule: {rule.Id} - {rule.Title}");
}

// Disable a specific rule
bool disabled = repo.DisableRule("CA2100");
if (disabled)
{
    Console.WriteLine("Rule CA2100 disabled.");
    await repo.SaveAsync();
}
```

### Example 2: Export rules, import into another repository, and inspect statistics

```csharp
var sourceRepo = new RuleRepository();
await sourceRepo.LoadAsync();

// Export current rules to a file
await sourceRepo.ExportAsync(@"C:\rules\backup.json");

// Create a new repository and import the backup
var targetRepo = new RuleRepository();
await targetRepo.ImportAsync(@"C:\rules\backup.json");

// Show statistics
var stats = targetRepo.GetStatistics();
Console.WriteLine($"Total: {stats.TotalRules}, Enabled: {stats.EnabledRules}, Disabled: {stats.DisabledRules}");
Console.WriteLine($"Enabled percentage: {targetRepo.GetEnabledPercentage():F1}%");

// List categories
foreach (var kvp in targetRepo.RulesByCategory)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value} rules");
}
```

## Notes

- **Thread safety:** The `RuleRepository` is not thread-safe. Concurrent calls to mutation methods (`DisableRule`, `EnableRule`, `SaveAsync`, `LoadAsync`, `ImportAsync`, `ExportAsync`) from multiple threads may cause data corruption or inconsistent state. External synchronization (e.g., a lock) is required if the repository is shared across threads.
- **Empty repository:** When the repository contains no rules, `GetEnabledPercentage` returns `0.0`, `RulesByCategory` is an empty dictionary, and all query methods return empty lists. `SaveAsync` will still write an empty state to disk.
- **Rule identity:** The `DisableRule` and `EnableRule` methods rely on a unique rule identifier (typically a string like `"CA2100"`). If multiple rules share the same identifier, only the first match is affected.
- **Persistence format:** The internal storage format is not guaranteed to be human-readable. Use `ExportAsync` and `ImportAsync` for portable rule sets.
- **Data directory:** The path returned by `GetDataDirectory` is determined at initialization and may be based on application configuration or a default location. It is not guaranteed to exist until `SaveAsync` is called at least once.
- **Error handling:** All async methods should be awaited and wrapped in try-catch blocks to handle I/O errors, corrupt data, or missing files.
