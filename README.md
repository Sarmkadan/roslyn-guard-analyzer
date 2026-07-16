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

## ValidationService

The `ValidationService` class provides comprehensive validation capabilities for rule configurations, projects, code elements, and analysis results. It validates rule configurations, project paths, code elements, and analysis results, ensuring that all inputs meet the required format and structural requirements before processing.

### Usage Example

```csharp
var validationService = new ValidationService();

// Validate a rule configuration
var configValidation = validationService.ValidateRuleConfiguration(
    new RuleConfiguration
    {
        RuleId = "my-rule",
        Enabled = true,
        Severity = SeverityLevel.Warning,
        Parameters = new Dictionary<string, string> { { "threshold", "10" } }
    });

if (!configValidation.IsValid)
{
    Console.WriteLine("Configuration errors:");
    foreach (var error in configValidation.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate a project path
var pathValidation = validationService.ValidateProjectPath("src/MyProject.csproj");
if (!pathValidation.IsValid)
{
    Console.WriteLine($"Invalid project path: {pathValidation.Error}");
}

// Validate a rule
var ruleValidation = validationService.ValidateRule(
    new AnalysisRule(
        "my-rule",
        "My Rule",
        "Description",
        RuleCategory.LayerDependency)
    {
        DefaultSeverity = SeverityLevel.Error
    });

if (!ruleValidation.IsValid)
{
    Console.WriteLine("Rule validation errors:");
    foreach (var error in ruleValidation.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate a code element identifier
var identifierValid = ValidationService.IsValidIdentifier("MyClass");
Console.WriteLine($"Is 'MyClass' a valid identifier? {identifierValid}");

// Validate naming conventions
var isPascalCase = ValidationService.IsPascalCase("MyClassName");
var isCamelCase = ValidationService.IsCamelCase("myVariableName");

Console.WriteLine($"Is PascalCase: {isPascalCase}");
Console.WriteLine($"Is CamelCase: {isCamelCase}");
```

...
