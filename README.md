# Roslyn Guard Analyzer

A comprehensive .NET code analyzer built with Roslyn, enforcing architectural rules and best practices across codebases.

## Features

- **Layer Dependency Analysis**: Enforces architectural layers and prevents illegal cross-layer dependencies
- **Naming Convention Enforcement**: Validates naming conventions for classes, methods, properties, and fields
- **Async Pattern Detection**: Identifies improper async/await patterns and blocking calls
- **Null Safety Validation**: Enforces nullable reference type handling and null safety patterns
- **Comprehensive Reporting**: Generates detailed analysis reports in multiple formats (Text, JSON, CSV, XML)
- **Rule Registry**: Extensible rule system for defining custom architectural rules
- **Project Analysis**: Analyze entire projects with automatic file discovery and parallel processing
- **Configuration Management**: Flexible configuration system with rule customization

## Getting Started

### Prerequisites

- .NET 10.0 or higher
- C# language support

### Installation

```bash
git clone https://github.com/sarmkadan/roslyn-guard-analyzer.git
cd roslyn-guard-analyzer
dotnet build
```

### Usage

```bash
# Analyze a project
dotnet run -- /path/to/project.csproj

# Analyze a single file
dotnet run -- /path/to/file.cs
```

## Architecture

The analyzer is structured into several layers:

- **Domain Models**: Core business entities (AnalysisRule, RuleViolation, CodeElement, etc.)
- **Services**: Business logic (RuleEngine, AnalysisService, ReportingService, etc.)
- **Data Access**: Repository pattern for persistence
- **Infrastructure**: Dependency injection and configuration management

## Key Components

### Services

- **IAnalysisService**: Orchestrates the complete analysis workflow
- **IRuleRegistry**: Manages available rules
- **IRuleEngine**: Executes rules against code elements
- **IReportingService**: Generates analysis reports
- **IValidationService**: Validates configurations and rules

### Models

- **AnalysisRule**: Defines an architectural rule
- **RuleViolation**: Represents a violation found during analysis
- **CodeElement**: Represents a code artifact being analyzed
- **AnalysisResult**: Complete results of an analysis execution
- **ViolationReport**: Formatted report for presentation

### Repositories

- **RuleRepository**: Manages rule persistence
- **AnalysisResultRepository**: Stores analysis results
- **ProjectRepository**: Tracks analyzed projects

## Rules

The analyzer includes built-in rules for:

1. **Layer Dependency (LYR001)**: Prevents repositories from depending on services or controllers
2. **Naming Convention (NAM001)**: Enforces PascalCase for classes/methods, snake_case for fields
3. **Async Pattern (ASY001)**: Validates async/await patterns and Task handling
4. **Null Safety (NUL001)**: Checks nullable reference type handling

## Configuration

Configure the analyzer via dependency injection:

```csharp
var services = new ServiceCollection();
services.ConfigureAnalyzer(config =>
{
    config.MaxViolationsToReport = 500;
    config.AnalysisTimeoutSeconds = 600;
    config.LogLevel = 3;
});
services.RegisterAnalyzerServices();
```

## Report Formats

- **Text**: Human-readable format with visual indicators
- **JSON**: Machine-readable format for tool integration
- **CSV**: Spreadsheet-friendly format
- **XML**: Structured data format

## Development

### Adding Custom Rules

1. Create a rule class extending `AnalysisRule`
2. Register it with `IRuleRegistry`
3. Implement validation logic in `RuleEngine`

### Extending Analysis

Modify `IAnalysisService` to support additional file types or project formats.

## License

MIT License - Copyright © 2026 Vladyslav Zaiets

## Author

**Vladyslav Zaiets** - CTO & Software Architect
- Website: https://sarmkadan.com
- Repository: https://github.com/sarmkadan/roslyn-guard-analyzer

---

For issues, feature requests, and contributions, please visit the GitHub repository.
