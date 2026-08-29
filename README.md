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