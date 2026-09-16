## ValidationService

The `ValidationService` class (in `RoslynGuardAnalyzer.Services`) validates rule configurations, individual analysis rules, project paths, code elements, and completed analysis results. It returns validation failures as error messages instead of throwing for invalid values; null arguments are rejected. Project-path validation expands environment variables, accepts directories or `.csproj`, `.cs`, and `.sln` files, and verifies that the target is accessible. The same source file also provides naming extensions for checking C# identifiers, PascalCase, and camelCase text.

### Public API:

```csharp
public interface IValidationService
public sealed class ValidationService : IValidationService
public (bool IsValid, List<string> Errors) ValidateRuleConfiguration(
    RuleConfiguration config)
public (bool IsValid, List<string> Errors) ValidateRule(AnalysisRule rule)
public (bool IsValid, string? Error) ValidateProjectPath(string projectPath)
public (bool IsValid, List<string> Errors) ValidateCodeElement(
    CodeElement element)
public (bool IsValid, List<string> Errors) ValidateAnalysisResult(
    AnalysisResult result)

public static class ValidationExtensions
public static bool IsValidIdentifier(this string identifier)
public static bool IsPascalCase(this string text)
public static bool IsCamelCase(this string text)
```

`IValidationService` exposes rule-configuration, rule, and project-path validation. The code-element and analysis-result methods are available when using `ValidationService` directly. Rule validation also checks an optional `RulePattern` for valid regular-expression syntax.

### Example usage:

```csharp
using Microsoft.Extensions.DependencyInjection;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Infrastructure;
using RoslynGuardAnalyzer.Services;

var services = new ServiceCollection();
services.RegisterValidationOnly();

using var provider = services.BuildServiceProvider();
var validationService = provider.GetRequiredService<IValidationService>();

var rule = new AnalysisRule(
    "RG001",
    "Public type naming",
    "Public types must use the configured naming convention.",
    RuleCategory.NamingConvention)
{
    RulePattern = "^[A-Z][A-Za-z0-9]*$"
};

var configuration = new RuleConfiguration(
    "Default rules",
    "Rules used by the default analysis profile.");
configuration.AddRule(rule);

var (isValid, errors) =
    validationService.ValidateRuleConfiguration(configuration);

if (!isValid)
{
    foreach (var error in errors)
        Console.Error.WriteLine(error);
}

var (pathIsValid, pathError) =
    validationService.ValidateProjectPath("src/MyProject/MyProject.csproj");

if (!pathIsValid)
    Console.Error.WriteLine(pathError);

Console.WriteLine("CustomerService is an identifier: " +
    "CustomerService".IsValidIdentifier());
```

## ReportingService

The `ReportingService` class (in `RoslynGuardAnalyzer.Services`) turns analysis results into human-readable or machine-readable reports. It can produce a detailed text summary, format an `AnalysisResult` as JSON, CSV, or XML, and save a `ViolationReport` asynchronously. When saving, it creates the parent directory if necessary and chooses the serialized content from the report's `Format`.

### Public API:

```csharp
public interface IReportingService
public sealed class ReportingService : IReportingService
public string GenerateReport(AnalysisResult result)
public string GenerateFormattedReport(AnalysisResult result, string format)
public Task SaveReportAsync(ViolationReport report, string filePath)
```

`GenerateFormattedReport` recognizes `"JSON"`, `"CSV"`, and `"XML"` case-insensitively; any other value produces the standard text report. `SaveReportAsync` serializes `ReportFormat.Json`, `ReportFormat.Csv`, and `ReportFormat.Xml` reports and writes `DetailedContent` for other formats. A null analysis result or report is rejected, as is an empty output path.

### Example usage:

```csharp
using Microsoft.Extensions.DependencyInjection;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Infrastructure;
using RoslynGuardAnalyzer.Services;

var services = new ServiceCollection();
services.RegisterAnalyzerServices();

using var provider = services.BuildServiceProvider();
var analysisService = provider.GetRequiredService<IAnalysisService>();
var reportingService = provider.GetRequiredService<IReportingService>();

var result = await analysisService.AnalyzeProjectAsync(
    "src/MyProject/MyProject.csproj");

var json = reportingService.GenerateFormattedReport(result, "json");
Console.WriteLine(json);

var report = new ViolationReport("Architecture analysis", result.ProjectName)
{
    Format = ReportFormat.Text,
    DetailedContent = reportingService.GenerateReport(result)
};

await reportingService.SaveReportAsync(
    report,
    Path.Combine("artifacts", "reports", "analysis.txt"));
```

## BaselineService

The `BaselineService` class (in `RoslynGuardAnalyzer.Services`) manages JSON baseline files containing violations that have already been accepted. It can create a baseline from analysis results, persist and reload it, and filter a later set of violations so that only findings not present in the baseline are reported. An optional expiration period removes stale baseline entries before filtering.

Paths passed to the service must be non-empty, contain no `..` traversal segments, and use valid path characters. Loading a missing, unreadable, or invalid baseline returns `null` after logging the problem. Saving creates the parent directory when necessary and rethrows write or serialization failures.

### Public API:

```csharp
public interface IBaselineService
public sealed class BaselineService : IBaselineService
public BaselineService(ILogger<BaselineService> logger)
public Task<Baseline?> LoadBaselineAsync(string filePath)
public Task SaveBaselineAsync(Baseline baseline, string filePath)
public List<RuleViolation> FilterNewViolations(
    List<RuleViolation> violations,
    Baseline? baseline,
    TimeSpan baselineExpiration = default)
public Baseline CreateBaseline(AnalysisResult result)
public Baseline CreateBaseline(
    string projectName,
    List<RuleViolation> violations)
```

Both `CreateBaseline` overloads copy the supplied violations into a new `Baseline`. `FilterNewViolations` returns all supplied violations when no usable baseline exists; when `baselineExpiration` is set, it also removes expired entries from the supplied baseline before comparing violations.

### Example usage:

```csharp
using Microsoft.Extensions.DependencyInjection;
using RoslynGuardAnalyzer.Infrastructure;
using RoslynGuardAnalyzer.Services;

var services = new ServiceCollection();
services.RegisterAnalyzerServices();

using var provider = services.BuildServiceProvider();
var analysisService = provider.GetRequiredService<IAnalysisService>();
var baselineService = provider.GetRequiredService<IBaselineService>();

var baselinePath = Path.Combine(".roslyn-guard", "baseline.json");
var previousBaseline = await baselineService.LoadBaselineAsync(baselinePath);
var result = await analysisService.AnalyzeProjectAsync(
    "src/MyProject/MyProject.csproj");

var newViolations = baselineService.FilterNewViolations(
    result.Violations,
    previousBaseline,
    TimeSpan.FromDays(90));

foreach (var violation in newViolations)
    Console.WriteLine($"{violation.RuleId}: {violation.Message}");

// Accept the current analysis results as the baseline for future runs.
var currentBaseline = baselineService.CreateBaseline(result);
await baselineService.SaveBaselineAsync(currentBaseline, baselinePath);
```

## ConfigurationLoader

The `ConfigurationLoader` class (in `RoslynGuardAnalyzer.Configuration`) loads analyzer settings from JSON configuration files into an `AnalysisConfig`. It can load a specific file or search a project directory and each of its parent directories for the default `.roslyn-guard.json` file.

### Public API:

```csharp
public sealed class ConfigurationLoader
public static Task<AnalysisConfig> LoadFromFileAsync(string filePath)
public static Task<AnalysisConfig?> TryLoadDefaultAsync(string projectPath)
```

`LoadFromFileAsync` throws when the path is empty, the file does not exist, or the file cannot be read or parsed. `TryLoadDefaultAsync` treats `projectPath` as a directory, searches upward for the first default configuration file, and returns `null` when none is found. If a discovered file cannot be loaded, it writes a warning to standard error and returns `null`.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Configuration;

var config = await ConfigurationLoader.TryLoadDefaultAsync(projectDirectory);

// Fall back to the built-in AnalysisConfig defaults when no file is available.
config ??= new AnalysisConfig();

if (!config.Validate(out var errors))
{
    foreach (var error in errors)
        Console.Error.WriteLine(error);

    return;
}

Console.WriteLine($"Minimum severity: {config.MinimumSeverity}");
Console.WriteLine($"Output format: {config.OutputFormat}");

// Load a known configuration file directly when discovery is not needed.
var explicitConfig = await ConfigurationLoader.LoadFromFileAsync(
    Path.Combine(projectDirectory, "analyzer-settings.json"));
```

## ConfigurationValidator

The `ConfigurationValidator` class (in `RoslynGuardAnalyzer.Configuration`) validates configuration objects for consistency and correctness. It checks rules, paths, and parameter values to ensure the analyzer configuration is valid before execution.

### Public API:

```csharp
public sealed class ConfigurationValidator
public sealed class ValidationResult
public bool IsValid { get; set; }
public List<string> Errors { get; }
public List<string> Warnings { get; }
public void AddError(string message)
public void AddWarning(string message)
public override string ToString()
public static ValidationResult ValidateAnalysisConfig(AnalysisConfig config)
public static ValidationResult ValidateCliOptions(Cli.CliOptions options)
public static ValidationResult ValidateRuleNames(IEnumerable<string> ruleNames, IEnumerable<string> supportedRules)
public static ValidationResult ValidateComprehensive(AnalysisConfig? analysisConfig, Cli.CliOptions? cliOptions)
```

### ValidationResult

The `ValidationResult` class contains the outcome of validation operations:
- `IsValid`: Boolean indicating if validation passed
- `Errors`: List of error messages that caused validation to fail
- `Warnings`: List of warning messages that don't prevent execution but indicate potential issues
- `AddError(string)`: Adds an error message and marks the result as invalid
- `AddWarning(string)`: Adds a warning message
- `ToString()`: Returns a formatted string representation of the validation result

### Validation Methods

1. **ValidateAnalysisConfig**: Validates an `AnalysisConfig` object
   - Checks minimum severity is one of: Low, Medium, High, Critical
   - Ensures max violations to report is positive
   - Validates output format is one of: text, json, csv, html, xml
   - Checks for duplicate rule names
   - Validates exclude patterns are not empty

2. **ValidateCliOptions**: Validates CLI options
   - Verifies specified file and directory paths exist
   - Ensures analysis timeout and max parallel threads are positive
   - Warns if max parallel threads exceeds reasonable limits

3. **ValidateRuleNames**: Validates that rule names are supported
   - Checks each rule name against a list of supported rules
   - Reports errors for unknown rule names

4. **ValidateComprehensive**: Performs comprehensive validation
   - Combines analysis config and CLI options validation
   - Returns combined validation result with all errors and warnings

### Example usage:

```csharp
using RoslynGuardAnalyzer.Configuration;
using RoslynGuardAnalyzer.Cli;

// Validate analysis configuration
var config = new AnalysisConfig 
{ 
    MinimumSeverity = "Medium",
    MaxViolationsToReport = 100,
    OutputFormat = "json",
    EnabledRules = new[] { "RG-N001", "RG-A001" }
};

var configResult = ConfigurationValidator.ValidateAnalysisConfig(config);
if (!configResult.IsValid)
{
    Console.Error.WriteLine("Configuration errors:");
    Console.Error.WriteLine(configResult);
    return;
}

// Validate CLI options
var cliOptions = new CliOptions
{
    ProjectPath = "./src/MyProject.csproj",
    AnalysisTimeoutSeconds = 30,
    MaxParallelThreads = 4
};

var cliResult = ConfigurationValidator.ValidateCliOptions(cliOptions);
if (!cliResult.IsValid)
{
    Console.Error.WriteLine("CLI options errors:");
    Console.Error.WriteLine(cliResult);
    return;
}

// Comprehensive validation
var combinedResult = ConfigurationValidator.ValidateComprehensive(config, cliOptions);
if (!combinedResult.IsValid)
{
    Console.Error.WriteLine("Validation failed:");
    Console.Error.WriteLine(combinedResult);
    return;
}

Console.WriteLine("Configuration is valid!");
if (combinedResult.Warnings.Count > 0)
{
    Console.WriteLine("Warnings:");
    foreach (var warning in combinedResult.Warnings)
    {
        Console.WriteLine($"  ! {warning}");
    }
}
```

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

## AnalysisService

The `AnalysisService` class (in `RoslynGuardAnalyzer.Services`) orchestrates analysis for either a project or a single C# source file. For projects, it validates the project path, discovers `.cs` files recursively while excluding `bin`, `obj`, and `.git` directories, extracts code elements, and runs all enabled rules through `IRuleEngine`. Both operations return an `AnalysisResult` containing the analyzed elements, violations, file and element counts, and completion status.

### Public API:

```csharp
public interface IAnalysisService
public Task<AnalysisResult> AnalyzeProjectAsync(string projectPath)
public Task<AnalysisResult> AnalyzeFileAsync(string filePath)

public sealed class AnalysisService : IAnalysisService
public AnalysisService(IRuleEngine ruleEngine, IValidationService validationService)
```

`AnalyzeProjectAsync` accepts a project-file path and throws `ConfigurationException` when project-path validation fails. `AnalyzeFileAsync` accepts only an existing `.cs` file and throws `FileAccessException` for a missing or unsupported file. Failures that occur during either analysis operation are wrapped in `AnalysisException`.

### Example usage:

```csharp
using Microsoft.Extensions.DependencyInjection;
using RoslynGuardAnalyzer.Infrastructure;
using RoslynGuardAnalyzer.Services;

var services = new ServiceCollection();
services.RegisterAnalyzerServices();

using var provider = services.BuildServiceProvider();
var analysisService = provider.GetRequiredService<IAnalysisService>();

var projectResult = await analysisService.AnalyzeProjectAsync(
    "src/MyProject/MyProject.csproj");

Console.WriteLine($"Files analyzed: {projectResult.TotalFilesAnalyzed}");
Console.WriteLine($"Elements analyzed: {projectResult.TotalElementsAnalyzed}");
Console.WriteLine($"Violations: {projectResult.ViolationCount}");

var fileResult = await analysisService.AnalyzeFileAsync(
    "src/MyProject/Program.cs");

foreach (var violation in fileResult.Violations)
    Console.WriteLine($"{violation.RuleId}: {violation.Message}");
```

## AnalysisStatisticsService

The `AnalysisStatisticsService` class (in `RoslynGuardAnalyzer.Services`) aggregates `RuleViolation` collections or an `AnalysisResult` into counts by severity, rule, and file. It can also rank the most frequently violated rules and files, calculate severity percentages and a bounded risk score, and produce human-readable summary and health reports. In its severity summaries, `Error`, `Warning`, and `Info` are presented as High, Medium, and Low respectively.

### Public API:

```csharp
public sealed class AnalysisStatisticsService
public sealed class ViolationStatistics
public int TotalCount { get; set; }
public int CriticalCount { get; set; }
public int HighCount { get; set; }
public int MediumCount { get; set; }
public int LowCount { get; set; }
public Dictionary<string, int> ByRule { get; }
public Dictionary<string, int> ByFile { get; }
public Dictionary<SeverityLevel, int> BySeverity { get; }
public int AffectedFiles { get; set; }
public int AffectedRules { get; set; }

public static ViolationStatistics CalculateStatistics(
    IEnumerable<RuleViolation>? violations)
public static ViolationStatistics CalculateStatistics(AnalysisResult? result)
public static List<(string Rule, int Count)> GetTopRulesByViolations(
    IEnumerable<RuleViolation> violations,
    int count = 10)
public static List<(string File, int Count)> GetTopFilesByViolations(
    IEnumerable<RuleViolation> violations,
    int count = 10)
public static Dictionary<string, double> GetSeverityDistribution(
    IEnumerable<RuleViolation> violations)
public static string GenerateSummaryReport(ViolationStatistics stats)
public static int CalculateRiskScore(ViolationStatistics stats)
public static string GetHealthAssessment(ViolationStatistics stats)
```

`CalculateStatistics` requires a non-null violation collection; passing `null`, including through a null `AnalysisResult`, throws `ArgumentNullException`. File totals use full stored paths, while `GetTopFilesByViolations` groups violations by file name. The risk score is capped at 100.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;

var result = new AnalysisResult("Store.Api", "src/Store.Api/Store.Api.csproj");
result.AddViolations(new[]
{
    new RuleViolation("RG-A001", "LayerDependency", "UI depends on Data", "src/Api/Orders.cs")
    {
        Severity = SeverityLevel.Error
    },
    new RuleViolation("RG-N001", "NamingConvention", "Type name is not PascalCase", "src/Api/Models.cs")
    {
        Severity = SeverityLevel.Warning
    }
});

var statistics = AnalysisStatisticsService.CalculateStatistics(result);

Console.WriteLine(AnalysisStatisticsService.GenerateSummaryReport(statistics));
Console.WriteLine($"Risk score: {AnalysisStatisticsService.CalculateRiskScore(statistics)}");
Console.WriteLine(AnalysisStatisticsService.GetHealthAssessment(statistics));

foreach (var (rule, count) in AnalysisStatisticsService.GetTopRulesByViolations(result.Violations, 5))
    Console.WriteLine($"{rule}: {count}");
```

## DiagnosticsService

The `DiagnosticsService` class (in `RoslynGuardAnalyzer.Services`) collects in-memory diagnostics for analysis runs. It tracks completed analyses, elapsed analysis time, violations, and errors, and can report process information such as runtime version, memory usage, processor count, and service uptime. Its statistics operations are thread-safe, and it retains only the 10 most recently recorded error messages.

### Public API:

```csharp
public sealed class DiagnosticsService
public void RecordAnalysis(long durationMs, int violationsFound)
public void RecordError(string errorMessage)
public long GetAverageAnalysisTime()
public int GetAnalysisCount()
public int GetTotalViolationsFound()
public int GetErrorCount()
public IReadOnlyList<string> GetRecentErrors(int count = 5)
public Dictionary<string, object> GetSystemInfo()
public string GenerateDiagnosticReport()
public void Reset()
```

`GetAverageAnalysisTime` returns the integer average in milliseconds, or zero before an analysis is recorded. `GetRecentErrors` returns a read-only snapshot of the newest requested messages. `GenerateDiagnosticReport` combines run statistics, up to five recent errors, and current system information in a human-readable string. `Reset` clears the collected counters and errors; the service uptime continues from when the instance was created.

### Example usage:

```csharp
using System.Diagnostics;
using RoslynGuardAnalyzer.Services;

var diagnostics = new DiagnosticsService();
var stopwatch = Stopwatch.StartNew();

try
{
    // Run analysis and record its violation count.
    var violationsFound = 4;
    stopwatch.Stop();
    diagnostics.RecordAnalysis(stopwatch.ElapsedMilliseconds, violationsFound);
}
catch (Exception exception)
{
    diagnostics.RecordError(exception.Message);
}

Console.WriteLine($"Analyses: {diagnostics.GetAnalysisCount()}");
Console.WriteLine($"Average time: {diagnostics.GetAverageAnalysisTime()} ms");
Console.WriteLine($"Violations: {diagnostics.GetTotalViolationsFound()}");
Console.WriteLine(diagnostics.GenerateDiagnosticReport());
```

## OutputWriter

The `OutputWriter` class (in `RoslynGuardAnalyzer.Services`) formats analyzer output and writes it either to standard output or to a file. It supports complete `AnalysisResult` objects, collections of `RuleViolation` objects, and `ViolationReport` objects through a `FormatterRegistry`, while `WriteAsync` writes unformatted text. When no registry is supplied, the writer uses the default JSON, CSV, HTML, and SARIF formatters.

### Public API:

```csharp
public sealed class OutputWriter
public OutputWriter(FormatterRegistry? formatterRegistry = null)
public Task WriteResultAsync(
    AnalysisResult result,
    string format,
    string? outputFilePath = null)
public Task WriteViolationsAsync(
    IEnumerable<RuleViolation> violations,
    string format,
    string? outputFilePath = null)
public Task WriteReportAsync(
    ViolationReport report,
    string format,
    string? outputFilePath = null)
public Task WriteAsync(string content, string? outputFilePath = null)
public IEnumerable<string> GetSupportedFormats()
public bool IsFormatSupported(string format)
```

An omitted or blank `outputFilePath` sends the content to `Console.Out`. For file output, missing parent directories are created automatically and the file is overwritten; write failures are reported as `IOException`. The formatted methods throw when the requested format is not registered.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;

var result = new AnalysisResult("Store.Api", "src/Store.Api/Store.Api.csproj");
result.AddViolation(new RuleViolation(
    "RG-N001",
    "NamingConvention",
    "Type name should use PascalCase.",
    "src/Store.Api/order_service.cs"));

var writer = new OutputWriter();

// Write JSON to stdout.
await writer.WriteResultAsync(result, "json");

// Create the reports directory if needed and write a SARIF file.
await writer.WriteResultAsync(result, "sarif", "reports/analysis.sarif");

if (writer.IsFormatSupported("csv"))
    await writer.WriteViolationsAsync(result.Violations, "csv", "reports/violations.csv");
```

## RuleRegistry

The `RuleRegistry` class (in `RoslynGuardAnalyzer.Services`) is the in-memory catalog of architectural rules used by the analyzer. Each new registry starts with the built-in layer-dependency, naming-convention, async-pattern, and null-safety rules. Callers can register additional valid `AnalysisRule` instances, look up or remove rules by ID, filter rules by category, and retrieve snapshots of all or only enabled rules.

Rule IDs must be unique. Registering a null, invalid, or duplicate rule throws an exception. ID lookups and category names are case-sensitive, and `GetRulesByCategory` expects the name of a `RuleCategory` value, such as `CodeStructure`. The registry is also registered as the singleton `IRuleRegistry` implementation by `RegisterAnalyzerServices()`.

### Public API:

```csharp
public sealed class RuleRegistry : IRuleRegistry
public RuleRegistry(ILogger<RuleRegistry>? logger = null)
public void RegisterRule(AnalysisRule rule)
public AnalysisRule? GetRule(string ruleId)
public IReadOnlyList<AnalysisRule> GetAllRules()
public IReadOnlyList<AnalysisRule> GetRulesByCategory(string category)
public bool RemoveRule(string ruleId)
public int GetRuleCount()
public IReadOnlyList<AnalysisRule> GetEnabledRules()
public void Clear()
```

### Example usage:

```csharp
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;

var registry = new RuleRegistry();

var codeStructureRule = new AnalysisRule(
    "RG-C001",
    "Controller Size Rule",
    "Flags controllers that contain too many actions.",
    RuleCategory.CodeStructure)
{
    DefaultSeverity = SeverityLevel.Warning,
    IsEnabled = true
};

registry.RegisterRule(codeStructureRule);

var registeredRule = registry.GetRule("RG-C001");
var structureRules = registry.GetRulesByCategory(
    nameof(RuleCategory.CodeStructure));
var enabledRules = registry.GetEnabledRules();

Console.WriteLine($"Registered rules: {registry.GetRuleCount()}");
Console.WriteLine($"Code structure rules: {structureRules.Count}");
Console.WriteLine($"Enabled rules: {enabledRules.Count}");

if (registeredRule is not null)
    Console.WriteLine($"Found {registeredRule.Id}: {registeredRule.Name}");

registry.RemoveRule("RG-C001");
```

## RuleEngine

The `RuleEngine` class (in `RoslynGuardAnalyzer.Services`) executes architectural analysis rules against extracted `CodeElement` instances and returns the resulting `RuleViolation` objects. It supports the built-in layer-dependency, naming-convention, async-pattern, and null-safety categories as well as `CustomAnalysisRule` instances. Disabled rules and suppressed elements are skipped; when all registered rules are executed, rule evaluation is parallelized and the results are returned in deterministic file, line, and rule-ID order.

### Public API:

```csharp
public sealed class RuleEngine : IRuleEngine
public RuleEngine(IRuleRegistry ruleRegistry)
public Task<List<RuleViolation>> ExecuteRuleAsync(
    AnalysisRule rule,
    List<CodeElement> elements)
public Task<List<RuleViolation>> ExecuteAllRulesAsync(
    List<CodeElement> elements)
```

`ExecuteRuleAsync` evaluates one rule and returns an empty list when the rule is disabled or there are no elements to inspect. `ExecuteAllRulesAsync` obtains every enabled rule from the supplied registry and evaluates them with the configured `ParallelAnalysisConfig.MaxRuleParallelism`. Rule failures are reported as warnings while successful results from the remaining rules are preserved.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;

var registry = new RuleRegistry();
var engine = new RuleEngine(registry);

var elements = new List<CodeElement>
{
    new("OrderRepository", CodeElementType.Class, "src/OrderRepository.cs")
    {
        Namespace = "MyApp.Data",
        StartLineNumber = 8
    }
};

var violations = await engine.ExecuteAllRulesAsync(elements);

foreach (var violation in violations)
{
    Console.WriteLine(
        $"{violation.FilePath}:{violation.LineNumber} " +
        $"{violation.RuleId} {violation.Message}");
}

// A specific registered rule can also be executed independently.
var namingRule = registry.GetRule(AnalyzerConstants.DefaultRules.NamingConventionRule);
if (namingRule is not null)
{
    var namingViolations = await engine.ExecuteRuleAsync(namingRule, elements);
    Console.WriteLine($"Naming violations: {namingViolations.Count}");
}
```

## CustomRuleBuilder

The `CustomRuleBuilder` class (in `RoslynGuardAnalyzer.Rules`) provides a fluent API for defining predicate-based analysis rules without creating a new `AnalysisRule` subclass. A completed build produces a `CustomAnalysisRule`, which stores the predicate used to identify violations and the message factory used to describe them.

### Public API:

```csharp
public sealed class CustomAnalysisRule : AnalysisRule
public CustomAnalysisRule(
    string id,
    string name,
    string description,
    RuleCategory category,
    SeverityLevel severity,
    Func<CodeElement, bool> violationPredicate,
    Func<CodeElement, string> messageFactory)
public Func<CodeElement, bool> ViolationPredicate { get; }
public Func<CodeElement, string> MessageFactory { get; }

public sealed class CustomRuleBuilder
public static CustomRuleBuilder Create(string id, string name)
public CustomRuleBuilder For(RuleCategory category)
public CustomRuleBuilder WithSeverity(SeverityLevel severity)
public CustomRuleBuilder WithDescription(string description)
public CustomRuleBuilder When(Func<CodeElement, bool> predicate)
public CustomRuleBuilder WithMessage(string message)
public CustomRuleBuilder WithMessage(Func<CodeElement, string> messageFactory)
public CustomAnalysisRule Build()
```

New builders default to the `CodeStructure` category and `Warning` severity. `When` is required before `Build`; the description defaults to the rule name, and the generated message identifies the rule and matching element when `WithMessage` is omitted. Calling a configuration method more than once replaces its previous value.

### Example usage:

```csharp
using System;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;

var rule = CustomRuleBuilder
    .Create("RG-CUSTOM-001", "AvoidManagerSuffix")
    .For(RuleCategory.NamingConvention)
    .WithSeverity(SeverityLevel.Warning)
    .WithDescription("Type names should describe a specific responsibility.")
    .When(element =>
        element.ElementType == CodeElementType.Class &&
        element.Name.EndsWith("Manager", StringComparison.Ordinal))
    .WithMessage(element => $"Rename '{element.Name}' to a more specific name.")
    .Build();

var element = new CodeElement
{
    Id = "OrderManager",
    Name = "OrderManager",
    ElementType = CodeElementType.Class,
    FilePath = "src/OrderManager.cs"
};

if (rule.ViolationPredicate(element))
    Console.WriteLine(rule.MessageFactory(element));
```

The resulting rule can be registered with `CustomRuleRegistry`, evaluated by `CustomRuleEngine`, or passed to the main `RuleEngine`.

## CustomRuleEngine

The `CustomRuleEngine` class (in `RoslynGuardAnalyzer.Rules`) evaluates predicate-based `CustomAnalysisRule` instances against extracted `CodeElement` objects. It can run a single supplied rule or retrieve every custom rule from an `ICustomRuleRegistry` and combine their violations. Registered rules are evaluated sequentially, and the input elements are materialized once so each rule receives the same collection.

### Public API:

```csharp
public sealed class CustomRuleEngine
public CustomRuleEngine(ICustomRuleRegistry customRuleRegistry)
public Task<List<RuleViolation>> EvaluateRuleAsync(
    CustomAnalysisRule rule,
    IEnumerable<CodeElement> elements,
    CancellationToken cancellationToken = default)
public Task<List<RuleViolation>> EvaluateAsync(
    IEnumerable<CodeElement> elements,
    CancellationToken cancellationToken = default)
```

`EvaluateRuleAsync` evaluates only the supplied rule. `EvaluateAsync` evaluates all rules returned by the registry and aggregates their violations. Both methods honor cancellation before rule evaluation; `EvaluateAsync` also checks for cancellation before each registered rule.

### Example usage:

```csharp
using System;
using System.Collections.Generic;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;

var registry = new CustomRuleRegistry();

var rule = CustomRuleBuilder
    .Create("RG-CUSTOM-001", "AvoidManagerSuffix")
    .For(RuleCategory.NamingConvention)
    .WithSeverity(SeverityLevel.Warning)
    .WithDescription("Type names should describe a specific responsibility.")
    .When(element =>
        element.ElementType == CodeElementType.Class &&
        element.Name.EndsWith("Manager", StringComparison.Ordinal))
    .WithMessage(element => $"Rename '{element.Name}' to a more specific name.")
    .Build();

registry.RegisterCustomRule(rule);
var engine = new CustomRuleEngine(registry);

var elements = new List<CodeElement>
{
    new()
    {
        Id = "OrderManager",
        Name = "OrderManager",
        ElementType = CodeElementType.Class,
        FilePath = "src/OrderManager.cs",
        Namespace = "MyApp.Services",
        StartLineNumber = 7
    }
};

var violations = await engine.EvaluateAsync(elements);

foreach (var violation in violations)
    Console.WriteLine($"{violation.RuleId}: {violation.Message}");

// A rule can also be evaluated directly without registering it.
var directViolations = await engine.EvaluateRuleAsync(rule, elements);
```

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

## CommandLineProcessor

The `CommandLineProcessor` class (in `RoslynGuardAnalyzer.Cli`) is the high-level facade for processing analyzer command-line arguments. It parses arguments into `CliOptions`, prints help or version information when requested, validates the parsed options, reports errors to standard error, and can separately verify that configured project and configuration-file paths exist.

### Public API:

```csharp
public enum ExitCode : int
{
    Success = 0,
    ViolationsFound = 1,
    BadArguments = 2,
    InternalError = 3
}

public sealed class CommandLineProcessor
public CommandLineProcessor(string[] args)
public (bool Success, ExitCode ExitCode, CliOptions Options, List<string> Errors) Process()
public CliOptions GetOptions()
public (bool Valid, List<string> Errors) ValidatePaths()
public void PrintOptionsSummary()
```

Call `Process` before `GetOptions`, `ValidatePaths`, or `PrintOptionsSummary`. `Process` returns `BadArguments` for option-validation failures and `InternalError` for unexpected exceptions; help and version requests are printed and returned as successful processing. `ValidatePaths` performs the separate filesystem existence checks after processing.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Cli;

var processor = new CommandLineProcessor(new[]
{
    "--project", "src/MyProject.csproj",
    "--format", "json"
});

var result = processor.Process();
if (!result.Success)
{
    Environment.ExitCode = (int)result.ExitCode;
    return;
}

var pathValidation = processor.ValidatePaths();
if (!pathValidation.Valid)
{
    foreach (var error in pathValidation.Errors)
        Console.Error.WriteLine(error);

    Environment.ExitCode = (int)ExitCode.BadArguments;
    return;
}

processor.PrintOptionsSummary();
var options = processor.GetOptions();
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

## CodeFixService

The `CodeFixService` class (in `RoslynGuardAnalyzer.CodeFixes`) generates and applies source-code fixes for violations reported by the analyzer. It currently provides fixes for interface naming (`RG-N001`), async method naming (`RG-N002`), missing `ConfigureAwait(false)` (`RG-A001`), and `async void` methods (`RG-A002`). Fixes are grouped by file and applied in reverse line order to avoid shifting later edits; unsupported rules are skipped.

### Public API:

```csharp
public interface ICodeFixService
public Task<IReadOnlyList<CodeFix>> GetFixesAsync(
    IEnumerable<RuleViolation> violations,
    CancellationToken cancellationToken = default)
public Task<CodeFixResult> ApplyFixesAsync(
    IEnumerable<CodeFix> fixes,
    bool dryRun = false,
    CancellationToken cancellationToken = default)

public sealed class CodeFixService : ICodeFixService
public CodeFixService(ILogger<CodeFixService> logger)
```

`GetFixesAsync` converts supported violations into `CodeFix` instances. `ApplyFixesAsync` returns a `CodeFixResult` containing the applied and failed fixes plus diagnostic messages. Set `dryRun` to `true` to validate and report the changes without writing them to disk. Both operations accept a cancellation token.

### Example usage:

```csharp
using Microsoft.Extensions.DependencyInjection;
using RoslynGuardAnalyzer.CodeFixes;
using RoslynGuardAnalyzer.Domain.Models;

var services = new ServiceCollection();
services.AddLogging();
services.AddCodeFixServices();

await using var provider = services.BuildServiceProvider();
var codeFixService = provider.GetRequiredService<ICodeFixService>();

var violations = new[]
{
    new RuleViolation(
        "RG-N001",
        "Interface naming",
        "Interface names should start with 'I'.",
        "Example.cs")
    {
        LineNumber = 1,
        CodeSnippet = "public interface Example"
    }
};

var fixes = await codeFixService.GetFixesAsync(violations);

// Preview the outcome without changing Example.cs.
var preview = await codeFixService.ApplyFixesAsync(fixes, dryRun: true);

if (preview.IsSuccess)
{
    var result = await codeFixService.ApplyFixesAsync(fixes);
    Console.WriteLine($"Applied {result.AppliedFixes.Count} fix(es).");
}
```

`ApplyFixesAsync` only replaces the expected source text on the reported line. A missing file, an out-of-range line, or source text that no longer matches is reported as a failed fix instead of being written.

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

## AsyncVoidRule

The `AsyncVoidRule` static class (in `RoslynGuardAnalyzer.Rules`) creates the built-in rule that detects `async void` methods that are not event handlers. Such methods cannot be awaited by their callers and can make exceptions difficult to observe, so the rule reports them as errors and recommends returning `Task` or marking the method as an event handler. Methods with EventHandler-, EventArgs-, IEventHandler-, Handler-, or Callback-related attributes are treated as event handlers and are not reported.

### Public API:

```csharp
public static class AsyncVoidRule
public const string RuleId = "AV001"
public const string RuleTitle = "Async Void Methods Must Be Event Handlers"
public static CustomAnalysisRule Create()
```

`Create` returns a `CustomAnalysisRule` in the `AsyncPattern` category with `Error` severity. Its predicate only matches method elements whose `IsAsync` property is `true` and whose `ReturnType` is exactly `"void"`.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;

var rule = AsyncVoidRule.Create();
var method = new CodeElement("SaveChanges", CodeElementType.Method, "OrderService.cs")
{
    IsAsync = true,
    ReturnType = "void",
    StartLineNumber = 24,
    EndLineNumber = 31
};

if (rule.ViolationPredicate(method))
{
    Console.WriteLine($"{rule.Id}: {rule.MessageFactory(method)}");
}
```

## EmptyCatchBlockRule

The `EmptyCatchBlockRule` static class (in `RoslynGuardAnalyzer.Rules`) creates the built-in rule that reports catch blocks with no exception-handling statements. An empty catch silently swallows an exception, so rule `ECB001` reports it as an `Error` in the `CodeStructure` category and recommends removing the catch block, rethrowing, logging, or wrapping the exception. Catch blocks containing a `throw` statement or other executable content are not reported.

### Public API:

```csharp
public static class EmptyCatchBlockRule
public static CustomAnalysisRule Create()
```

`Create` returns the configured `CustomAnalysisRule`. The rule evaluates only `CodeElementType.CatchBlock` elements and reads the source file identified by `CodeElement.FilePath`; it does not report a violation when the path is missing, the file does not exist, or the file cannot be read. `CustomRuleRegistry` includes this rule in its built-in rules, so normal registry-based analysis does not require manual registration.

### Example usage:

```csharp
using System.IO;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;

var sourcePath = Path.GetFullPath("OrderService.cs");
var catchBlock = new CodeElement(
    "SaveOrderCatch",
    CodeElementType.CatchBlock,
    sourcePath)
{
    StartLineNumber = 18,
    EndLineNumber = 20
};

var rule = EmptyCatchBlockRule.Create();
if (rule.ViolationPredicate(catchBlock))
{
    Console.WriteLine($"{rule.Id}: {rule.MessageFactory(catchBlock)}");
}
```

## MeaningfulTypeNameRule

The `MeaningfulTypeNameRule` static class (in `RoslynGuardAnalyzer.Rules`) creates the built-in rule that flags public classes, structs, interfaces, and enums whose names appear to be placeholders or are too generic to communicate their purpose. Rule `MTN001` reports these names as an `Error` in the `NamingConvention` category. It detects configured terms such as `Temp`, `Placeholder`, `Foo`, `Helper`, and `Service` case-insensitively, as well as `Result` names with a letter or number suffix, names ending in a digit, and single-letter names other than `T`, `K`, or `V`. Non-public types and non-type code elements are ignored.

### Public API:

```csharp
public static class MeaningfulTypeNameRule
public static CustomAnalysisRule Create()
```

`Create` returns the configured `CustomAnalysisRule`. `CustomRuleRegistry` includes this rule in its built-in rules, so normal registry-based analysis does not require manual registration.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;

var rule = MeaningfulTypeNameRule.Create();
var type = new CodeElement("TemporaryService", CodeElementType.Class, "Orders.cs")
{
    IsPublic = true,
    StartLineNumber = 8,
    EndLineNumber = 14
};

if (rule.ViolationPredicate(type))
{
    Console.WriteLine($"{rule.Id}: {rule.MessageFactory(type)}");
}
```

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

## CsvFormatter

The `CsvFormatter` class (in `RoslynGuardAnalyzer.Formatters`) formats analysis results as CSV (Comma-Separated Values) output suitable for import into spreadsheet applications and data analysis tools.

### Public API:

```csharp
public sealed class CsvFormatter : IOutputFormatter
public string Format => "csv";
public bool CanFormat(string format);
public string FormatResult(AnalysisResult result);
public string FormatViolations(IEnumerable<RuleViolation> violations);
public string FormatReport(ViolationReport report);
```

### Example usage:

```csharp
using RoslynGuardAnalyzer.Formatters;
using RoslynGuardAnalyzer.Core;

// Create formatter instance
var formatter = new CsvFormatter();

// Check if it can handle a format
bool canHandleCsv = formatter.CanFormat("csv"); // returns true
bool canHandleJson = formatter.CanFormat("json"); // returns false

// Format analysis results
string csvOutput = formatter.FormatResult(analysisResult);

// Format violations directly
string violationsCsv = formatter.FormatViolations(violations);

// Format a violation report
string reportCsv = formatter.FormatReport(report);
```

The CSV output includes columns for Rule, Severity, Message, File, Line, Column, and Code, with proper escaping for special characters.

## JsonFormatter

The `JsonFormatter` class (in `RoslynGuardAnalyzer.Formatters`) formats analysis results as JSON output suitable for programmatic consumption and integration with other tools.

### Public API:

```csharp
public sealed class JsonFormatter : IOutputFormatter
public string Format => "json";
public bool CanFormat(string format);
public string FormatResult(AnalysisResult result);
public string FormatViolations(IEnumerable<RuleViolation> violations);
public string FormatReport(ViolationReport report);
```

### Example usage:

```csharp
using RoslynGuardAnalyzer.Formatters;
using RoslynGuardAnalyzer.Core;

// Create formatter instance
var formatter = new JsonFormatter();

// Check if it can handle a format
bool canHandleJson = formatter.CanFormat("json"); // returns true
bool canHandleCsv = formatter.CanFormat("csv"); // returns false

// Format analysis results
string jsonOutput = formatter.FormatResult(analysisResult);

// Format violations directly
string violationsJson = formatter.FormatViolations(violations);

// Format a violation report
string reportJson = formatter.FormatReport(report);
```

The JSON output includes all analysis data in a structured format with proper escaping for special characters. The output is minified JSON suitable for programmatic consumption.

## HtmlFormatter

The `HtmlFormatter` class (in `RoslynGuardAnalyzer.Formatters`) formats analysis results as HTML output suitable for viewing in web browsers. It produces styled, readable HTML with summary statistics and detailed violation information.

### Public API:

```csharp
public sealed class HtmlFormatter : IOutputFormatter
public string Format => "html";
public bool CanFormat(string format);
public string FormatResult(AnalysisResult result);
public string FormatViolations(IEnumerable<RuleViolation> violations);
public string FormatReport(ViolationReport report);
```

### Example usage:

```csharp
using RoslynGuardAnalyzer.Formatters;
using RoslynGuardAnalyzer.Core;

// Create formatter instance
var formatter = new HtmlFormatter();

// Check if it can handle a format
bool canHandleHtml = formatter.CanFormat("html"); // returns true
bool canHandleJson = formatter.CanFormat("json"); // returns false

// Format analysis results
string htmlOutput = formatter.FormatResult(analysisResult);

// Format violations directly
string violationsHtml = formatter.FormatViolations(violations);

// Format a violation report
string reportHtml = formatter.FormatReport(report);
```

The HTML output includes:
- A responsive, styled layout with summary statistics (total violations and affected files)
- A violations table showing rule, severity, message, file, and line number for each violation
- Severity-based row coloring (critical, error, warning, info)
- Proper HTML escaping to prevent injection issues
- Embedded CSS for consistent styling across browsers

## WebhookHandler

The `WebhookHandler` class (in `RoslynGuardAnalyzer.Integration`) manages webhook registrations and dispatches analysis results to external endpoints. It's useful for CI/CD pipeline integrations and notifications.

## HttpClientFactory

The `HttpClientFactory` class (in `RoslynGuardAnalyzer.Integration`) creates and configures HTTP clients for external integrations. It manages client lifecycle, connection pooling, timeouts, retry policies, and implements a circuit-breaker pattern for resilient HTTP communication.

### Public API

```csharp
public sealed class HttpClientFactory : IDisposable
public HttpClientFactory(HttpClientFactoryOptions? options = null)
public HttpClient CreateClient(string baseUrl, string? clientName = null)
public Task<HttpResponseMessage> ExecuteWithRetryAsync(
    HttpClient client,
    Func<HttpClient, Task<HttpResponseMessage>> request)
public Task<string> GetJsonAsync(HttpClient client, string path)
public Task<string> PostJsonAsync(HttpClient client, string path, string jsonContent)
public void ClearCache()
public void Dispose()
```

### HttpClientFactoryOptions

Configuration options for the HttpClientFactory:

```csharp
public sealed class HttpClientFactoryOptions
public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromSeconds(30);
public int MaxRetries { get; init; } = 3;
public int CircuitBreakerFailureThreshold { get; init; } = 5;
public TimeSpan CircuitBreakerOpenDuration { get; init; } = TimeSpan.FromSeconds(30);
public TimeSpan PooledConnectionLifetime { get; init; } = TimeSpan.FromMinutes(2);
public int MaxConnectionsPerServer { get; init; } = 100;
public bool EnableDnsRefresh { get; init; } = true;
```

### Example usage

```csharp
using RoslynGuardAnalyzer.Integration;

// Create factory with default options
var factory = new HttpClientFactory();

// Create a client for a specific endpoint
var client = factory.CreateClient("https://api.example.com");

// Execute a request with automatic retry and circuit breaker
var response = await factory.ExecuteWithRetryAsync(client, async c =>
{
    return await c.GetAsync("/data");
});

// Get JSON response
string json = await factory.GetJsonAsync(client, "/api/users");

// Post JSON data
string result = await factory.PostJsonAsync(client, "/api/users", "{\"name\":\"John\"}");

// Or create a named client (useful for multiple clients to same base URL)
var githubClient = factory.CreateClient("https://api.github.com", "github");

// Remember to dispose when done
factory.Dispose();
```

The factory implements intelligent retry logic with exponential back-off and jitter, automatic circuit-breaking to prevent cascading failures, and connection pooling for efficient HTTP resource usage.

### Public API:

```csharp
public sealed class WebhookHandler
public sealed class WebhookRegistration
public required string Id { get; init; }
public required string Url { get; init; }
public required string EventType { get; init; } // AnalysisCompleted, ViolationDetected, etc.
public Dictionary<string, string> Headers { get; init; } = []
public bool IsActive { get; set; } = true
public DateTime RegisteredAt { get; init; } = DateTime.UtcNow

public WebhookHandler(HttpClientFactory? httpClientFactory = null)
public string RegisterWebhook(string url, string eventType, Dictionary<string, string>? headers = null)
public bool UnregisterWebhook(string webhookId)
public bool DeactivateWebhook(string webhookId)
public Task TriggerWebhooksAsync(string eventType, string jsonPayload)
public IReadOnlyList<WebhookRegistration> GetAllWebhooks()
public IReadOnlyList<WebhookRegistration> GetWebhooksForEvent(string eventType)
public int WebhookCount { get; }
```

### Example usage:

```csharp
using RoslynGuardAnalyzer.Integration;

// Create webhook handler
var webhookHandler = new WebhookHandler();

// Register a webhook for analysis completion events
string webhookId = webhookHandler.RegisterWebhook(
    url: "https://ci.example.com/webhook/roslyn-guard",
    eventType: "AnalysisCompleted",
    headers: new Dictionary<string, string>
    {
        ["Authorization"] = "Bearer your-token-here",
        ["Content-Type"] = "application/json"
    }
);

// Register another webhook for violation detection
webhookHandler.RegisterWebhook(
    url: "https://slack.example.com/webhook",
    eventType: "ViolationDetected",
    headers: new Dictionary<string, string>
    {
        ["Content-Type"] = "application/json"
    }
);

// Trigger webhooks when analysis completes
var analysisResults = JsonSerializer.Serialize(analysisResult);
await webhookHandler.TriggerWebhooksAsync("AnalysisCompleted", analysisResults);

// Get all registered webhooks
IReadOnlyList<WebhookRegistration> webhooks = webhookHandler.GetAllWebhooks();

// Get webhooks for specific event type
IReadOnlyList<WebhookRegistration> completionWebhooks = 
    webhookHandler.GetWebhooksForEvent("AnalysisCompleted");

// Deactivate a webhook (without removing it)
webhookHandler.DeactivateWebhook(webhookId);

// Unregister a webhook completely
webhookHandler.UnregisterWebhook(webhookId);
```

### Webhook Payload Format

When triggered, webhooks receive a JSON payload containing the analysis results. The payload structure matches the `AnalysisResult` class:

```json
{
  "analysisId": "string",
  "projectPath": "string",
  "timestampUtc": "2023-01-01T12:00:00Z",
  "violations": [
    {
      "ruleId": "string",
      "ruleName": "string",
      "message": "string",
      "severity": "string",
      "filePath": "string",
      "lineNumber": 0,
      "columnNumber": 0,
      "codeSnippet": "string"
    }
  ],
  "totalFilesAnalyzed": 0,
  "totalElementsAnalyzed": 0,
  "violationCount": 0,
  "analysisDurationMs": 0,
  "success": true
}
```

### Event Types

Common event types include:
- `AnalysisCompleted`: Fired when analysis finishes successfully
- `AnalysisFailed`: Fired when analysis encounters an error
- `ViolationDetected`: Fired when violations are found during analysis
- `AnalysisStarted`: Fired when analysis begins

### Thread Safety

All methods of `WebhookHandler` are thread-safe and can be called concurrently from multiple threads.

## SarifFormatter

The `SarifFormatter` class (in `RoslynGuardAnalyzer.Formatters`) formats analysis results as SARIF 2.1.0 (Static Analysis Results Interchange Format) output. SARIF is a JSON-based standard format for the output of static analysis tools, enabling integration with various code analysis platforms and tools.

### Public API:

```csharp
public sealed class SarifFormatter : IOutputFormatter, IEquatable<SarifFormatter>
public string Format => "sarif";
public bool CanFormat(string format);
public string FormatResult(AnalysisResult result);
public string FormatViolations(IEnumerable<RuleViolation> violations);
public string FormatReport(ViolationReport report);
```

### Example usage:

```csharp
using RoslynGuardAnalyzer.Formatters;
using RoslynGuardAnalyzer.Core;

// Create formatter instance
var formatter = new SarifFormatter();

// Check if it can handle a format
bool canHandleSarif = formatter.CanFormat("sarif"); // returns true
bool canHandleJson = formatter.CanFormat("json"); // returns false

// Format analysis results
string sarifOutput = formatter.FormatResult(analysisResult);

// Format violations directly
string violationsSarif = formatter.FormatViolations(violations);

// Format a violation report
string reportSarif = formatter.FormatReport(report);
```

The SARIF output includes:
- Standard SARIF 2.1.0 JSON structure with version and schema references
- Tool information identifying "Roslyn Guard Analyzer" as the analysis tool
- Rule definitions for each unique rule ID encountered
- Result entries for each violation with proper SARIF levels (error, warning, note)
- Location information including file paths, line/column numbers, and code snippets
- Properties containing additional metadata like severity, category, project name, and detection timestamp
- GUID identifiers for each result to enable result tracking across runs

## AnalysisResultRepository

The `AnalysisResultRepository` class (in `RoslynGuardAnalyzer.Data`) manages persistence of analysis results to disk storage. It inherits from `RepositoryBase<AnalysisResult>` and provides specialized methods for querying and managing analysis results stored as JSON files in the application data directory.

### Public API:

```csharp
public sealed class AnalysisResultRepository : RepositoryBase<AnalysisResult>
public AnalysisResultRepository(string? dataDirectory = null)
public IReadOnlyList<AnalysisResult> GetByProject(string projectPath)
public IReadOnlyList<AnalysisResult> GetAnalyzedAfter(DateTime date)
public IReadOnlyList<AnalysisResult> GetFailedAnalyses()
public IReadOnlyList<AnalysisResult> GetSuccessfulAnalyses()
public IReadOnlyList<AnalysisResult> GetWithViolationsInCategory(string category)
public AnalysisResult? GetLatestForProject(string projectPath)
public IReadOnlyList<AnalysisResult> GetWithViolationCountGreaterThan(int violationCount)
public Task SaveAsync(AnalysisResult result)
public Task LoadAllAsync()
public Task ExportToCsvAsync(string filePath)
public AnalysisResultStatistics GetStatistics()
public Task ClearOldResultsAsync(int daysOld)
```

### Inherited from RepositoryBase<AnalysisResult>:

- `void Add(string id, AnalysisResult entity)` - Adds an entity to the repository
- `AnalysisResult? GetById(string id)` - Retrieves an entity by ID
- `IReadOnlyList<AnalysisResult> GetAll()` - Gets all entities in the repository
- `void Update(string id, AnalysisResult entity)` - Updates an existing entity
- `bool Remove(string id)` - Removes an entity by ID
- `bool Exists(string id)` - Checks if an entity exists
- `int Count()` - Gets the count of entities in the repository
- `void Clear()` - Clears all entities from the repository
- `void AddRange(Dictionary<string, AnalysisResult> entities)` - Adds multiple entities at once
- `IReadOnlyList<AnalysisResult> Find(Func<AnalysisResult, bool> predicate)` - Finds entities matching a predicate

### Example usage:

```csharp
using RoslynGuardAnalyzer.Data;
using RoslynGuardAnalyzer.Domain.Models;

// Create repository instance (uses default AppData location)
var repository = new AnalysisResultRepository();

// Or specify custom data directory
var customRepository = new AnalysisResultRepository("/path/to/custom/data");

// Save an analysis result
await repository.SaveAsync(analysisResult);

// Load all results from disk
await repository.LoadAllAsync();

// Get results for a specific project
var projectResults = repository.GetByProject(@"C:\src\MyProject");

// Get successful analyses only
var successfulResults = repository.GetSuccessfulAnalyses();

// Get results with more than 5 violations
var highViolationResults = repository.GetWithViolationCountGreaterThan(5);

// Get the latest analysis for a project
var latestResult = repository.GetLatestForProject(@"C:\src\MyProject");

// Export results to CSV
await repository.ExportToCsvAsync(@"C:\temp\analysis-results.csv");

// Get statistics about stored results
var stats = repository.GetStatistics();
Console.WriteLine($"Success rate: {stats.GetSuccessRate()}%");

// Clear results older than 30 days
await repository.ClearOldResultsAsync(30);
```

The repository stores analysis results as JSON files in a `results` subdirectory within the configured data directory (default: `%APPDATA%\RoslynGuardAnalyzer\Data\results`). Each file is named using the pattern `{resultId}_{timestamp}.json` to ensure uniqueness.

## RuleConfigurationBuilder

The `RuleConfigurationBuilder` class (in `RoslynGuardAnalyzer.Configuration`) is a fluent builder that creates `RuleConfiguration` instances with type safety and sensible defaults. It lets you compose a rule configuration step by step and then materialize it with `Build()`. The builder is `sealed`, so it cannot be subclassed.

### Public API:

```csharp
public sealed class RuleConfigurationBuilder
public RuleConfigurationBuilder(string? ruleName)   // throws on null/empty
public RuleConfigurationBuilder WithEnabled(bool enabled)
public RuleConfigurationBuilder WithSeverity(string severity)
public RuleConfigurationBuilder WithParameter(string? key, object? value)
public RuleConfigurationBuilder WithParameters(Dictionary<string, object> parameters)
public RuleConfigurationBuilder WithDescription(string description)
public RuleConfiguration Build()
public static RuleConfigurationBuilder CreateNamingConvention()
public static RuleConfigurationBuilder CreateLayerDependency()
public static RuleConfigurationBuilder CreateAsyncPatterns()
public static RuleConfigurationBuilder CreateNullSafety()
```

The constructor requires a non-empty rule name. `WithSeverity` accepts only `Low`, `Medium`, `High`, or `Critical` (case-insensitive) and throws `ArgumentException` otherwise. `WithParameter` and `WithDescription` throw on null or empty input. `Build()` returns a `RuleConfiguration` whose `Name` and `Description` are set and whose `Enabled` and `Severity` values are stored as custom settings, followed by any additional parameters (values are converted to strings).

The static factory methods seed a builder with common settings for a specific rule category:

- `CreateNamingConvention()` — `NamingConvention`, `Medium` severity, checks public members and constants.
- `CreateLayerDependency()` — `LayerDependency`, `High` severity, with `StrictMode` off by default.
- `CreateAsyncPatterns()` — `AsyncPatterns`, `Medium` severity, requires the async suffix and disallows blocking calls.
- `CreateNullSafety()` — `NullSafety`, `High` severity, requires null checks and disallows null-forgiving operators.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Configuration;
using RoslynGuardAnalyzer.Domain.Models;

// Compose a configuration step by step.
var config = new RuleConfigurationBuilder("LayerDependency")
    .WithEnabled(true)
    .WithSeverity("High")
    .WithDescription("Enforces architectural layer dependencies")
    .WithParameter("StrictMode", true)
    .WithParameter("AllowedLayers", new[] { "Domain", "Application", "Infrastructure" })
    .Build();

Console.WriteLine(config.Name);            // LayerDependency
Console.WriteLine(config.GetCustomSetting("Severity")); // High
Console.WriteLine(config.GetCustomSetting("StrictMode")); // True

// Or start from a pre-seeded category builder and tweak it.
var naming = RuleConfigurationBuilder.CreateNamingConvention()
    .WithEnabled(false)
    .Build();
```

`Build()` returns a fully populated `RuleConfiguration` that can be passed to the analyzer or further customized through its own API (for example, `AddRule`, `ExcludeNamespace`, or `SetCustomSetting`).

## ProjectRepository

The `ProjectRepository` class (in `RoslynGuardAnalyzer.Data`) manages persistence of analyzed projects to disk storage. It inherits from `RepositoryBase<AnalysisProject>` and provides specialized query methods for filtering projects by target framework, language, file count, analysis date, name, path, and referenced dependencies. Projects are stored as a single JSON file (`projects.json`) in the configured data directory, which defaults to `%APPDATA%\RoslynGuardAnalyzer\Data`.

### Public API:

```csharp
public sealed class ProjectRepository : RepositoryBase<AnalysisProject>
public ProjectRepository(string? dataDirectory = null)
public IReadOnlyList<AnalysisProject> GetByTargetFramework(string targetFramework)
public IReadOnlyList<AnalysisProject> GetModernDotNetProjects()
public IReadOnlyList<AnalysisProject> GetByLanguage(string language)
public IReadOnlyList<AnalysisProject> GetWithMoreFilesThan(int fileCount)
public IReadOnlyList<AnalysisProject> GetAnalyzedAfter(DateTime date)
public IReadOnlyList<AnalysisProject> SearchByName(string pattern)
public AnalysisProject? FindByPath(string path)
public IReadOnlyList<AnalysisProject> GetWithReferences()
public Task SaveAsync()
public Task LoadAsync()
public Task ExportAsync(string filePath)
public Task ImportAsync(string filePath)
public ProjectRepositoryStatistics GetStatistics()
public Task RemoveProjectAsync(string projectId, bool deleteSourceFiles = false)
public void ValidateAndCleanup()
public override string ToString()
```

`GetByTargetFramework`, `GetByLanguage`, `SearchByName`, and `FindByPath` throw `ArgumentException` when given a null or empty argument. `GetAnalyzedAfter` throws when the date is `DateTime.MinValue`. `SaveAsync` and `LoadAsync` persist and restore the full project set to and from `projects.json`; `ExportAsync` and `ImportAsync` do the same against an arbitrary JSON file, with `ImportAsync` updating existing projects by ID and adding new ones. `RemoveProjectAsync` removes a project and, when `deleteSourceFiles` is `true`, also deletes its source directory. `ValidateAndCleanup` removes projects that fail `AnalysisProject.IsValid()`.

### Inherited from RepositoryBase<AnalysisProject>:

- `void Add(string id, AnalysisProject entity)` - Adds an entity to the repository
- `AnalysisProject? GetById(string id)` - Retrieves an entity by ID
- `IReadOnlyList<AnalysisProject> GetAll()` - Gets all entities in the repository
- `void Update(string id, AnalysisProject entity)` - Updates an existing entity
- `bool Remove(string id)` - Removes an entity by ID
- `bool Exists(string id)` - Checks if an entity exists
- `int Count()` - Gets the count of entities in the repository
- `void Clear()` - Clears all entities from the repository
- `void AddRange(Dictionary<string, AnalysisProject> entities)` - Adds multiple entities at once
- `IReadOnlyList<AnalysisProject> Find(Func<AnalysisProject, bool> predicate)` - Finds entities matching a predicate

### ProjectRepositoryStatistics

`GetStatistics()` returns a `ProjectRepositoryStatistics` instance describing the stored projects:

- `TotalProjects` - Total number of projects
- `ModernDotNetProjects` - Count of .NET Core/.NET 5+ projects
- `AverageFileCount` - Average file count across projects
- `TotalFiles` - Sum of all project file counts
- `ProjectsByFramework` - Dictionary mapping target framework to project count
- `UniqueLanguages` - Number of distinct project languages
- `GetModernDotNetPercentage()` - Percentage of projects that are modern .NET

### Example usage:

```csharp
using RoslynGuardAnalyzer.Data;
using RoslynGuardAnalyzer.Domain.Models;

// Create repository instance (uses default AppData location)
var repository = new ProjectRepository();

// Or specify a custom data directory
var customRepository = new ProjectRepository("/path/to/custom/data");

// Add a project and persist it
var project = new AnalysisProject("OrderService", @"C:\src\OrderService");
project.AddSourceFile(@"C:\src\OrderService\OrderService.cs");
repository.Add(project.Id, project);
await repository.SaveAsync();

// Load all projects from disk
await repository.LoadAsync();

// Query projects by framework and language
var modern = repository.GetModernDotNetProjects();
var csharp = repository.GetByLanguage("C#");
var net8 = repository.GetByTargetFramework("net8.0");

// Find a project by path
var byPath = repository.FindByPath(@"C:\src\OrderService");

// Get statistics about stored projects
var stats = repository.GetStatistics();
Console.WriteLine($"Total projects: {stats.TotalProjects}");
Console.WriteLine($"Modern .NET: {stats.GetModernDotNetPercentage():F1}%");

// Remove a project (optionally deleting its source files)
await repository.RemoveProjectAsync(project.Id, deleteSourceFiles: false);
```

## RuleRepository

The `RuleRepository` class (in `RoslynGuardAnalyzer.Data`) manages persistence of architectural analysis rules. It inherits from `RepositoryBase<AnalysisRule>` and provides specialized query methods for filtering rules by category, severity, enabled state, and creation date, plus enable/disable operations and JSON import/export. Rules are stored as a single JSON file (`rules.json`) in the configured data directory, which defaults to `%APPDATA%\RoslynGuardAnalyzer\Data`.

### Public API:

```csharp
public sealed class RuleRepository : RepositoryBase<AnalysisRule>
public RuleRepository(string? dataDirectory = null)
public IReadOnlyList<AnalysisRule> GetByCategory(RuleCategory category)
public IReadOnlyList<AnalysisRule> GetEnabledRules()
public IReadOnlyList<AnalysisRule> GetBySeverity(SeverityLevel severity)
public IReadOnlyList<AnalysisRule> GetCreatedAfter(DateTime date)
public bool DisableRule(string ruleId)
public bool EnableRule(string ruleId)
public Task SaveAsync()
public Task LoadAsync()
public Task ExportAsync(string filePath)
public Task ImportAsync(string filePath)
public string GetDataDirectory()
public RuleRepositoryStatistics GetStatistics()
```

`GetByCategory` throws `ArgumentNullException` when given a null category. `DisableRule` and `EnableRule` throw `ArgumentException` when given a null or empty rule ID and return `false` when no rule with that ID exists. `SaveAsync` and `LoadAsync` persist and restore the full rule set to and from `rules.json`; `ExportAsync` and `ImportAsync` do the same against an arbitrary JSON file, with `ImportAsync` updating existing rules by ID and adding new ones. All persistence methods wrap failures in `InvalidOperationException`.

### Inherited from RepositoryBase<AnalysisRule>:

- `void Add(string id, AnalysisRule entity)` - Adds an entity to the repository
- `AnalysisRule? GetById(string id)` - Retrieves an entity by ID
- `IReadOnlyList<AnalysisRule> GetAll()` - Gets all entities in the repository
- `void Update(string id, AnalysisRule entity)` - Updates an existing entity
- `bool Remove(string id)` - Removes an entity by ID
- `bool Exists(string id)` - Checks if an entity exists
- `int Count()` - Gets the count of entities in the repository
- `void Clear()` - Clears all entities from the repository
- `void AddRange(Dictionary<string, AnalysisRule> entities)` - Adds multiple entities at once
- `IReadOnlyList<AnalysisRule> Find(Func<AnalysisRule, bool> predicate)` - Finds entities matching a predicate

### RuleRepositoryStatistics

`GetStatistics()` returns a `RuleRepositoryStatistics` instance describing the stored rules:

- `TotalRules` - Total number of rules
- `EnabledRules` - Count of enabled rules
- `DisabledRules` - Count of disabled rules
- `RulesByCategory` - Dictionary mapping rule category to rule count
- `GetEnabledPercentage()` - Percentage of rules that are enabled

### Example usage:

```csharp
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Data;
using RoslynGuardAnalyzer.Domain.Models;

// Create repository instance (uses default AppData location)
var repository = new RuleRepository();

// Or specify a custom data directory
var customRepository = new RuleRepository("/path/to/custom/data");

// Add a rule and persist it
var rule = new AnalysisRule(
    "RG-N001",
    "Interface naming",
    "Interface names should start with 'I'.",
    RuleCategory.NamingConvention)
{
    DefaultSeverity = SeverityLevel.Error
};
repository.Add(rule.Id, rule);
await repository.SaveAsync();

// Load all rules from disk
await repository.LoadAsync();

// Query rules by category and severity
var namingRules = repository.GetByCategory(RuleCategory.NamingConvention);
var errorRules = repository.GetBySeverity(SeverityLevel.Error);

// Get only enabled rules
var enabledRules = repository.GetEnabledRules();

// Enable or disable a rule by ID
repository.DisableRule("RG-N001");
repository.EnableRule("RG-N001");

// Export rules to a JSON file
await repository.ExportAsync(@"C:\temp\rules-backup.json");

// Get statistics about stored rules
var stats = repository.GetStatistics();
Console.WriteLine($"Total rules: {stats.TotalRules}");
Console.WriteLine($"Enabled: {stats.GetEnabledPercentage():F1}%");
```

## EventBus

The `EventBus` class (in `RoslynGuardAnalyzer.Events`) is an in-memory implementation of the `IEventBus` publish-subscribe interface. It maintains a thread-safe registry of subscribers and dispatches events to them asynchronously. Events implement `IEvent` (or derive from the `Event` base class) and carry an `EventId`, an `EventType` name, a UTC `TimestampUtc`, and optional `Metadata`.

`EventBus` provides the following guarantees:

- **Ordering**: Events are dispatched to subscribers in the order they were subscribed.
- **Isolation**: An exception thrown by one subscriber does not prevent other subscribers from being invoked.
- **Delivery**: All subscribers for a given event type are invoked unless cancelled via a `CancellationToken`.
- **Inheritance**: Subscribers registered for a base type also receive events of derived types.
- **Aggregation**: If multiple subscribers throw, the exceptions are collected and thrown together as an `AggregateException` after all subscribers have run.

### Public API:

```csharp
public sealed class EventBus : IEventBus
public EventBus()
public Task PublishAsync(IEvent @event)
public Task PublishAsync(IEvent @event, CancellationToken cancellationToken)
public IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where TEvent : IEvent
public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
public void Unsubscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where TEvent : IEvent
public int SubscriptionCount { get; }
public void ClearSubscriptions()
```

`PublishAsync` throws `ArgumentNullException` when the event is `null` and `AggregateException` when any subscriber fails. The overload taking a `CancellationToken` additionally throws `OperationCanceledException` when cancelled. `Subscribe` returns an `IDisposable` that unsubscribes the handler when disposed. `Unsubscribe` removes all matching subscriptions for the given handler. `SubscriptionCount` and `ClearSubscriptions` are primarily useful for testing.

The `EventBusExtensions` class adds convenience overloads: strongly-typed `PublishAsync<TEvent>`, `Subscribe<TEvent>`/`Unsubscribe<TEvent>` that take the bus as the receiver, and `PublishAllAsync` which publishes a collection of events sequentially in order.

### Example usage:

```csharp
using RoslynGuardAnalyzer.Events;

var bus = new EventBus();

// Subscribe to a specific event type. The returned IDisposable unsubscribes on dispose.
using var subscription = bus.Subscribe<AnalysisStartedEvent>(async ev =>
{
    Console.WriteLine($"Analysis {ev.AnalysisId} started for {ev.ProjectPath}");
});

// Publish an event; all matching subscribers are invoked asynchronously.
await bus.PublishAsync(new AnalysisStartedEvent
{
    ProjectPath = "src/MyProject/MyProject.csproj",
    AnalysisId = Guid.NewGuid().ToString()
});

// Subscribe with cancellation support.
var cts = new CancellationTokenSource();
bus.Subscribe<ViolationDetectedEvent>(
    async (ev, token) =>
    {
        token.ThrowIfCancellationRequested();
        Console.WriteLine($"{ev.RuleName}: {ev.Violation.Message}");
    },
    cts.Token);

// Publish with cancellation.
await bus.PublishAsync(new ViolationDetectedEvent
{
    Violation = violation,
    RuleName = "RG-N001",
    Severity = "High"
}, cts.Token);

// Unsubscribe explicitly when the handler is no longer needed.
bus.Unsubscribe<AnalysisStartedEvent>(handler);
```

## AnalysisPipeline

The `AnalysisPipeline` class (in `RoslynGuardAnalyzer.Middleware`) manages and executes a chain of middleware components in order. It implements the pipeline/filter pattern for composable request handling, allowing middleware to inspect, transform, or short-circuit the analysis flow.

### Public API

```csharp
public sealed class AnalysisPipeline
public IReadOnlyList<IMiddleware> Middlewares { get; }
public AnalysisPipeline Use(IMiddleware middleware)
public AnalysisPipeline UseHandler(MiddlewareDelegate handler)
public Task ExecuteAsync(PipelineContext context)
public string GetChainDescription()
```

- `Use(IMiddleware middleware)`: Adds a middleware component to the pipeline. Middleware is executed in the order it was added. Returns the pipeline instance for fluent chaining.
- `UseHandler(MiddlewareDelegate handler)`: Sets the final handler to be called after all middleware. Returns the pipeline instance for fluent chaining.
- `ExecuteAsync(PipelineContext context)`: Executes the pipeline with the given context. Builds the middleware chain and invokes it with proper ordering.
- `GetChainDescription()`: Gets a string representation of the middleware chain for diagnostics.

### Example usage

```csharp
using RoslynGuardAnalyzer.Middleware;

// Create pipeline and register middleware
var pipeline = new AnalysisPipeline()
    .Use(new LoggingMiddleware())
    .Use(new PerformanceMetricsMiddleware())
    .Use(new ErrorHandlingMiddleware());

// Set the final handler (your core analysis logic)
pipeline.UseHandler(async context =>
{
    // Perform actual analysis here
    await AnalyzeProjectAsync(context);
});

// Execute the pipeline
await pipeline.ExecuteAsync(new PipelineContext
{
    ProjectPath = @"src/MyProject/MyProject.csproj",
    AnalysisId = Guid.NewGuid().ToString(),
    StartTimeMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
});
```

### Middleware Contract

Middleware components implement the `IMiddleware` interface:

```csharp
public interface IMiddleware
{
    string Name { get; }
    Task InvokeAsync(PipelineContext context, MiddlewareDelegate next);
}

public delegate Task MiddlewareDelegate(PipelineContext context);
```

Each middleware receives a `PipelineContext` and a `next` delegate. Calling `await next(context)` invokes the next middleware in the chain. Middleware can:
- Execute logic before calling `next()` (pre-processing)
- Execute logic after calling `next()` (post-processing)
- Short-circuit by not calling `next()`
- Modify the context for downstream middleware

Because subscribers are invoked outside the lock and exceptions are isolated per subscriber, a failing handler does not block the remaining subscribers; all failures are surfaced together in the resulting `AggregateException`.

## PerformanceMetricsMiddleware

The `PerformanceMetricsMiddleware` class (in `RoslynGuardAnalyzer.Middleware`) measures an analysis pipeline invocation and stores the resulting metrics in its `PipelineContext`. It captures elapsed wall-clock time, UTC start and end times, processor count, and any non-negative increase in managed memory. Metrics are saved after downstream middleware finishes, including when downstream processing throws, making them available for diagnostics and performance-regression reporting after execution.

### Public API

```csharp
public sealed class PerformanceMetricsMiddleware : IMiddleware
public string Name { get; }
public Task InvokeAsync(PipelineContext context, MiddlewareDelegate next)
public static void RecordComponentTiming(
    PipelineContext context,
    string componentName,
    long milliseconds)
public static PerformanceMetrics? GetMetrics(PipelineContext context)
public static string GenerateReport(PerformanceMetrics metrics)

public sealed class PerformanceMetrics
public long TotalMilliseconds { get; set; }
public long PeakMemoryBytes { get; set; }
public int ProcessorCount { get; set; }
public Dictionary<string, long> ComponentTimingsMs { get; }
public DateTime StartTime { get; set; }
public DateTime EndTime { get; set; }
public TimeSpan GetElapsed()
```

`GetMetrics` returns `null` until metrics have been placed in the context. `RecordComponentTiming` adds repeated measurements for the same component when metrics are already present, and `GenerateReport` produces a human-readable summary with component timings ordered from slowest to fastest.

### Example usage

```csharp
using RoslynGuardAnalyzer.Middleware;

var context = new PipelineContext
{
    ProjectPath = "src/MyProject/MyProject.csproj",
    AnalysisId = Guid.NewGuid().ToString(),
    StartTimeMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

var pipeline = new AnalysisPipeline()
    .Use(new PerformanceMetricsMiddleware())
    .UseHandler(async _ =>
    {
        await Task.Delay(25); // Replace with the analysis operation.
    });

await pipeline.ExecuteAsync(context);

var metrics = PerformanceMetricsMiddleware.GetMetrics(context);
if (metrics is not null)
{
    PerformanceMetricsMiddleware.RecordComponentTiming(
        context,
        "Project analysis",
        metrics.TotalMilliseconds);

    Console.WriteLine(PerformanceMetricsMiddleware.GenerateReport(metrics));
}
```

## SuppressionManager

The `SuppressionManager` class (in `RoslynGuardAnalyzer.Suppressions`) maintains a thread-safe, in-memory collection of rule suppressions. Suppressions can apply to an entire rule or be narrowed to a source file and target element, and can be inactive or expire at a specified UTC time. The manager can test or filter violations and persist the collection as JSON.

Adding a record with an existing suppression ID replaces that record. `LoadAsync` replaces the current collection with the active, unexpired records from the file; a missing file leaves the current collection unchanged. Persistence failures are logged and do not propagate to the caller.

### Public API

```csharp
public interface ISuppressionManager
public sealed class SuppressionManager : ISuppressionManager
public SuppressionManager(ILogger<SuppressionManager> logger)
public void AddSuppression(SuppressionRecord record)
public bool RemoveSuppression(string suppressionId)
public IReadOnlyList<SuppressionRecord> GetSuppressions(string? ruleId = null)
public bool IsSuppressed(RuleViolation violation)
public IReadOnlyList<RuleViolation> FilterSuppressed(
    IEnumerable<RuleViolation> violations)
public Task SaveAsync(
    string filePath,
    CancellationToken cancellationToken = default)
public Task LoadAsync(
    string filePath,
    CancellationToken cancellationToken = default)
```

`GetSuppressions` returns records ordered by creation time and can filter them by rule ID. Matching is case-insensitive for rule IDs, file paths, and target elements. A suppression without a target file or element covers every violation for its rule; inactive and expired records do not match.

### Example usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Infrastructure;
using RoslynGuardAnalyzer.Suppressions;

var services = new ServiceCollection();
services.RegisterAnalyzerServices();

using var provider = services.BuildServiceProvider();
var suppressions = provider.GetRequiredService<ISuppressionManager>();
var suppressionPath = Path.Combine(".roslyn-guard", "suppressions.json");

await suppressions.LoadAsync(suppressionPath);

suppressions.AddSuppression(new SuppressionRecord
{
    RuleId = "RG001",
    TargetFile = "src/MyProject/LegacyService.cs",
    Justification = "Accepted until the legacy service is replaced.",
    Author = "architecture-team",
    ExpiresAt = DateTime.UtcNow.AddDays(30)
});

var violations = new[]
{
    new RuleViolation(
        "RG001",
        "Public type naming",
        "Public types must use the configured naming convention.",
        "src/MyProject/LegacyService.cs"),
    new RuleViolation(
        "RG001",
        "Public type naming",
        "Public types must use the configured naming convention.",
        "src/MyProject/CustomerService.cs")
};

var unsuppressed = suppressions.FilterSuppressed(violations);
foreach (var violation in unsuppressed)
    Console.WriteLine(violation);

await suppressions.SaveAsync(suppressionPath);
```
