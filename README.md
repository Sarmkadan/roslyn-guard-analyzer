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

## ProjectRepository

The `ProjectRepository` class manages persistence of analyzed projects, providing methods for querying, saving, loading, and managing project data. It serves as a centralized repository for all projects analyzed by the Roslyn Guard Analyzer, enabling efficient filtering by various criteria such as target framework, language, file count, and analysis date.

### Usage Example

```csharp
// Create a project repository with default data directory
var projectRepository = new ProjectRepository();

// Load projects from disk
await projectRepository.LoadAsync();

// Add a new project
var newProject = new AnalysisProject(
    "MyWebApp",
    "/path/to/MyWebApp.csproj",
    "CSharp",
    "net8.0",
    42);
projectRepository.Add(newProject.Id, newProject);

// Get projects by target framework
var net8Projects = projectRepository.GetByTargetFramework("net8.0");

// Get modern .NET projects
var modernProjects = projectRepository.GetModernDotNetProjects();

// Get projects with more than 10 files
var largeProjects = projectRepository.GetWithMoreFilesThan(10);

// Get projects analyzed after a specific date
var recentProjects = projectRepository.GetAnalyzedAfter(DateTime.Now.AddDays(-7));

// Search projects by name pattern
var matchingProjects = projectRepository.SearchByName("Web");

// Find a project by path
var foundProject = projectRepository.FindByPath("/path/to/MyWebApp.csproj");

// Get projects with referenced dependencies
var projectsWithReferences = projectRepository.GetWithReferences();

// Get repository statistics
var stats = projectRepository.GetStatistics();
Console.WriteLine($"Total projects: {stats.TotalProjects}");
Console.WriteLine($"Modern .NET projects: {stats.ModernDotNetProjects}");
Console.WriteLine($"Total files: {stats.TotalFiles}");

// Save projects to disk
await projectRepository.SaveAsync();

// Export projects to a file
await projectRepository.ExportAsync("projects-backup.json");

// Import projects from a file
await projectRepository.ImportAsync("projects-backup.json");

// Remove a project
await projectRepository.RemoveProjectAsync("MyWebApp");

// Validate and cleanup invalid projects
projectRepository.ValidateAndCleanup();
```

## RuleRepository

The `RuleRepository` class manages persistence of analysis rules, providing methods for querying, saving, loading, enabling/disabling rules, and managing rule configurations. It serves as a centralized repository for all rules used by the Roslyn Guard Analyzer, enabling efficient filtering by category, severity, creation date, and enabled status.

### Usage Example

```csharp
// Create a rule repository with default data directory
var ruleRepository = new RuleRepository();

// Load rules from disk
await ruleRepository.LoadAsync();

// Add a new rule
var newRule = new AnalysisRule(
    "my-rule",
    "My Rule",
    "This is my custom rule",
    RuleCategory.LayerDependency)
{
    DefaultSeverity = SeverityLevel.Error,
    Author = "John Doe",
    Version = new Version(1, 0, 0),
    IsEnabled = true
};
ruleRepository.Add(newRule.Id, newRule);

// Get rules by category
var layerRules = ruleRepository.GetByCategory(RuleCategory.LayerDependency);

// Get enabled rules only
var enabledRules = ruleRepository.GetEnabledRules();

// Get rules by severity
var errorRules = ruleRepository.GetBySeverity(SeverityLevel.Error);

// Get rules created after a specific date
var recentRules = ruleRepository.GetCreatedAfter(DateTime.Now.AddDays(-30));

// Disable a rule
var disableSuccess = ruleRepository.DisableRule("my-rule");

// Enable a rule
var enableSuccess = ruleRepository.EnableRule("my-rule");

// Get repository statistics
var stats = ruleRepository.GetStatistics();
Console.WriteLine($"Total rules: {stats.TotalRules}");
Console.WriteLine($"Enabled rules: {stats.EnabledRules}");
Console.WriteLine($"Disabled rules: {stats.DisabledRules}");
Console.WriteLine($"Enabled percentage: {stats.GetEnabledPercentage():F2}%");
Console.WriteLine("Rules by category:");
foreach (var kvp in stats.RulesByCategory)
{
    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
}

// Save rules to disk
await ruleRepository.SaveAsync();

// Export rules to a file
await ruleRepository.ExportAsync("rules-backup.json");

// Import rules from a file
await ruleRepository.ImportAsync("rules-backup.json");

// Get the data directory path
var dataDir = ruleRepository.GetDataDirectory();
Console.WriteLine($"Data directory: {dataDir}");
```

## RepositoryBase

The `RepositoryBase<T>` class is an abstract base repository that provides common CRUD operations for managing entities in memory. It serves as a foundation for concrete repository implementations, offering thread-safe methods for adding, retrieving, updating, and removing entities, along with additional utility methods for bulk operations and searching.

## AnalysisResultRepository

The `AnalysisResultRepository` class provides methods to retrieve and manage analysis results, including getting results by project, analyzed after a specific date, failed analyses, successful analyses, results with violations in a specific category, and the latest result for a project. It also includes methods to save analysis results asynchronously, load all results asynchronously, export results to CSV asynchronously, get statistics, and clear old results asynchronously.

### Usage Example

```csharp
// Create an AnalysisResultRepository with default data directory
var analysisResultRepository = new AnalysisResultRepository();

// Load all analysis results from disk
await analysisResultRepository.LoadAllAsync();

// Add a new analysis result
var newResult = new AnalysisResult
{
    ProjectName = "MyWebApp",
    Timestamp = DateTime.UtcNow,
    Violations = new List<RuleViolation>
    {
        new RuleViolation("rule-1", "LayerDependency", "Namespace1.Class1", "Namespace2.Class2", 42)
    }
};
analysisResultRepository.Add(newResult);

// Get results by project
var webAppResults = analysisResultRepository.GetByProject("MyWebApp");

// Get results analyzed after a specific date
var recentResults = analysisResultRepository.GetAnalyzedAfter(DateTime.Now.AddDays(-7));

// Get failed analyses
var failedAnalyses = analysisResultRepository.GetFailedAnalyses();

// Get successful analyses
var successfulAnalyses = analysisResultRepository.GetSuccessfulAnalyses();

// Get results with violations in a specific category
var layerViolations = analysisResultRepository.GetWithViolationsInCategory("LayerDependency");

// Get the latest result for a project
var latestResult = analysisResultRepository.GetLatestForProject("MyWebApp");

// Get results with violation count greater than a threshold
var highViolationResults = analysisResultRepository.GetWithViolationCountGreaterThan(5);

// Get repository statistics
var stats = analysisResultRepository.GetStatistics();
Console.WriteLine($"Total analyses: {stats.TotalAnalyses}");
Console.WriteLine($"Successful analyses: {stats.SuccessfulAnalyses}");
Console.WriteLine($"Failed analyses: {stats.FailedAnalyses}");
Console.WriteLine($"Average violation count: {stats.AverageViolationCount}");
Console.WriteLine($"Total violations: {stats.TotalViolations}");
Console.WriteLine($"Average analysis duration: {stats.AverageAnalysisDurationSeconds} seconds");
Console.WriteLine($"Projects analyzed: {stats.ProjectsAnalyzed}");

// Save results to disk
await analysisResultRepository.SaveAsync();

// Export results to CSV
await analysisResultRepository.ExportToCsvAsync("analysis-results.csv");

// Clear old results
await analysisResultRepository.ClearOldResultsAsync(TimeSpan.FromDays(30));
```

## RepositoryBase

### Usage Example

```csharp
// Create a concrete repository by inheriting from RepositoryBase<T>
public class UserRepository : RepositoryBase<User>
{
    // Add custom methods specific to User entities
    public User? GetByEmail(string email)
    {
        return Find(u => u.Email == email).FirstOrDefault();
    }
}

// Usage
var userRepository = new UserRepository();

// Add entities
userRepository.Add("user1", new User { Id = "user1", Name = "Alice", Email = "alice@example.com" });
userRepository.Add("user2", new User { Id = "user2", Name = "Bob", Email = "bob@example.com" });

// Get an entity by ID
var user = userRepository.GetById("user1");
Console.WriteLine($"Retrieved user: {user?.Name}");

// Get all entities
var allUsers = userRepository.GetAll();
Console.WriteLine($"Total users: {userRepository.Count()}");

// Update an entity
userRepository.Update("user1", new User { Id = "user1", Name = "Alice Updated", Email = "alice.new@example.com" });

// Check if entity exists
var exists = userRepository.Exists("user2");
Console.WriteLine($"User2 exists: {exists}");

// Find entities using a predicate
var usersWithNameA = userRepository.Find(u => u.Name.StartsWith("A"));
Console.WriteLine($"Users with name starting with 'A': {usersWithNameA.Count}");

// Add multiple entities at once
var newUsers = new Dictionary<string, User>
{
    { "user3", new User { Id = "user3", Name = "Charlie", Email = "charlie@example.com" } },
    { "user4", new User { Id = "user4", Name = "Diana", Email = "diana@example.com" } }
};
userRepository.AddRange(newUsers);

// Remove an entity
var removed = userRepository.Remove("user4");
Console.WriteLine($"User removed: {removed}");

// Clear all entities
userRepository.Clear();
Console.WriteLine($"Repository cleared. Count: {userRepository.Count()}");
```

## ConfigurationValidator

The `ConfigurationValidator` class provides comprehensive validation capabilities for configuration objects used throughout the Roslyn Guard Analyzer. It validates analysis configurations, CLI options, rule names, and performs comprehensive cross-validation across all configuration components. The validator collects both errors and warnings, providing detailed feedback about configuration issues.

### Usage Example

```csharp
// Validate an analysis configuration
var analysisConfig = new AnalysisConfig
{
    MinimumSeverity = "High",
    MaxViolationsToReport = 100,
    OutputFormat = "json",
    EnabledRules = new List<string> { "LayerDependency", "NamingConvention" },
    ExcludePatterns = new List<string> { "**/Tests/**", "**/bin/**" }
};

var analysisValidation = ConfigurationValidator.ValidateAnalysisConfig(analysisConfig);
if (!analysisValidation.IsValid)
{
    Console.WriteLine("Analysis configuration errors:");
    foreach (var error in analysisValidation.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate CLI options
var cliOptions = new Cli.CliOptions
{
    ProjectPath = "./src/MySolution.sln",
    AnalysisTimeoutSeconds = 900,
    MaxParallelThreads = Environment.ProcessorCount,
    OutputFormat = "json"
};

var cliValidation = ConfigurationValidator.ValidateCliOptions(cliOptions);
if (!cliValidation.IsValid)
{
    Console.WriteLine("CLI options errors:");
    foreach (var error in cliValidation.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate rule names against supported rules
var supportedRules = new List<string> { "LayerDependency", "NamingConvention", "AsyncPatterns" };
var ruleValidation = ConfigurationValidator.ValidateRuleNames(
    new List<string> { "LayerDependency", "UnknownRule" },
    supportedRules
);

if (!ruleValidation.IsValid)
{
    Console.WriteLine("Invalid rule names:");
    foreach (var error in ruleValidation.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Perform comprehensive validation combining multiple configuration components
var comprehensiveValidation = ConfigurationValidator.ValidateComprehensive(analysisConfig, cliOptions);

Console.WriteLine(comprehensiveValidation.ToString());
```

## ConfigurationLoader

The `ConfigurationLoader` class is responsible for loading and parsing configuration files for the Roslyn Guard Analyzer. It supports loading from specific file paths or automatically searching for default configuration files (`.roslyn-guard.json`) in project directories and parent directories. The loader parses JSON configuration into strongly-typed `AnalysisConfig` objects with support for rules, exclusion patterns, severity levels, and output formatting options.

### Usage Example

```csharp
// Load configuration from a specific file path
var config = await ConfigurationLoader.LoadFromFileAsync(".roslyn-guard.json");

Console.WriteLine($"Loaded {config.EnabledRules.Count} enabled rules");
Console.WriteLine($"Exclude patterns: {string.Join(", ", config.ExcludePatterns)}");
Console.WriteLine($"Minimum severity: {config.MinimumSeverity}");
Console.WriteLine($"Max violations to report: {config.MaxViolationsToReport}");
Console.WriteLine($"Enable caching: {config.EnableCaching}");
Console.WriteLine($"Output format: {config.OutputFormat}");

// Search for default configuration in project directory and parents
var projectPath = "./src/MySolution.sln";
var defaultConfig = await ConfigurationLoader.TryLoadDefaultAsync(projectPath);

if (defaultConfig != null)
{
  Console.WriteLine("Successfully loaded default configuration");
  
  // Validate the loaded configuration
  if (defaultConfig.Validate(out var errors))
  {
    Console.WriteLine("Configuration is valid");
  }
  else
  {
    Console.WriteLine("Configuration errors:");
    foreach (var error in errors)
    {
      Console.WriteLine($"- {error}");
    }
  }
}
else
{
  Console.WriteLine("No default configuration found");
}

// Create and save a configuration file
var customConfig = new RoslynGuardAnalyzer.Configuration.AnalysisConfig
{
  EnabledRules = new List<string> { "LayerDependency", "NamingConvention", "AsyncPatterns" },
  ExcludePatterns = new List<string> { "**/Tests/**", "**/bin/**", "**/obj/**" },
  MinimumSeverity = "High",
  MaxViolationsToReport = 500,
  EnableCaching = true,
  OutputFormat = "json"
};

var json = @"{
  \"enabledRules\": [\"LayerDependency\", \"NamingConvention\", \"AsyncPatterns\"],
  \"excludePatterns\": [\"**/Tests/**\", \"**/bin/**\", \"**/obj/**\"],
  \"severity\": \"High\",
  \"maxViolations\": 500,
  \"enableCaching\": true,
  \"outputFormat\": \"json\"
}";

await File.WriteAllTextAsync(".roslyn-guard.json", json);
```

## CacheKeyGenerator

The `CacheKeyGenerator` class provides methods for generating consistent and unique cache keys used throughout the Roslyn Guard Analyzer's caching system. It supports various types of cache keys including project analysis, file analysis, rule execution, and code element analysis. The generator uses content-based hashing to detect changes and ensure cache invalidation when source files or configurations change.

### Usage Example

```csharp
// Generate a cache key for a project analysis
var projectKey = CacheKeyGenerator.GenerateProjectAnalysisKey(
    "./src/MySolution.sln",
    "a1b2c3d4e5f6"
);
Console.WriteLine($"Project analysis key: {projectKey}");

// Generate a cache key for a file analysis with content hash
var filePath = "./src/MyClass.cs";
var fileContentHash = CacheKeyGenerator.ComputeFileHash(filePath);
var fileKey = CacheKeyGenerator.GenerateFileAnalysisKey(filePath, fileContentHash);
Console.WriteLine($"File analysis key: {fileKey}");

// Generate a cache key for an analysis result
var resultKey = CacheKeyGenerator.GenerateResultKey(projectKey);
Console.WriteLine($"Result key: {resultKey}");

// Generate a cache key for rule execution
var ruleExecutionKey = CacheKeyGenerator.GenerateRuleExecutionKey(
    "LayerDependency",
    "MyNamespace.MyClass"
);
Console.WriteLine($"Rule execution key: {ruleExecutionKey}");

// Generate a cache key for a code element
var elementKey = CacheKeyGenerator.GenerateCodeElementKey(
    "MyNamespace.MyClass",
    "MyMethod"
);
Console.WriteLine($"Code element key: {elementKey}");

// Generate a composite key from multiple components
var compositeKey = CacheKeyGenerator.CreateCompositeKey(
    "component1",
    "component2",
    "component3"
);
Console.WriteLine($"Composite key: {compositeKey}");

// Generate a pattern key for cache invalidation
var patternKey = CacheKeyGenerator.GeneratePatternKey("analysis_project");
Console.WriteLine($"Pattern key: {patternKey}");

// Compute a simple hash
var simpleHash = CacheKeyGenerator.ComputeHash("some-input-string");
Console.WriteLine($"Simple hash: {simpleHash}");

// Compute file hash
var fileHash = CacheKeyGenerator.ComputeFileHash(filePath);
Console.WriteLine($"File hash: {fileHash}");
```

## CacheService

The `CacheService` class provides an in-memory caching service for storing analysis results and derived data. It supports expiration policies, automatic cleanup of expired entries, and various cache management operations including pattern-based invalidation.

### Usage Example

```csharp
// Create a cache service with a default expiration of 2 hours
var cacheService = new CacheService(TimeSpan.FromHours(2));
```

## CollectionExtensions

The `CollectionExtensions` class provides a set of utility extension methods for working with collections and enumerables in a more convenient and expressive way. It includes methods for batching, filtering, partitioning, finding elements, and handling null values, reducing boilerplate code when working with collections.

### Usage Example

```csharp
// Create a list of sample data
var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
var names = new List<string?> { "Alice", "Bob", null, "Charlie", "David", null, "Eve" };
var items = new List<(string Name, int Value)>
{
    ("Item1", 10), ("Item2", 20), ("Item3", 30), ("Item4", 40)
};

// Batch items into groups of 3
var batches = numbers.Batch(3);
foreach (var batch in batches)
{
    Console.WriteLine(string.Join(", ", batch));
}
// Output: 1, 2, 3
//         4, 5, 6
//         7, 8, 9
//         10

// Get distinct items by a key selector
var distinctNumbers = numbers.DistinctBy(x => x % 3);
Console.WriteLine(string.Join(", ", distinctNumbers));
// Output: 1, 2, 0

// Add items only if they are not null
var result = new List<string>();
result.AddIfNotNull("Valid");
result.AddIfNotNull(null);
result.AddIfNotNull("Another");
Console.WriteLine(string.Join(", ", result));
// Output: Valid, Another

// Add a range of items only if they are not null
var collection = new List<string?>();
collection.AddRangeIfNotNull(new[] { "First", null, "Second", null, "Third" });
Console.WriteLine(string.Join(", ", collection));
// Output: First, Second, Third

// Check if a collection is null or empty
List<int>? nullList = null;
var emptyList = new List<int>();
var populatedList = new List<int> { 1, 2, 3 };

Console.WriteLine(CollectionExtensions.IsNullOrEmpty(nullList));
// Output: True
Console.WriteLine(CollectionExtensions.IsNullOrEmpty(emptyList));
// Output: True
Console.WriteLine(CollectionExtensions.IsNullOrEmpty(populatedList));
// Output: False

// Get a collection or return empty if null
IEnumerable<string>? nullableCollection = null;
var safeCollection = nullableCollection.OrEmpty();
Console.WriteLine(safeCollection.Count());
// Output: 0

// Iterate with index
foreach (var (index, item) in names.WithIndex())
{
    Console.WriteLine($"{index}: {item}");
}
// Output: 0: Alice
//         1: Bob
//         2: 
//         3: Charlie
//         4: David
//         5: 
//         6: Eve

// Get first element or null if collection is empty
var firstOrNull = numbers.FirstOrNull();
Console.WriteLine(firstOrNull);
// Output: 1

// Iterate over collection with ForEach
numbers.ForEach(x => Console.WriteLine(x * 2));
// Output: 2
//         4
//         6
//         8
//         10
//         12
//         14
//         16
//         18
//         20

// Partition a collection into two based on a predicate
var (evens, odds) = numbers.Partition(x => x % 2 == 0);
Console.WriteLine($"Evens: {string.Join(", ", evens)}");
Console.WriteLine($"Odds: {string.Join(", ", odds)}");
// Output: Evens: 2, 4, 6, 8, 10
//         Odds: 1, 3, 5, 7, 9

// Flatten a collection of collections
var nestedLists = new List<List<int>>
{
    new List<int> { 1, 2 },
    new List<int> { 3, 4 },
    new List<int> { 5 }
};
var flattened = nestedLists.Flatten();
Console.WriteLine(string.Join(", ", flattened));
// Output: 1, 2, 3, 4, 5

// Take elements while a condition is true
var taken = numbers.TakeWhile(x => x < 5);
Console.WriteLine(string.Join(", ", taken));
// Output: 1, 2, 3, 4

// Get the mode (most frequent element) of a collection
var mode = new List<int> { 1, 2, 2, 3, 3, 3, 4 }.GetMode();
Console.WriteLine(mode);
// Output: 3
```

// Set a value in the cache with default expiration
cacheService.Set("my-analysis-result", analysisResult);

// Set a value with custom expiration
cacheService.Set("temporary-data", tempData, TimeSpan.FromMinutes(30));

// Try to get a value (returns false if not found or expired)
if (cacheService.TryGet("my-analysis-result", out AnalysisResult? cachedResult))
{
    Console.WriteLine("Retrieved cached analysis result");
    analysisResult = cachedResult!;
}

// Get a value (throws if not found or expired)
try
{
    var result = cacheService.Get<AnalysisResult>("my-analysis-result");
    Console.WriteLine("Successfully retrieved cached result");
}
catch (KeyNotFoundException)
{
    Console.WriteLine("Cache entry not found or expired");
}

// Get a value or return default if not found
var defaultResult = cacheService.GetOrDefault("missing-key", new AnalysisResult());

// Get a value or compute and cache it if not found
var computedResult = await cacheService.GetOrComputeAsync(
    "expensive-computation",
    async () => await PerformExpensiveAnalysisAsync()
);

// Remove a specific key
var wasRemoved = cacheService.Remove("temporary-data");
Console.WriteLine($"Key removed: {wasRemoved}");

// Check if a key exists
var containsKey = cacheService.Contains("my-analysis-result");
Console.WriteLine($"Cache contains key: {containsKey}");

// Get all keys in the cache
var allKeys = cacheService.GetKeys();
Console.WriteLine($"Total keys: {allKeys.Count()}");

// Invalidate all keys matching a pattern
var invalidatedCount = cacheService.InvalidateByPattern("analysis_");
Console.WriteLine($"Invalidated {invalidatedCount} keys with pattern 'analysis_'");

// Remove all expired entries
var expiredCount = cacheService.RemoveExpired();
Console.WriteLine($"Removed {expiredCount} expired entries");

// Clear the entire cache
cacheService.Clear();
Console.WriteLine("Cache cleared");

// Get current cache statistics
Console.WriteLine($"Cache contains {cacheService.Count} items");
```

## RuleConfigurationBuilder

The `RuleConfigurationBuilder` class provides a fluent interface for creating and configuring rule configurations with type safety. It simplifies the creation of rule configurations by providing a chainable API with sensible defaults and validation, making it easier to define rules programmatically.


### Usage Example

```csharp
// Create a custom rule configuration using the builder
var customRuleConfig = new RuleConfigurationBuilder("CustomSecurityRule")
    .WithEnabled(true)
    .WithSeverity("High")
    .WithDescription("Ensures security best practices are followed")
    .WithParameter("RequireEncryption", true)
    .WithParameter("MinimumKeyLength", 256)
    .WithParameter("AllowLegacyAlgorithms", false)
    .Build();

// Use one of the predefined builder methods for common rule types
var namingRuleConfig = RuleConfigurationBuilder.CreateNamingConvention()
    .WithSeverity("Critical")
    .WithParameter("CheckPublicMembers", true)
    .WithParameter("CheckPrivateMembers", true)
    .WithDescription("Strict naming conventions for all public members")
    .Build();

var layerRuleConfig = RuleConfigurationBuilder.CreateLayerDependency()
    .WithSeverity("High")
    .WithParameter("StrictMode", true)
    .WithDescription("Enforce strict layer boundaries")
    .Build();

var asyncRuleConfig = RuleConfigurationBuilder.CreateAsyncPatterns()
    .WithSeverity("Medium")
    .WithParameter("RequireAsyncSuffix", true)
    .WithDescription("Ensure proper async/await patterns")
    .Build();

var nullSafetyConfig = RuleConfigurationBuilder.CreateNullSafety()
    .WithSeverity("High")
    .WithParameter("RequireNullChecks", true)
    .WithDescription("Enforce null safety throughout the codebase")
    .Build();

// Access the built configuration
Console.WriteLine($"Rule: {customRuleConfig.Name}");
Console.WriteLine($"Description: {customRuleConfig.Description}");
Console.WriteLine($"Enabled: {customRuleConfig.GetCustomSetting("Enabled")}");
Console.WriteLine($"Severity: {customRuleConfig.GetCustomSetting("Severity")}");
```

## RoslynGuardAnalyzerOptions

The `RoslynGuardAnalyzerOptions` class provides strongly-typed configuration for the Roslyn Guard Analyzer using the IOptions pattern. It supports validation via DataAnnotations and allows customization of analysis behavior through various properties such as project path, timeout settings, output format, and rule filtering.

### Usage Example

```csharp
// Create configuration with default values
var options = new RoslynGuardAnalyzer.Configuration.RoslynGuardAnalyzerOptions
{
    ProjectPath = "./src/MySolution.sln",
    AnalysisTimeoutSeconds = 900,
    MaxViolationsToReport = 500,
    LogLevel = 3, // Info level
    OutputFormat = "json",
    OutputFile = "analysis-results.json",
    GenerateReport = true,
    ReportType = "detailed",
    FailOnViolations = true,
    SkipCache = false,
    MaxParallelThreads = Environment.ProcessorCount,
    RuleFilter = new List<string> { "LayerDependency", "NamingConvention" },
    ExcludePatterns = new List<string> { "**/Tests/**", "**/bin/**" },
    MinimumSeverity = "High",
    ConfigFile = ".roslyn-guard.json"
};

// Validate configuration
var validationErrors = options.Validate();
if (validationErrors.Any())
{
    Console.WriteLine("Configuration errors:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Merge with CLI options (CLI options take priority)
var cliOptions = new RoslynGuardAnalyzer.Cli.CliOptions
{
    ProjectPath = "./src/MyProject.csproj",
    OutputFormat = "html",
    OutputFile = "report.html"
};
options.MergeWithCliOptions(cliOptions);

// Display configuration summary
Console.WriteLine(options.ToString());
```

...
