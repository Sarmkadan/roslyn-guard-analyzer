# RuleRegistry

The `RuleRegistry` class manages a collection of `AnalysisRule` instances used by the analyzer to evaluate code. It provides methods to add, remove, query, and clear rules, as well as to retrieve subsets such as all rules, rules by category, or only those currently enabled.

## API

### RegisterRule
**Purpose:** Adds a rule to the registry.  
**Parameters:**  
- `rule` – The `AnalysisRule` to register. Must not be `null`.  
**Return value:** `void`.  
**Exceptions:**  
- Throws `ArgumentNullException` if `rule` is `null`.  
- Throws `ArgumentException` if a rule with the same identifier is already registered.

### GetRule
**Purpose:** Retrieves a rule by its unique identifier.  
**Parameters:**  
- `ruleId` – The string identifier of the rule to fetch. Must not be `null` or empty.  
**Return value:** The matching `AnalysisRule`, or `null` if no rule with the given identifier exists.  
**Exceptions:**  
- Throws `ArgumentException` if `ruleId` is `null` or empty.

### GetAllRules
**Purpose:** Returns all rules currently stored in the registry.  
**Parameters:** None.  
**Return value:** An `IReadOnlyList<AnalysisRule>` containing every registered rule. The list is live; modifications to the registry are reflected in the returned collection.  
**Exceptions:** None.

### GetRulesByCategory
**Purpose:** Returns all rules that belong to a specified category.  
**Parameters:**  
- `category` – The category string to filter by. Must not be `null` or empty.  
**Return value:** An `IReadOnlyList<AnalysisRule>` containing rules whose `Category` property matches `category`. Returns an empty list if no rules match.  
**Exceptions:**  
- Throws `ArgumentException` if `category` is `null` or empty.

### RemoveRule
**Purpose:** Attempts to remove a rule from the registry.  
**Parameters:**  
- `rule` – The `AnalysisRule` instance to remove. Must not be `null`.  
**Return value:** `true` if the rule was found and removed; otherwise `false`.  
**Exceptions:**  
- Throws `ArgumentNullException` if `rule` is `null`.

### GetRuleCount
**Purpose:** Gets the number of rules currently registered.  
**Parameters:** None.  
**Return value:** An `int` representing the count of rules in the registry.  
**Exceptions:** None.

### GetEnabledRules
**Purpose:** Returns only the rules that are currently enabled.  
**Parameters:** None.  
**Return value:** An `IReadOnlyList<AnalysisRule>` containing rules where the `IsEnabled` property is `true`.  
**Exceptions:** None.

### Clear
**Purpose:** Removes all rules from the registry.  
**Parameters:** None.  
**Return value:** `void`.  
**Exceptions:** None.

## Usage

```csharp
using RoslynGuardAnalyzer;

// Create a registry and register a couple of rules.
var registry = new RuleRegistry();
var nullCheckRule = new AnalysisRule(
    id: "RG001",
    title: "Null check",
    category: "Usage",
    description: "Warns about possible null dereferences.",
    isEnabled: true);
registry.RegisterRule(nullCheckRule);

var asyncRule = new AnalysisRule(
    id: "RG002",
    title: "Async method naming",
    category: "Naming",
    description: "Ensures async methods end with Async.",
    isEnabled: false);
registry.RegisterRule(asyncRule);

// Retrieve all enabled rules.
IReadOnlyList<AnalysisRule> enabled = registry.GetEnabledRules();
// enabled contains only the nullCheckRule.
```

```csharp
using RoslynGuardAnalyzer;

// Look up a rule by its ID and remove it if present.
var registry = new RuleRegistry();
// ... (rules added elsewhere) ...

string ruleId = "RG002";
AnalysisRule? rule = registry.GetRule(ruleId);
if (rule != null)
{
    bool removed = registry.RemoveRule(rule);
    // removed is true if the rule existed and was deleted.
}

// Get rules belonging to the "Usage" category.
IReadOnlyList<AnalysisRule> usageRules = registry.GetRulesByCategory("Usage");
// usageRules now holds any rules with Category == "Usage".
```

## Notes

- The registry does **not** synchronize access; concurrent calls from multiple threads may result in undefined behavior. External locking is required for thread‑safe usage.  
- Registering a rule with an identifier that already exists throws an `ArgumentException`; callers should check for existence with `GetRule` if they wish to avoid exceptions.  
- `GetAllRules`, `GetRulesByCategory`, and `GetEnabledRules` return live views; modifying the registry after obtaining the list will affect the contents of the returned list.  
- Passing `null` for any parameter that is expected to be non‑null results in an `ArgumentNullException`.  
- Empty strings are not accepted for identifiers or categories and will cause an `ArgumentException`.  
- The `RemoveRule` method returns `false` when the supplied rule instance is not present in the registry; it does not throw in this case.  
- After calling `Clear`, all subsequent query methods will return empty collections and `GetRuleCount` will yield zero.
