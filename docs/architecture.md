# Architecture Guide

This document describes the internal architecture and design patterns used in Roslyn Guard Analyzer.

## Design Philosophy

Roslyn Guard Analyzer follows these architectural principles:

1. **Separation of Concerns**: Each component has a single, well-defined responsibility
2. **Dependency Inversion**: Abstractions via interfaces, not concrete implementations
3. **Composition over Inheritance**: Middleware chain pattern for cross-cutting concerns
4. **Extensibility**: Plugin-like rule registration and formatter system
5. **Performance**: Parallel processing, caching, and efficient Roslyn integration
6. **Testability**: All business logic abstracted behind interfaces

## Layered Architecture

```
┌─────────────────────────────────────┐
│     Presentation Layer              │
│  (CLI, Formatters, Output Writers)  │
├─────────────────────────────────────┤
│     Application Layer               │
│  (Services, Middleware Pipeline)    │
├─────────────────────────────────────┤
│     Domain Layer                    │
│  (Rules, Models, Exceptions)        │
├─────────────────────────────────────┤
│     Infrastructure Layer            │
│  (Repositories, Config, Events)     │
├─────────────────────────────────────┤
│     External (Roslyn, File System)  │
└─────────────────────────────────────┘
```

## Component Breakdown

### 1. CLI Layer

**Files**: `Cli/CliArgumentParser.cs`, `Cli/CommandLineProcessor.cs`, `Program.cs`

**Responsibility**: Parse command-line arguments and orchestrate execution

**Key Classes**:
- `CliArgumentParser`: Converts command-line args to `CliOptions`
- `CliOptions`: Immutable data object containing parsed arguments
- `CommandLineProcessor`: Orchestrates the full analysis workflow
- `HelpGenerator`: Generates help text and usage documentation

**Example Flow**:
```
args: ["./src", "--format", "json", "--output", "report.json"]
  ↓
CliArgumentParser
  ↓
CliOptions { ProjectPath: "./src", Format: "json", Output: "report.json" }
  ↓
CommandLineProcessor
  ↓
Analysis execution
```

### 2. Configuration Layer

**Files**: `Configuration/ConfigurationLoader.cs`, `Configuration/ConfigurationValidator.cs`

**Responsibility**: Load and validate configuration from files and environment

**Key Classes**:
- `ConfigurationLoader`: Loads JSON/YAML configuration files
- `ConfigurationValidator`: Validates configuration syntax and values
- `RuleConfigurationBuilder`: Fluent API for rule configuration

**Configuration Sources** (in order of precedence):
1. Command-line arguments
2. `.roslyn-guard.json` in current directory
3. Environment variables (e.g., `ROSLYN_GUARD_TIMEOUT`)
4. Default values in code

**Validation Rules**:
- Required fields must be non-empty
- Numeric fields within acceptable ranges
- File paths must exist (when applicable)
- Rule IDs must be registered

### 3. Middleware Pipeline

**Files**: `Middleware/AnalysisPipeline.cs`, `Middleware/*.cs`

**Responsibility**: Apply cross-cutting concerns in a composable chain

**Pattern**: Chain of Responsibility

```csharp
ErrorHandlingMiddleware
  ↓ (catches exceptions)
LoggingMiddleware
  ↓ (logs analysis progress)
PerformanceMetricsMiddleware
  ↓ (measures execution time)
AnalysisService
  ↓ (core business logic)
```

**Built-in Middleware**:
- `ErrorHandlingMiddleware`: Catches and logs exceptions
- `LoggingMiddleware`: Logs analysis events
- `PerformanceMetricsMiddleware`: Measures execution time

**Adding Custom Middleware**:
```csharp
public class CustomMiddleware : IMiddleware
{
    private readonly IMiddleware _next;
    
    public CustomMiddleware(IMiddleware next) => _next = next;
    
    public async Task<AnalysisResult> ExecuteAsync(
        AnalysisContext context)
    {
        // Pre-processing
        var result = await _next.ExecuteAsync(context);
        // Post-processing
        return result;
    }
}
```

### 4. Analysis Service Layer

**Files**: `Services/AnalysisService.cs`, `Services/RuleEngine.cs`, `Services/DiagnosticsService.cs`

**Responsibility**: Core analysis workflow orchestration

**Key Interfaces**:
- `IAnalysisService`: Orchestrates the complete analysis
- `IRuleEngine`: Executes rules against code elements
- `IRuleRegistry`: Manages rule registration and configuration
- `IDiagnosticsService`: Integrates with Roslyn for semantic analysis

**Analysis Workflow**:
```
1. Load project (Roslyn workspace)
   ↓
2. Discover code elements
   ↓
3. For each code element:
   a. Run all enabled rules
   b. Collect violations
   ↓
4. Aggregate results
   ↓
5. Generate statistics
   ↓
6. Return AnalysisResult
```

**Roslyn Integration**:
- Uses `Microsoft.CodeAnalysis` NuGet package
- Creates a workspace from project file
- Analyzes semantic information (types, symbols)
- Extracts syntax trees for pattern matching

### 5. Domain Layer

**Files**: `Domain/Models/*.cs`, `Exceptions/`, `Core/`

**Responsibility**: Core business entities and logic

**Key Classes**:
- `AnalysisRule`: Abstract base for all rule implementations
- `RuleViolation`: Represents a single violation instance
- `CodeElement`: Represents analyzed code (class, method, field)
- `AnalysisResult`: Complete analysis results
- `RuleConfiguration`: Configuration for a specific rule

**Rule Hierarchy**:
```
AnalysisRule (abstract)
  ├── LayerDependencyRule (LYR001)
  ├── NamingConventionRule (NAM001)
  ├── AsyncPatternRule (ASY001)
  └── NullSafetyRule (NUL001)
```

**Adding Custom Rules**:
```csharp
public class CustomRule : AnalysisRule
{
    public override string Id => "CUSTOM001";
    public override string Category => "Custom";
    public override string Description => "...";
    
    public override async Task<IEnumerable<RuleViolation>> ValidateAsync(
        CodeElement element,
        RuleConfiguration config)
    {
        // Implementation
    }
}
```

### 6. Output Layer

**Files**: `Formatters/`, `Services/OutputWriter.cs`, `Services/ReportingService.cs`

**Responsibility**: Format results for different consumers

**Formatter Pattern**:
```csharp
public interface IOutputFormatter
{
    string Format(AnalysisResult result);
    string Name { get; }
}
```

**Built-in Formatters**:
- `JsonFormatter`: Machine-readable JSON
- `CsvFormatter`: Spreadsheet-compatible CSV
- `HtmlFormatter`: Interactive HTML report
- `TextFormatter`: Human-readable plain text

**Formatter Registry**:
```csharp
var registry = new FormatterRegistry();
registry.Register(new JsonFormatter());
registry.Register(new CsvFormatter());

var formatter = registry.Get("json");
var output = formatter.Format(result);
```

### 7. Infrastructure Layer

**Files**: `Data/`, `Infrastructure/`, `Events/`

**Responsibility**: Technical infrastructure concerns

**Repository Pattern**:
```
IRepository<T>
  ├── AnalysisResultRepository
  ├── ProjectRepository
  └── RuleRepository
```

**Event Bus**:
- Publish/subscribe pattern for loose coupling
- Events: `AnalysisStarted`, `AnalysisCompleted`, `ViolationFound`

**Service Registration**:
```csharp
services.RegisterAnalyzerServices();
// Registers:
// - All rules
// - All formatters
// - Analysis services
// - Repositories
// - Event bus
```

## Data Flow

### Complete Analysis Flow

```
User Input (CLI args)
    ↓
CliArgumentParser
    ↓
CliOptions
    ↓
ConfigurationLoader
    ↓
RuleConfiguration
    ↓
CommandLineProcessor
    ↓
AnalysisPipeline (Middleware chain)
    ↓
AnalysisService.AnalyzeProjectAsync()
    ↓
1. Load project (Roslyn)
2. Discover code elements
3. RuleEngine.ExecuteRulesAsync()
4. Collect RuleViolation instances
    ↓
AnalysisResult
    ↓
ReportingService.GenerateReport()
    ↓
FormatterRegistry.Get(format).Format()
    ↓
Formatted Output (Text/JSON/CSV/HTML)
    ↓
OutputWriter.WriteAsync()
    ↓
Console/File output
```

## Caching Strategy

**Cache Service**: `Caching/CacheService.cs`

Caches expensive operations:
- Roslyn syntax trees
- Symbol resolution results
- Rule execution results for identical code elements

```csharp
var cache = new CacheService();
var key = CacheKeyGenerator.GenerateKey(element);

if (cache.TryGet(key, out var result))
    return result; // Cache hit

// Perform expensive operation
var result = await expensiveOperation();
cache.Set(key, result);
```

## Performance Optimizations

### 1. Parallel Processing
```csharp
var violations = await Task.WhenAll(
    tasks.AsParallel()
        .Select(t => t())
);
```

### 2. Lazy Evaluation
Code elements are analyzed on-demand, not all upfront.

### 3. Background Tasks
Long-running operations use `BackgroundTaskQueue`:
```csharp
await backgroundQueue.EnqueueAsync(async () =>
{
    // Long operation
});
```

### 4. Memory Management
- Roslyn syntaxes disposed after analysis
- Large result sets streamed to output
- Temporary caches cleared between projects

## Extension Points

### 1. Custom Rules
Implement `AnalysisRule` and register with `IRuleRegistry`

### 2. Custom Formatters
Implement `IOutputFormatter` and register with `FormatterRegistry`

### 3. Custom Middleware
Implement `IMiddleware` and add to pipeline

### 4. Event Subscriptions
Subscribe to `EventBus` events:
```csharp
eventBus.Subscribe<AnalysisCompletedEvent>(e =>
{
    Console.WriteLine($"Analysis found {e.ViolationCount} violations");
});
```

### 5. Custom Configuration Sources
Extend `ConfigurationLoader` to support additional formats

## Testing Strategy

### Unit Tests
- Test individual rules in isolation
- Mock `CodeElement` and `RuleConfiguration`
- Verify violation detection logic

### Integration Tests
- Load real projects
- Run full analysis pipeline
- Verify end-to-end results

### Performance Tests
- Measure analysis time on large projects
- Monitor memory usage
- Benchmark Roslyn operations

## Dependency Injection

All dependencies injected via constructor:

```csharp
public class AnalysisService : IAnalysisService
{
    public AnalysisService(
        IRuleRegistry ruleRegistry,
        IReportingService reportingService,
        IDiagnosticsService diagnosticsService)
    {
        _ruleRegistry = ruleRegistry;
        // ...
    }
}
```

Configuration in `ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<IRuleRegistry, RuleRegistry>();
services.AddTransient<IAnalysisService, AnalysisService>();
services.AddScoped<ICacheService, CacheService>();
```

## Configuration Management

### Configuration Hierarchy

```
Default Values (code)
    ↑
Environment Variables
    ↑
Configuration File (.roslyn-guard.json)
    ↑
Command-Line Arguments (highest priority)
```

### Rule Configuration

Each rule independently configurable:

```json
{
  "rules": {
    "LYR001": {
      "enabled": true,
      "severity": "error",
      "configuration": {
        "allowedDependencies": ["Domain", "Infrastructure"]
      }
    }
  }
}
```

## Error Handling

### Exception Hierarchy

```
RoslynGuardException (base)
  ├── ConfigurationException
  ├── AnalysisException
  ├── RuleExecutionException
  └── FormattingException
```

### Error Recovery

- Configuration errors → Clear error message + usage help
- Analysis errors → Log and continue with other files
- Rule errors → Isolated, don't affect other rules
- Formatting errors → Fallback to text format

## Security Considerations

1. **Input Validation**: All configuration and CLI arguments validated
2. **Path Traversal**: File paths validated and normalized
3. **Resource Limits**: Timeout and max violations configured
4. **Dependency**: Only official NuGet packages from Microsoft
5. **No Code Execution**: Analyzer only reads code, doesn't execute it

## Future Architecture Improvements

- **Plugin System**: Load rules from external assemblies
- **Caching Layer**: Persistent cache across runs
- **Incremental Analysis**: Only analyze changed files
- **Language Support**: Extend beyond C#
- **IDE Integration**: LSP support for real-time analysis
