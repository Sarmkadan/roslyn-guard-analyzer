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

## BackgroundTaskQueue

The `BackgroundTaskQueue` class provides a priority-based queue for managing and processing background tasks asynchronously. It supports task prioritization, cancellation, and graceful shutdown, making it ideal for background processing scenarios in long-running applications.

### Usage Example

```csharp
// Create and configure the background task queue
var taskQueue = new BackgroundTaskQueue();

// Create a task processor to handle dequeued tasks
taskQueue.Start();
var processor = new BackgroundTaskQueue.BackgroundTaskProcessor(taskQueue);

// Enqueue tasks with different priorities
var highPriorityTaskId = taskQueue.EnqueueTask(
    async ct => {
        Console.WriteLine("Processing high priority task...");
        await Task.Delay(1000, ct);
        Console.WriteLine("High priority task completed!");
    },
    priority: 10 // Higher priority
);

var normalPriorityTaskId = taskQueue.EnqueueTask(
    async ct => {
        Console.WriteLine("Processing normal priority task...");
        await Task.Delay(500, ct);
        Console.WriteLine("Normal priority task completed!");
    }
);

var lowPriorityTaskId = taskQueue.EnqueueTask(
    async ct => {
        Console.WriteLine("Processing low priority task...");
        await Task.Delay(200, ct);
        Console.WriteLine("Low priority task completed!");
    },
    priority: -5 // Lower priority
);

Console.WriteLine($"Queued tasks: {taskQueue.Count}");

// Gracefully stop processing when application shuts down
await processor.StopAsync();

// Clear any remaining tasks
taskQueue.Clear();
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

## OutputWriter

The `OutputWriter` class handles writing formatted analysis results to files or the console. It supports multiple output formats and automatically creates directories as needed when writing to files. The class provides methods for writing analysis results, violations, reports, and plain text content.


### Usage Example

```csharp
// Create an OutputWriter with default formatters
var outputWriter = new OutputWriter();

// Get supported formats
var supportedFormats = outputWriter.GetSupportedFormats();
Console.WriteLine("Supported formats: " + string.Join(", ", supportedFormats));

// Check if a format is supported
var isJsonSupported = outputWriter.IsFormatSupported("json");
Console.WriteLine($"Is JSON supported? {isJsonSupported}");

// Write an analysis result to console in JSON format
var analysisResult = new AnalysisResult
{
    ProjectName = "MyProject",
    Timestamp = DateTime.UtcNow,
    Violations = new List<RuleViolation>
    {
        new RuleViolation("rule-1", "LayerDependency", "Namespace1.Class1", "Namespace2.Class2", 42)
    }
};
await outputWriter.WriteResultAsync(analysisResult, "json");

// Write violations to a file in SARIF format
var violations = new List<RuleViolation>
{
    new RuleViolation("rule-1", "LayerDependency", "Namespace1.Class1", "Namespace2.Class2", 42),
    new RuleViolation("rule-2", "Naming", "MyClass", "myClass", 15)
};
await outputWriter.WriteViolationsAsync(violations, "sarif", "violations.sarif");

// Write a report to console in text format
var report = new ViolationReport
{
    ProjectName = "MyProject",
    TotalViolations = 2,
    CriticalViolations = 1,
    WarningViolations = 1
};
await outputWriter.WriteReportAsync(report, "text");

// Write plain text output to a file
await outputWriter.WriteAsync("Analysis completed successfully!", "output/analysis-results.txt");
```

...
