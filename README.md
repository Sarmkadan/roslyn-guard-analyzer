## BaselineService

The BaselineService class is responsible for managing baseline files that store known violations. It provides methods to load and save baselines, as well as filter new violations not present in the baseline.

### Example usage:

```csharp
public async Task<Baseline?> LoadBaselineAsync(string filePath)
public async Task SaveBaselineAsync(Baseline baseline, string filePath)
public List<RuleViolation> FilterNewViolations(List<RuleViolation> violations, Baseline? baseline, TimeSpan baselineExpiration = default)
public Baseline CreateBaseline(AnalysisResult result)
public Baseline CreateBaseline(string projectName, List<RuleViolation> violations)
```

These methods can be used to manage baselines and filter new violations in a .NET application.

## ParallelAnalysisConfig

The `ParallelAnalysisConfig` class provides configuration options for controlling parallel execution during code analysis. It allows developers to tune concurrency levels for both project-level and rule-level operations to optimize performance based on available system resources.

### Example usage:

```csharp
var config = new ParallelAnalysisConfig();
ParallelAnalysisConfig.MaxDegreeOfParallelism = 4;
ParallelAnalysisConfig.MaxRuleParallelism = 2;

var service = new AnalysisService();
var result = await service.AnalyzeProjectAsync("MyProject.csproj");
var fileResult = await service.AnalyzeFileAsync("Program.cs");
```

These members enable fine-grained control over parallel analysis tasks in a .NET application.

## ServiceCollectionExtensionsValidationTests

The ServiceCollectionExtensionsValidationTests class contains unit tests for the AnalyzerConfiguration validation extension methods in the ServiceCollectionExtensionsValidation class.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Tests;

var tests = new ServiceCollectionExtensionsValidationTests();
tests.Validate_WithValidConfiguration_ReturnsEmptyList();
```

This test class verifies validation logic for analyzer configuration settings such as data directory paths, violation limits, timeouts, log levels, parallel threads, and report formats.

## RoslynGuardExceptionTests

The RoslynGuardExceptionTests class contains unit tests for the RoslynGuardException base class and its derived exception classes (RuleNotFoundException, AnalysisException, ConfigurationException, etc.). These tests verify the behavior of custom exceptions used throughout the Roslyn Guard analyzer, including error code setting, message formatting, and ToString overrides.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Tests;

var tests = new RoslynGuardExceptionTests();
tests.RoslynGuardException_DefaultConstructor_SetsDefaultErrorCodeAndOccurredAt();
```

This test class ensures that custom exceptions correctly initialize properties, handle inner exceptions, and produce formatted string representations as expected.

## FormatterOutputTests
The FormatterOutputTests class contains unit tests for the output formatters (CSV, JSON, HTML) to ensure they produce valid output. It verifies that the formatters correctly handle various inputs, including special characters, and produce the expected output format.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Tests.Formatters;

var tests = new FormatterOutputTests();
tests.CsvFormatter_Format_ReturnsNonEmptyString();
```

## CliArgumentParserTests
The CliArgumentParserTests class contains unit tests for the CliArgumentParser class, verifying that command-line arguments are correctly parsed into configuration options.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Tests.Cli;

var tests = new CliArgumentParserTests();
tests.Parse_EmptyArgs_ReturnsDefaultOptions();
```

## CliArgumentParser

The `CliArgumentParser` class (in `RoslynGuardAnalyzer.Cli`) converts command-line arguments into a `CliOptions` instance. It supports flags, positional project paths, options written as either `--option=value` or `--option value`, and nested response files referenced with `@filename`. Response-file expansion is protected by recursion, file-size, total-length, and argument-count limits.

### Public API:

```csharp
public sealed class CliArgumentParser
public CliArgumentParser(string[] args)
public CliOptions Parse()
public static CliOptions ParseSafe(string[] args)
```

`Parse` returns the parsed options and throws `ArgumentException` when a required option value is missing or response-file processing exceeds its limits. `ParseSafe` is intended for CLI entry points: it writes parsing errors to standard error and returns default options with `ShowHelp` enabled.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Cli;

var arguments = new[]
{
    "--project", "src/MyProject.csproj",
    "--format=json",
    "--threads", "4",
    "--verbose"
};

var options = new CliArgumentParser(arguments).Parse();

Console.WriteLine(options.ProjectPath);        // src/MyProject.csproj
Console.WriteLine(options.OutputFormat);       // json
Console.WriteLine(options.MaxParallelThreads); // 4
Console.WriteLine(options.Verbose);             // True

// For a CLI entry point that should show help instead of throwing on invalid input:
var safeOptions = CliArgumentParser.ParseSafe(args);
```

## ServiceCollectionExtensionsTests

The ServiceCollectionExtensionsTests class contains unit tests for the ServiceCollectionExtensions class, which provides extension methods for registering analyzer services in the dependency injection container. It tests various registration scenarios including null checks, validation, and configuration of analyzer services.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Tests;
using Microsoft.Extensions.DependencyInjection;

var tests = new ServiceCollectionExtensionsTests();
var services = new ServiceCollection();
tests.RegisterAnalyzerServices_WithValidServiceCollection_RegistersAllServices(services);
```

## CodeFixServiceTests

The CodeFixServiceTests class contains unit tests for the CodeFixService class, which provides code fix functionality for known violations in the Roslyn Guard analyzer.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Tests;
using System.Threading.Tasks;

var tests = new CodeFixServiceTests();
await tests.GetFixesAsync_WithKnownViolations_ReturnsExpectedFixes();
```

## AnalysisStartedEventTests

The AnalysisStartedEventTests class contains unit tests for the AnalysisStartedEvent class, which represents the event that fires when analysis starts for a project. It verifies the event's properties, inheritance, and behavior under various input conditions.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Tests;

var tests = new AnalysisStartedEventTests();
tests.Constructor_WithRequiredProperties_InitializesCorrectly();
```

## CustomRuleBuilderEdgeTests

The CustomRuleBuilderEdgeTests class contains unit tests for edge cases in the CustomRuleBuilder class. It verifies the behavior of the builder when dealing with invalid inputs, duplicate configurations, and default values.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Tests;

var tests = new CustomRuleBuilderEdgeTests();
tests.Build_WithoutPredicate_ThrowsInvalidOperationException();
```

## EventBusTests

The EventBusTests class contains unit tests for the EventBus class, which manages publish-subscribe messaging within the analyzer. It verifies subscription handling, event publishing, inheritance behavior, and error conditions.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Tests;

var tests = new EventBusTests();
await tests.PublishAsync_WithSingleSubscriber_InvokesHandler();
```

## RoslynGuardExceptionExtensionsTests

The RoslynGuardExceptionExtensionsTests class contains unit tests for the RoslynGuardExceptionExtensions class, which provides extension methods for formatting error reports, generating error summaries, checking criticality, and converting exceptions to property dictionaries.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Tests;

var tests = new RoslynGuardExceptionExtensionsTests();
tests.FormatErrorReport_RuleNotFoundException_ReturnsFormattedReport();
```

## SuppressRoslynGuardAttributeTests

The SuppressRoslynGuardAttributeTests class contains unit tests for the SuppressRoslynGuard attribute, which is used to exclude code elements from analysis by rule ID.
The tests verify that the attribute correctly filters elements when the rule ID matches, respects case-insensitivity, handles multiple attributes, and respects member-level scope.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Tests.Core;

var tests = new SuppressRoslynGuardAttributeTests();
tests.FilterElements_ClassWithMatchingSuppressAttribute_ElementFiltered();
```

## CacheKeyGenerator

The `CacheKeyGenerator` static class (in `RoslynGuardAnalyzer.Caching`) generates cache keys for analysis results based on project/file characteristics. It uses content hashing to detect changes and invalidate stale cache entries, so that cached analysis results are only reused when the underlying inputs are unchanged.

### Public methods:

```csharp
public static string GenerateProjectAnalysisKey(string? projectPath, string? configHash = null)
public static string GenerateFileAnalysisKey(string filePath, string? fileContentHash = null)
public static string GenerateResultKey(string analysisId)
public static string GenerateRuleExecutionKey(string ruleName, string targetName)
public static string GenerateCodeElementKey(string fullTypeName, string memberName = "")
public static string ComputeHash(string input)
public static string ComputeFileHash(string filePath)
public static string CreateCompositeKey(params string[] components)
public static string GeneratePatternKey(string prefix)
```

### Example usage:

```csharp
using RoslynGuardAnalyzer.Caching;

// Key for a whole-project analysis (path is hashed, config hash is optional)
var projectKey = CacheKeyGenerator.GenerateProjectAnalysisKey(@"C:\src\MyProject.csproj", "config-v1");

// Key for a single file analysis (content hash detects edits)
var fileKey = CacheKeyGenerator.GenerateFileAnalysisKey(@"C:\src\Program.cs", CacheKeyGenerator.ComputeFileHash(@"C:\src\Program.cs"));

// Key for a stored analysis result
var resultKey = CacheKeyGenerator.GenerateResultKey(projectKey);

// Key for a specific rule run against a target
var ruleKey = CacheKeyGenerator.GenerateRuleExecutionKey("AvoidMagicNumbers", "MyClass.MyMethod");

// Composite key from multiple components
var compositeKey = CacheKeyGenerator.CreateCompositeKey("analysis", projectKey, fileKey);

// Prefix pattern for bulk cache invalidation
var pattern = CacheKeyGenerator.GeneratePatternKey("analysis");
```

These methods enable deterministic, content-addressed cache keys for analysis results in a .NET application, allowing stale entries to be invalidated automatically when project or file contents change.

## CacheService

The `CacheService` class (in `RoslynGuardAnalyzer.Caching`) is an in-memory caching service for analysis results and derived data. It supports per-entry expiration policies, cache invalidation by key or prefix pattern, and async compute-on-miss caching. Entries are stored with a UTC expiration timestamp and are lazily evicted when accessed or when `RemoveExpired`/`GetKeys` runs.

### Public API:

```csharp
public sealed class CacheService
public CacheService(TimeSpan? defaultExpiration = null)   // default: 1 hour
public int Count
public void Set<T>(string key, T value)
public void Set<T>(string key, T value, TimeSpan expiration)
public bool TryGet<T>(string key, out T? value)
public T Get<T>(string key)
public T? GetOrDefault<T>(string key, T? defaultValue = default)
public Task<T> GetOrComputeAsync<T>(string key, Func<Task<T>> computeAsync)
public bool Remove(string key)
public void Clear()
public int RemoveExpired()
public bool Contains(string key)
public IEnumerable<string> GetKeys()
public int InvalidateByPattern(string pattern)
```

### Example usage:

```csharp
using RoslynGuardAnalyzer.Caching;

var cache = new CacheService(TimeSpan.FromMinutes(30));

// Set with the default expiration
cache.Set("analysis:project:MyProject", result);

// Set with a custom expiration
cache.Set("analysis:file:Program.cs", fileResult, TimeSpan.FromMinutes(5));

// Try to read a value (returns false if missing or expired)
if (cache.TryGet("analysis:project:MyProject", out AnalysisResult? cached))
{
    // use cached result
}

// Compute and cache on miss
var result = await cache.GetOrComputeAsync("analysis:project:MyProject", async () =>
{
    return await analyzer.AnalyzeProjectAsync("MyProject.csproj");
});

// Invalidate all entries under a prefix
cache.InvalidateByPattern("analysis:project:");
```

This service provides a simple, thread-unsafe in-memory cache for analysis results in a .NET application, letting callers avoid recomputing expensive analysis when the underlying inputs are unchanged.
