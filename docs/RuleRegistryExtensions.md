# RuleRegistryExtensions

Provides extension methods for working with the `RuleRegistry` type, enabling retrieval, inspection, and counting of registered analysis rules.

## API

### `GetRequiredRule`

Retrieves a specific analysis rule by its identifier, throwing if the rule is not found.

- **Parameters**
  - `registry` (`RuleRegistry`): The rule registry to search.
  - `ruleId` (`string`): The identifier of the rule to retrieve.
- **Returns**
  - `AnalysisRule`: The requested rule if it exists.
- **Throws**
  - `ArgumentNullException`: If `registry` or `ruleId` is `null`.
  - `KeyNotFoundException`: If no rule with the specified `ruleId` exists in the registry.

---

### `ContainsRule`

Determines whether the registry contains a rule with the specified identifier.

- **Parameters**
  - `registry` (`RuleRegistry`): The rule registry to check.
  - `ruleId` (`string`): The identifier of the rule to locate.
- **Returns**
  - `bool`: `true` if a rule with the specified `ruleId` exists; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `registry` or `ruleId` is `null`.

---

### `GetRuleCountByCategory`

Returns the number of rules registered under a specific category.

- **Parameters**
  - `registry` (`RuleRegistry`): The rule registry to query.
  - `category` (`string`): The category name to count rules for.
- **Returns**
  - `int`: The number of rules in the specified category.
- **Throws**
  - `ArgumentNullException`: If `registry` or `category` is `null`.

---
### `GetAllRuleIds`

Enumerates all rule identifiers registered in the registry.

- **Parameters**
  - `registry` (`RuleRegistry`): The rule registry to inspect.
- **Returns**
  - `IReadOnlyList<string>`: An immutable list of all rule identifiers in the registry.
- **Throws**
  - `ArgumentNullException`: If `registry` is `null`.

## Usage

```csharp
// Example 1: Retrieve a required rule and validate its presence
var registry = RuleRegistry.CreateDefault();
var ruleId = "RG0001";

if (RuleRegistryExtensions.ContainsRule(registry, ruleId))
{
    var rule = RuleRegistryExtensions.GetRequiredRule(registry, ruleId);
    Console.WriteLine($"Rule '{ruleId}' found: {rule.Title}");
}
else
{
    Console.WriteLine($"Rule '{ruleId}' not found.");
}

// Example 2: Count rules by category and list all identifiers
var category = "Design";
var count = RuleRegistryExtensions.GetRuleCountByCategory(registry, category);
var allIds = RuleRegistryExtensions.GetAllRuleIds(registry);

Console.WriteLine($"Category '{category}' contains {count} rules.");
Console.WriteLine("All rule IDs: " + string.Join(", ", allIds));
```

## Notes

- All methods validate arguments for `null` and throw `ArgumentNullException` immediately.
- `GetRequiredRule` throws `KeyNotFoundException` when the rule is missing, making it suitable for scenarios where rule presence is mandatory.
- `GetAllRuleIds` returns an immutable list, ensuring thread-safe enumeration without risk of modification.
- The registry is assumed to be immutable after creation; concurrent reads are safe, but concurrent modifications are not supported unless externally synchronized.
