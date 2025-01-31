# Roslyn Guard Analyzer

![Build](https://github.com/sarmkadan/roslyn-guard-analyzer/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/roslyn-guard-analyzer)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

**A production-grade architectural code analyzer powered by Roslyn for .NET projects**

Enforce architectural rules, naming conventions, async patterns, and null safety across your entire codebase with a flexible, extensible analysis engine. Built for teams that take code quality seriously.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Quick Start](#quick-start)
- [Installation](#installation)
- [Usage Examples](#usage-examples)
- [Architecture](#architecture)
- [API Reference](#api-reference)
- [Configuration Reference](#configuration-reference)
- [CLI Reference](#cli-reference)
- [Troubleshooting](#troubleshooting)
- [Testing](#testing)
- [Performance](#performance)
- [Documentation](#documentation)
- [Related Projects](#related-projects)
- [Contributing](#contributing)

## Overview

Roslyn Guard Analyzer is a comprehensive static analysis tool that enforces architectural patterns and best practices in .NET codebases. Built on the Microsoft Roslyn compiler platform, it provides deep syntactic and semantic analysis capabilities to identify violations before they reach production.

### Why Roslyn Guard Analyzer?

- **Architectural Enforcement**: Define and enforce layer dependencies, preventing circular dependencies and architectural violations
- **Naming Convention Validation**: Enforce consistent naming patterns across your codebase automatically
- **Async Pattern Detection**: Identify improper async/await patterns, blocking calls, and Task handling issues
- **Null Safety Validation**: Enforce nullable reference type patterns and null-safety best practices
- **Team Scalability**: Run analysis as part of CI/CD pipelines to maintain standards across distributed teams
- **Zero Configuration**: Works out of the box with sensible defaults
- **Fully Customizable**: Define custom rules tailored to your architectural needs
- **Multiple Output Formats**: Generate reports in Text, JSON, CSV, XML, and HTML formats

### Perfect For

- Large teams maintaining shared architectural standards
- Microservices architectures requiring strict layer separation
- Projects adopting async-first patterns
- Teams implementing nullable reference types
- CI/CD integration for code quality gates

## Features

### Core Analysis Capabilities

| Feature | Description |
|---------|-------------|
| **Layer Dependency Analysis** | Enforces architectural layers and prevents illegal cross-layer dependencies |
| **Naming Convention Enforcement** | Validates naming conventions for classes, methods, properties, and fields |
| **Async Pattern Detection** | Identifies improper async/await patterns and blocking calls in async contexts |
| **Null Safety Validation** | Enforces nullable reference type handling and null-safety patterns |
| **Project Analysis** | Analyze entire projects with automatic file discovery and parallel processing |
| **Multi-Format Reporting** | Generate reports in Text, JSON, CSV, XML, and HTML formats |
| **Rule Registry** | Extensible rule system for defining custom architectural rules |
| **Configuration Management** | Flexible configuration system with rule customization |
| **Performance Metrics** | Built-in performance profiling and analysis statistics |
| **Event-Driven Architecture** | Publish/subscribe system for extensibility and monitoring |

### Built-In Rules

The analyzer ships with four foundational rules covering the most common architectural concerns:

| Rule ID | Category | Description |
|---------|----------|-------------|
| `LYR001` | Layer Dependencies | Prevents repositories from depending on services or controllers |
| `NAM001` | Naming Conventions | Enforces PascalCase for classes/methods, snake_case for fields |
| `ASY001` | Async Patterns | Validates async/await patterns and proper Task handling |
| `NUL001` | Null Safety | Checks nullable reference type handling and null-coalescing patterns |

## Advanced Features

### Custom Rule Builder DSL

Define predicate-based rules with a fluent API and register them just like built-in rules:

```csharp
var rule = CustomRuleBuilder.Create("CUS001", "Async suffix rule")
    .For(RuleCategory.AsyncPattern)
    .WithSeverity(SeverityLevel.Warning)
    .WithDescription("Requires async methods to end with Async")
    .When(element => element.ElementType == CodeElementType.Method && element.IsAsync && !element.Name.EndsWith("Async"))
    .WithMessage(element => $"Method '{element.Name}' must end with Async")
    .Build();

ruleRegistry.RegisterRule(rule);
var violations = await ruleEngine.ExecuteRuleAsync(rule, elements);
```

### Suppression Manager

Persist rule suppressions and filter out known exceptions with justification and optional expiration:

```csharp
var suppression = new SuppressionRecord
{
    RuleId = "LYR001",
    TargetFile = "src/Legacy/LegacyRepository.cs",
    Justification = "Legacy dependency scheduled for refactor",
    Author = "team-maintainer",
    CreatedAt = DateTime.UtcNow,
    ExpiresAt = DateTime.UtcNow.AddDays(30),
    IsActive = true
};

suppressionManager.AddSuppression(suppression);
await suppressionManager.SaveAsync("suppressions.json");
await suppressionManager.LoadAsync("suppressions.json");
var visibleViolations = suppressionManager.FilterSuppressed(violations);
```

### Fix-All Provider

Preview or apply bulk fixes with severity and rule filters:

```csharp
var result = await fixAllProvider.ApplyAllAsync(
    violations,
    new FixAllOptions
    {
        DryRun = false,
        MinimumSeverity = SeverityLevel.Warning,
        RuleIds = new[] { "RG-N001", "RG-A001" },
        SkipBreakingChanges = true,
        MaxFixes = 25
    });
```

### Diagnostic Codes Examples

#### LYR001: Layer Dependencies
**Non-compliant:**
```csharp
public class UserRepository
{
    private readonly UserService _service; // RGD001: Repository depends on service
}
```
**Compliant:**
```csharp
public class UserService
{
    private readonly UserRepository _repository; // OK
}
```

#### NAM001: Naming Conventions
**Non-compliant:**
```csharp
public class my_class
{
    private int PublicField;
}
```
**Compliant:**
```csharp
public class MyClass
{
    private int _publicField;
}
```

#### ASY001: Async Patterns (RGD003, RGD004)
**Non-compliant:**
```csharp
public Task DoWork() // Returns Task but not marked async
{
    return Task.FromResult(0);
}
public async Task DoWork2() // Missing Async suffix
{
}
```
**Compliant:**
```csharp
public async Task DoWorkAsync()
{
}
```

#### NUL001: Null Safety (RGD008, RGD009)
**Non-compliant:**
```csharp
public class MyClass
{
    public string Name { get; set; } // Non-nullable without initialization
}
```
**Compliant:**
```csharp
public class MyClass
{
    public string Name { get; set; } = string.Empty; // Initialized
}
```

## Quick Start

### Prerequisites

- **.NET 10.0** or higher
- **C# language support** (latest language features)
- Visual Studio Code, Visual Studio, or any .NET IDE

### One-Command Installation

```bash
# Clone and build
git clone https://github.com/sarmkadan/roslyn-guard-analyzer.git
cd roslyn-guard-analyzer
dotnet build -c Release

# Run analysis on a project
dotnet run --project src/RoslynGuardAnalyzer -- /path/to/your/project.csproj
```

### 30-Second Example

```bash
# Analyze your current project
cd ~/MyProject
roslyn-guard-analyzer .

# View results in JSON
roslyn-guard-analyzer . --format json

# Export to file
roslyn-guard-analyzer . --output analysis-report.json
```

## Installation

### Method 1: Clone and Build from Source

```bash
git clone https://github.com/sarmkadan/roslyn-guard-analyzer.git
cd roslyn-guard-analyzer
dotnet build -c Release

# Create a convenient alias
alias roslyn-guard='dotnet /path/to/roslyn-guard-analyzer/src/RoslynGuardAnalyzer/bin/Release/net10.0/RoslynGuardAnalyzer.dll'
```

### Method 2: NuGet Package (When Published)

```bash
dotnet tool install --global roslyn-guard-analyzer
roslyn-guard-analyzer --version
```

### Method 3: Docker Container

Run the analyzer in a containerized environment without installing .NET:

```bash
# Build the Docker image
docker build -t roslyn-guard-analyzer .

# Quick start: Analyze this repository
# (Mounts current directory and analyzes the analyzer itself)
docker run --rm -v $(pwd):/workspace roslyn-guard-analyzer /workspace/src/RoslynGuardAnalyzer/RoslynGuardAnalyzer.csproj

# Analyze your own project (replace PROJECT_PATH with your .csproj file or directory)
docker run --rm -v /path/to/your/project:/workspace roslyn-guard-analyzer /workspace/YourProject.csproj

# Or use docker-compose for more complex scenarios
# Copy and customize the docker-compose configuration
cp docker-compose.yml docker-compose.override.yml
# Edit docker-compose.override.yml to set your project path
nano docker-compose.override.yml

# Run the analyzer
docker-compose up analyzer

# Run in detached mode
docker-compose up -d analyzer
```

**Usage Notes:**
- The Docker image includes the Roslyn Guard Analyzer CLI tool
- Mount your source code with `-v` flag to analyze your projects
- The container runs as non-root user for security
- Resource limits are configured for optimal performance
- Reports can be saved to the `./reports` directory

### Method 4: Using Makefile

```bash
make build
make install
roslyn-guard-analyzer --help
```

## Usage Examples

### Example 1: Basic Project Analysis

Analyze an entire project directory:

```bash
roslyn-guard-analyzer ~/MyProject

# Output:
# === Roslyn Guard Analyzer ===
# Starting architecture rule analysis...
#
# File: src/Domain/UserRepository.cs:42
# Rule: LYR001 (Layer Dependencies)
# Violation: Repository depends on service layer
#
# Analysis completed: 3 violations found
```

### Example 2: Analyze Specific File

```bash
roslyn-guard-analyzer ~/MyProject/src/Services/UserService.cs
```

### Example 3: JSON Output for Tool Integration

```bash
roslyn-guard-analyzer ~/MyProject --format json > analysis.json

# Contents:
# {
#   "timestamp": "2026-05-04T10:30:00Z",
#   "projectPath": "/home/user/MyProject",
#   "totalFilesAnalyzed": 125,
#   "violations": [
#     {
#       "ruleId": "LYR001",
#       "category": "Layer Dependencies",
#       "filePath": "src/Domain/UserRepository.cs",
#       "line": 42,
#       "column": 5,
#       "message": "Repository class depends on service layer",
#       "severity": "error"
#     }
#   ]
# }
```

### Example 4: CSV Export for Spreadsheet Analysis

```bash
roslyn-guard-analyzer ~/MyProject --format csv --output violations.csv

# Opens in Excel/Sheets for sorting and filtering:
# RuleID,Category,File,Line,Column,Message,Severity
# LYR001,Layer Dependencies,src/Domain/UserRepository.cs,42,5,Repository depends on service,error
# NAM001,Naming,src/Services/userService.cs,15,7,Field should be snake_case,warning
```

### Example 5: HTML Report Generation

```bash
roslyn-guard-analyzer ~/MyProject --format html --output report.html
open report.html
```

### Example 6: Filtering by Rule

```bash
# Analyze only naming convention violations
roslyn-guard-analyzer ~/MyProject --rules NAM001

# Analyze multiple specific rules
roslyn-guard-analyzer ~/MyProject --rules LYR001,ASY001
```

### Example 7: Strict Mode (Fail on Any Violation)

```bash
roslyn-guard-analyzer ~/MyProject --strict
# Exit code 1 if any violations found
```

### Example 8: Custom Configuration File

Create `.roslyn-guard.json`:

```json
{
  "projectPath": "./src",
  "analysisTimeout": 600,
  "maxViolationsToReport": 1000,
  "rules": {
    "LYR001": { "enabled": true, "severity": "error" },
    "NAM001": { "enabled": true, "severity": "warning" },
    "ASY001": { "enabled": false },
    "NUL001": { "enabled": true, "severity": "error" }
  },
  "excludePatterns": [
    "**/bin/**",
    "**/obj/**",
    "**/*.Generated.cs"
  ]
}
```

```bash
roslyn-guard-analyzer --config .roslyn-guard.json
```

### Example 9: Continuous Integration Pipeline

GitHub Actions workflow:

```yaml
- name: Run Roslyn Guard Analyzer
  run: |
    dotnet run --project RoslynGuardAnalyzer -- ./src \
      --format json \
      --output analysis.json
    
    # Fail if critical violations found
    if [ $(jq '.violations | map(select(.severity=="error")) | length' analysis.json) -gt 0 ]; then
      echo "Architecture violations found!"
      exit 1
    fi
```

### Example 10: Custom Rule Integration

Implement a custom rule by extending `AnalysisRule`:

```csharp
public class CustomLayerRule : AnalysisRule
{
    public override string Id => "CUSTOM001";
    public override string Category => "Custom";
    public override string Description => "Enforce custom architectural rule";
    
    public override async Task<IEnumerable<RuleViolation>> ValidateAsync(
        CodeElement element,
        RuleConfiguration config)
    {
        var violations = new List<RuleViolation>();
        
        // Implement your custom logic
        if (element.Name.Contains("Temp"))
        {
            violations.Add(new RuleViolation
            {
                RuleId = Id,
                FilePath = element.FilePath,
                Line = element.Line,
                Message = "Temporary classes should not be committed"
            });
        }
        
        return violations;
    }
}
```

## Code-Based Examples

In addition to CLI-based examples, you can integrate Roslyn Guard Analyzer directly into your .NET applications. Check out the following examples in the [examples/](./examples/) directory:

- [BasicUsage.cs](./examples/BasicUsage.cs): Demonstrates minimal setup and running a basic project analysis.
- [AdvancedUsage.cs](./examples/AdvancedUsage.cs): Shows how to use custom configurations, custom rule registration, suppression management, and the fix-all provider.
- [IntegrationExample.cs](./examples/IntegrationExample.cs): Illustrates how to wire the analyzer into an ASP.NET Core DI container.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Roslyn Guard Analyzer                    │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              CLI & Command Processing                │  │
│  │  (CliArgumentParser, CliOptions, CommandLineProcessor) │  │
│  └──────────────────────────────────────────────────────┘  │
│                           ↓                                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         Configuration & Validation Layer             │  │
│  │  (ConfigurationLoader, ConfigurationValidator)      │  │
│  └──────────────────────────────────────────────────────┘  │
│                           ↓                                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Analysis Middleware Pipeline            │  │
│  │  • ErrorHandling                                      │  │
│  │  • Logging                                            │  │
│  │  • PerformanceMetrics                                 │  │
│  └──────────────────────────────────────────────────────┘  │
│                           ↓                                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │          Core Analysis Service Layer                 │  │
│  │  • AnalysisService (Orchestration)                   │  │
│  │  • RuleEngine (Rule Execution)                        │  │
│  │  • RuleRegistry (Rule Management)                     │  │
│  │  • DiagnosticsService (Roslyn Integration)           │  │
│  └──────────────────────────────────────────────────────┘  │
│                           ↓                                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Domain Models & Entities                │  │
│  │  • AnalysisRule                                       │  │
│  │  • RuleViolation                                      │  │
│  │  • CodeElement                                        │  │
│  │  • AnalysisResult                                     │  │
│  │  • ViolationReport                                    │  │
│  └──────────────────────────────────────────────────────┘  │
│                           ↓                                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │        Output Formatting & Reporting                 │  │
│  │  • JsonFormatter                                      │  │
│  │  • CsvFormatter                                       │  │
│  │  • HtmlFormatter                                      │  │
│  │  • FormatterRegistry                                  │  │
│  └──────────────────────────────────────────────────────┘  │
│                           ↓                                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │           Data Persistence Layer                      │  │
│  │  • AnalysisResultRepository                           │  │
│  │  • ProjectRepository                                  │  │
│  │  • RuleRepository                                     │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Cross-Cutting Concerns                  │  │
│  │  • EventBus (Pub/Sub)                                 │  │
│  │  • CacheService                                       │  │
│  │  • BackgroundTaskQueue                                │  │
│  │  • WebhookHandler                                     │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

**CLI Layer**: Parses command-line arguments, handles user interaction, and delegates to services

**Configuration Layer**: Loads and validates configuration from files and environment variables

**Middleware Pipeline**: Cross-cutting concerns (logging, error handling, performance metrics)

**Analysis Layer**: Core business logic for rule execution and violation detection

**Domain Layer**: Pure business entities with no dependencies on infrastructure

**Repository Layer**: Abstraction over data storage (currently in-memory, extensible)

**Output Layer**: Formats results for consumption by different tools and users

## API Reference

### IAnalysisService

Main service for orchestrating the analysis workflow.

```csharp
public interface IAnalysisService
{
    /// <summary>
    /// Analyzes a project or file asynchronously
    /// </summary>
    /// <param name="projectPath">Path to project.csproj or individual .cs file</param>
    /// <returns>Analysis results including violations found</returns>
    Task<AnalysisResult> AnalyzeProjectAsync(string projectPath);
    
    /// <summary>
    /// Analyzes with custom configuration
    /// </summary>
    Task<AnalysisResult> AnalyzeWithConfigAsync(
        string projectPath,
        RuleConfiguration configuration);
}
```

### IRuleRegistry

Manages available rules and their configurations.

```csharp
public interface IRuleRegistry
{
    /// <summary>
    /// Gets all registered rules
    /// </summary>
    IEnumerable<AnalysisRule> GetAllRules();
    
    /// <summary>
    /// Registers a new rule
    /// </summary>
    void RegisterRule(AnalysisRule rule);
    
    /// <summary>
    /// Gets a specific rule by ID
    /// </summary>
    AnalysisRule? GetRule(string ruleId);
    
    /// <summary>
    /// Enables or disables a rule
    /// </summary>
    void SetRuleEnabled(string ruleId, bool enabled);
}
```

### IRuleEngine

Executes rules against code elements.

```csharp
public interface IRuleEngine
{
    /// <summary>
    /// Executes all enabled rules against a code element
    /// </summary>
    /// <returns>Violations found by all rules</returns>
    Task<IEnumerable<RuleViolation>> ExecuteRulesAsync(
        CodeElement element);
    
    /// <summary>
    /// Executes a specific rule
    /// </summary>
    Task<IEnumerable<RuleViolation>> ExecuteRuleAsync(
        string ruleId,
        CodeElement element);
}
```

### IReportingService

Generates formatted reports from analysis results.

```csharp
public interface IReportingService
{
    /// <summary>
    /// Generates a human-readable text report
    /// </summary>
    string GenerateReport(AnalysisResult result);
    
    /// <summary>
    /// Generates a JSON report
    /// </summary>
    string GenerateJsonReport(AnalysisResult result);
    
    /// <summary>
    /// Generates a CSV report
    /// </summary>
    string GenerateCsvReport(AnalysisResult result);
    
    /// <summary>
    /// Generates an HTML report
    /// </summary>
    string GenerateHtmlReport(AnalysisResult result);
}
```

### IValidationService

Validates configurations and code elements.

```csharp
public interface IValidationService
{
    /// <summary>
    /// Validates a rule configuration
    /// </summary>
    /// <returns>Validation errors, empty if valid</returns>
    IEnumerable<string> ValidateConfiguration(RuleConfiguration config);
    
    /// <summary>
    /// Validates a code element
    /// </summary>
    bool IsValidCodeElement(CodeElement element);
}
```

### Domain Models

#### AnalysisRule

Base class for implementing custom rules:

```csharp
public abstract class AnalysisRule
{
    public abstract string Id { get; }
    public abstract string Category { get; }
    public abstract string Description { get; }
    public virtual RuleSeverity DefaultSeverity => RuleSeverity.Error;
    
    public abstract Task<IEnumerable<RuleViolation>> ValidateAsync(
        CodeElement element,
        RuleConfiguration config);
}
```

#### RuleViolation

Represents a single violation:

```csharp
public class RuleViolation
{
    public string RuleId { get; set; }
    public string FilePath { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    public string Message { get; set; }
    public RuleSeverity Severity { get; set; }
    public CodeElement? Element { get; set; }
}
```

#### AnalysisResult

Complete results from an analysis:

```csharp
public class AnalysisResult
{
    public string ProjectPath { get; set; }
    public DateTime TimestampUtc { get; set; }
    public int TotalFilesAnalyzed { get; set; }
    public List<RuleViolation> Violations { get; set; }
    public int ViolationCount => Violations.Count;
    public AnalysisStatistics Statistics { get; set; }
}
```

## Configuration Reference

Roslyn Guard Analyzer supports multiple configuration approaches:

### 1. appsettings.json (Recommended - IOptions Pattern)

Create an `appsettings.json` file in your project root or use the provided `appsettings.example.json`:

```json
{
  "RoslynGuardAnalyzer": {
    "ProjectPath": "./",
    "AnalysisTimeoutSeconds": 600,
    "MaxViolationsToReport": 1000,
    "LogLevel": 2,
    "OutputFormat": "text",
    "OutputFile": null,
    "GenerateReport": true,
    "ReportType": "summary",
    "FailOnViolations": true,
    "SkipCache": false,
    "MaxParallelThreads": 8,
    "RuleFilter": [],
    "ExcludePatterns": [
      "**/bin/**",
      "**/obj/**",
      "**/*.Generated.cs",
      "**/*.Designer.cs"
    ],
    "MinimumSeverity": "Low",
    "ConfigFile": null
  }
}
```

**Configuration Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ProjectPath` | string | `./` | Root path for analysis |
| `AnalysisTimeoutSeconds` | int | `600` | Timeout in seconds (1-max int) |
| `MaxViolationsToReport` | int | `1000` | Maximum violations to include (1-100000) |
| `LogLevel` | int | `2` | Verbosity (0=none, 1=errors, 2=warnings, 3=info, 4=debug) |
| `OutputFormat` | string | `text` | Output format (text, json, csv, html, xml) |
| `OutputFile` | string | null | Output file path (optional) |
| `GenerateReport` | bool | `true` | Whether to generate a report |
| `ReportType` | string | `summary` | Report type (summary, detailed, full) |
| `FailOnViolations` | bool | `true` | Exit with code 1 if violations found |
| `SkipCache` | bool | `false` | Skip caching for fresh analysis |
| `MaxParallelThreads` | int | CPU count | Maximum parallel threads (1-64) |
| `RuleFilter` | string[] | [] | Comma-separated rule IDs to execute |
| `ExcludePatterns` | string[] | [bin/**, obj/**, *.Generated.cs, *.Designer.cs] | Glob patterns to exclude |
| `MinimumSeverity` | string | `Low` | Minimum severity to report (Low, Medium, High, Critical) |
| `ConfigFile` | string | null | Path to custom configuration file |

### 2. .roslyn-guard.json (Legacy Format)

For backward compatibility, you can still use `.roslyn-guard.json`:

```json
{
  "projectPath": "./src",
  "analysisTimeout": 600,
  "maxViolationsToReport": 1000,
  "logLevel": 2,
  "rules": {
    "LYR001": {
      "enabled": true,
      "severity": "error",
      "configuration": {
        "allowedDependencies": ["Domain", "Infrastructure"]
      }
    },
    "NAM001": {
      "enabled": true,
      "severity": "warning"
    },
    "ASY001": {
      "enabled": true,
      "severity": "error"
    },
    "NUL001": {
      "enabled": true,
      "severity": "warning"
    }
  },
  "excludePatterns": [
    "**/bin/**",
    "**/obj/**",
    "**/*.Generated.cs",
    "**/*.Designer.cs"
  ]
}
```

### 3. Environment Variables

All configuration properties can be set via environment variables with the prefix `RoslynGuardAnalyzer__`:

```bash
export RoslynGuardAnalyzer__ProjectPath="./src"
export RoslynGuardAnalyzer__AnalysisTimeoutSeconds=900
export RoslynGuardAnalyzer__OutputFormat="json"
export RoslynGuardAnalyzer__MaxViolationsToReport=500

# Run the analyzer
roslyn-guard-analyzer ./src
```

### 4. Command-Line Arguments (Highest Priority)

Command-line arguments override all configuration sources:

```bash
# Basic usage
roslyn-guard-analyzer ./src

# With custom format
roslyn-guard-analyzer ./src --format json --output report.json

# With specific rules
roslyn-guard-analyzer ./src --rules LYR001,NAM001

# With custom config file
roslyn-guard-analyzer --config ./custom-config.json

# Verbose output
roslyn-guard-analyzer ./src --verbose

# Fail on any violation
roslyn-guard-analyzer ./src --strict
```

### Configuration Precedence

Configuration values are applied in the following order (later sources override earlier ones):

1. **Default values** (hardcoded in `RoslynGuardAnalyzerOptions`)
2. **appsettings.json** (if present)
3. **Environment variables** (if set)
4. **Command-line arguments** (highest priority)

This allows you to:
- Set sensible defaults in code
- Override with environment-specific settings via appsettings.json
- Tweak for specific runs via environment variables
- Force specific values via command-line for CI/CD pipelines

### Validation

The analyzer validates all configuration values and will report errors for:
- Invalid enum values (OutputFormat, MinimumSeverity, ReportType)
- Out-of-range numeric values (AnalysisTimeoutSeconds, MaxViolationsToReport, etc.)
- Missing required values (ProjectPath)
- Invalid file paths (if specified)

Validation errors are displayed before analysis begins, preventing misconfiguration.

### Configuration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `projectPath` | string | `./` | Root path for analysis |
| `analysisTimeout` | int | `600` | Timeout in seconds |
| `maxViolationsToReport` | int | `500` | Maximum violations to include in report |
| `logLevel` | int | `2` | Verbosity (0=none, 1=errors, 2=warnings, 3=info, 4=debug) |
| `excludePatterns` | string[] | `["**/bin/**", "**/obj/**"]` | Glob patterns to exclude |

### Rule Configuration

Each rule can be individually configured:

```json
{
  "rules": {
    "LYR001": {
      "enabled": true,
      "severity": "error"
    }
  }
}
```

## CLI Reference

### Global Options

```bash
roslyn-guard-analyzer <path> [options]
```

| Option | Short | Description |
|--------|-------|-------------|
| `--format` | `-f` | Output format: `text`, `json`, `csv`, `xml`, `html` |
| `--output` | `-o` | Output file path (optional) |
| `--config` | `-c` | Configuration file path |
| `--rules` | `-r` | Comma-separated rule IDs to execute |
| `--strict` | `-s` | Fail on any violation (exit code 1) |
| `--quiet` | `-q` | Suppress console output |
| `--verbose` | `-v` | Verbose logging |
| `--help` | `-h` | Show help message |
| `--version` | | Show version information |

### Examples

```bash
# Basic analysis with default settings
roslyn-guard-analyzer ./src

# JSON output for CI/CD
roslyn-guard-analyzer ./src -f json -o report.json

# Only specific rules
roslyn-guard-analyzer ./src -r LYR001,NAM001

# Use config file
roslyn-guard-analyzer -c ./analyzer.json

# Verbose output for debugging
roslyn-guard-analyzer ./src -v

# Fail if violations found
roslyn-guard-analyzer ./src -s && echo "Analysis passed" || echo "Violations found"
```

## Troubleshooting

### Problem: "Project path not found"

**Solution**: Verify the path exists and is accessible:
```bash
ls -la /path/to/project.csproj
roslyn-guard-analyzer /path/to/project.csproj
```

### Problem: "No violations found but expected some"

**Solution**: Check if rules are enabled in configuration:
```bash
# Verbose output shows which rules are active
roslyn-guard-analyzer ./src -v

# Verify rule is not disabled
cat .roslyn-guard.json | grep -A2 '"LYR001"'
```

### Problem: "Analysis timeout"

**Solution**: Increase timeout in configuration:
```json
{
  "analysisTimeout": 1800
}
```

### Problem: "Out of memory on large projects"

**Solution**: Analyze files in batches:
```bash
# Analyze one directory at a time
roslyn-guard-analyzer ./src/Domain
roslyn-guard-analyzer ./src/Services
roslyn-guard-analyzer ./src/Presentation
```

### Problem: "False positives in generated code"

**Solution**: Exclude generated files:
```json
{
  "excludePatterns": [
    "**/*.Generated.cs",
    "**/*.Designer.cs",
    "**/obj/**"
  ]
}
```

### Problem: "Custom rule not executing"

**Solution**: Verify rule is registered:
```csharp
var ruleRegistry = serviceProvider.GetRequiredService<IRuleRegistry>();
var myRule = ruleRegistry.GetRule("CUSTOM001");
if (myRule == null)
    throw new Exception("Rule not registered");
```

## Testing

The test suite covers the rule engine, string utilities, and type-name matching logic.

### Running Tests

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure

| Test File | What It Covers |
|-----------|----------------|
| `RuleRegistryTests.cs` | Rule registration, lookup, enable/disable |
| `StringExtensionsTests.cs` | String utility helpers used throughout analysis |
| `TypeNameMatcherTests.cs` | Pattern matching for type names in rule evaluation |

### Writing Tests for Custom Rules

```csharp
[Fact]
public async Task MyCustomRule_WhenNameContainsTemp_ReturnsViolation()
{
    var rule = new MyCustomRule();
    var element = new CodeElement { Name = "TempService", FilePath = "src/TempService.cs", Line = 1 };
    var config = new RuleConfiguration { Enabled = true };

    var violations = await rule.ValidateAsync(element, config);

    Assert.Single(violations);
    Assert.Equal("CUSTOM001", violations.First().RuleId);
}
```

## Performance

Roslyn Guard Analyzer is designed for fast, low-overhead analysis suitable for both local development and CI/CD pipelines.

### Benchmarks

| Scenario | Mean | Error | StdDev | Allocated |
|----------|-----:|------:|-------:|----------:|
| Single Rule Execution | 6.841 us | 0.136 us | 0.228 us | 2.98 KB |
| Full Rule Suite Execution | 18.102 us | 0.358 us | 0.815 us | 8.76 KB |

*Benchmarks measured on .NET 10.0, AMD EPYC-Rome Processor, Ubuntu 26.04.*

You can run these benchmarks yourself using the `tests/roslyn-guard-analyzer.Benchmarks` project:

```bash
dotnet run --project tests/roslyn-guard-analyzer.Benchmarks/roslyn-guard-analyzer.Benchmarks.csproj -c Release
```

## Documentation

Detailed guides are available in the [`docs/`](./docs/) directory:

| Document | Description |
|---|---|
| [Getting Started](./docs/getting-started.md) | Installation, first analysis, and built-in rules |
| [Custom Rule Development](./docs/custom-rule-development.md) | Writing, configuring, and testing your own rules |
| [API Reference](./docs/api-reference.md) | Full interface and model documentation |
| [Architecture Guide](./docs/architecture.md) | Internal design decisions |
| [Deployment Guide](./docs/deployment.md) | CI/CD and production setup |
| [FAQ](./docs/faq.md) | Common questions and troubleshooting |
| [Migration v2](./docs/MIGRATION_v2.md) | Upgrading from v1 to v2 |

## RuleViolationExtensions

The `RuleViolationExtensions` class provides a set of fluent extension methods for working with `RuleViolation` objects. These methods enable you to:

- Create modified copies of violations with updated properties using `WithMessage`, `WithLocation`, `WithSeverity`, `WithMetadata`, and `WithDetectedAt`
- Query violation categories using `HasCategory` and `HasAnyCategory`
- Format code snippets with line numbers using `GetFormattedCodeSnippet`

All methods return new `RuleViolation` instances rather than modifying the original, following immutable design patterns for thread safety and predictable behavior.

### Usage Example

```csharp
// Start with a violation from the rule engine
var violations = await ruleEngine.ExecuteRulesAsync(codeElement);
var originalViolation = violations.First();

// Use extension methods to create modified copies
var updatedViolation = originalViolation
    .WithMessage("Method 'ProcessData' must be marked as async")
    .WithSeverity(SeverityLevel.Warning)
    .WithMetadata("suppressionId", "SUP-123")
    .WithMetadata("reviewer", "john.doe@company.com")
    .WithDetectedAt(DateTime.UtcNow.AddDays(-1));

// Check violation categories
if (originalViolation.HasCategory(RuleCategory.AsyncPattern))
{
    Console.WriteLine("This is an async pattern violation");
}

if (originalViolation.HasAnyCategory(RuleCategory.AsyncPattern, RuleCategory.LayerDependencies))
{
    Console.WriteLine("This is either an async or layer violation");
}

// Get formatted code snippet with line numbers
var snippet = originalViolation.GetFormattedCodeSnippet();
if (snippet != null)
{
    Console.WriteLine("Code snippet:");
    Console.WriteLine(snippet);
}

// Create a new violation at a different location
var relocatedViolation = originalViolation.WithLocation(
    "src/Services/UserService.cs",
    42,
    5
);
```

## Related Projects

Part of a collection of .NET libraries and tools. See more at [github.com/sarmkadan](https://github.com/sarmkadan).

### Integration Examples

**Embed the analyzer in a custom build tool or pre-commit hook:**

```csharp
var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services => services.AddRoslynGuardAnalyzer())
    .Build();

var analyzer = host.Services.GetRequiredService<IAnalysisService>();
var result = await analyzer.AnalyzeProjectAsync("./src/MyApp.csproj");

if (result.Violations.Any(v => v.Severity == RuleSeverity.Error))
    Environment.Exit(1);
```

**Register a project-specific rule alongside the built-in set:**

```csharp
var registry = host.Services.GetRequiredService<IRuleRegistry>();
registry.RegisterRule(new DomainEventsNamingRule());   // custom rule
registry.RegisterRule(new OutboxPatternRule());         // custom rule

var engine = host.Services.GetRequiredService<IRuleEngine>();
var violations = await engine.ExecuteRulesAsync(codeElement);
Console.WriteLine($"{violations.Count()} violation(s) found.");
```

## Contributing

Contributions are welcome! Here's how to get started:

### Development Setup

```bash
git clone https://github.com/sarmkadan/roslyn-guard-analyzer.git
cd roslyn-guard-analyzer
dotnet restore
dotnet build
```

### Adding a Custom Rule

1. Create a rule class in `src/RoslynGuardAnalyzer/Rules/`:

```csharp
public class MyCustomRule : AnalysisRule
{
    public override string Id => "CUSTOM001";
    public override string Category => "Custom";
    public override string Description => "My custom rule";
    
    public override async Task<IEnumerable<RuleViolation>> ValidateAsync(
        CodeElement element,
        RuleConfiguration config)
    {
        // Implementation here
        return new List<RuleViolation>();
    }
}
```

2. Register it in `ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<AnalysisRule, MyCustomRule>();
```

3. Add tests in `tests/` directory

4. Submit a pull request with:
   - Rule implementation
   - Unit tests (>80% coverage)
   - Documentation
   - Example usage

### Reporting Issues

Please include:
- .NET version
- Project type (.csproj structure)
- Reproduction steps
- Expected vs actual behavior
- Configuration file (if applicable)

### Code Style

- Follow C# naming conventions
- Use async/await throughout
- Add XML documentation comments
- Target .NET 10.0 minimum
- Enable nullable reference types

## License

MIT License - Copyright © 2026 Vladyslav Zaiets

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

See [LICENSE](LICENSE) for full details.

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)



