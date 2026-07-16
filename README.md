# Roslyn Guard Analyzer

...

## RuleRegistry

The `RuleRegistry` class is a centralized repository for managing architectural analysis rules. It provides methods for registering, retrieving, and manipulating rules, making it easier to manage and extend the rule set.

### Usage Example

```csharp
var registry = new RuleRegistry();

// Register a new rule
var rule = new AnalysisRule(
    "my-rule",
    "My Rule",
    "This is my rule",
    RuleCategory.LayerDependency)
{
    DefaultSeverity = SeverityLevel.Error,
    Author = "John Doe",
    Version = new Version(1, 0, 0)
};

registry.RegisterRule(rule);

// Get a rule by its ID
var existingRule = registry.GetRule("my-rule");

// Get all registered rules
var allRules = registry.GetAllRules();

// Get rules filtered by category
var layerRules = registry.GetRulesByCategory(RuleCategory.LayerDependency);

// Remove a rule
registry.RemoveRule("my-rule");

// Get the total count of registered rules
var ruleCount = registry.GetRuleCount();

// Get enabled rules only
var enabledRules = registry.GetEnabledRules();

// Clear all registered rules
registry.Clear();
```

...
