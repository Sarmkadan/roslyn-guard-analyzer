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
