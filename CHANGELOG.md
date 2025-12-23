# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-05-04

### Added
- **Docker Support**: Complete Docker and docker-compose configuration for containerized analysis
- **HTML Report Generation**: New HTML formatter with interactive charts and filtering capabilities
- **Webhook Integration**: Support for sending analysis results to external endpoints
- **Incremental Analysis**: Cache-based analysis for improved performance on unchanged projects
- **Event Bus System**: Publish/subscribe architecture for extensibility and monitoring
- **Custom Rule API**: Formal extension API for implementing custom architectural rules
- **Background Task Queue**: Async task processing for long-running analysis operations
- **Performance Metrics**: Built-in profiling and statistics collection

### Changed
- **Improved Middleware Pipeline**: Fully composable cross-cutting concerns architecture
- **Enhanced CLI**: Better help text, command suggestions, and error messages
- **Parallel Processing**: Automatic utilization of all CPU cores for faster analysis
- **Better Caching**: Intelligent result caching with configurable cache strategies

### Fixed
- **Memory Leaks**: Fixed resource disposal in Roslyn syntax tree processing
- **Configuration Loading**: Improved environment variable interpolation
- **Null Reference Handling**: Better error messages for missing configurations
- **Pattern Matching**: More accurate exclusion pattern matching

### Documentation
- Added comprehensive getting-started guide
- Added architecture documentation with ASCII diagrams
- Added deployment guide for production environments
- Added FAQ with troubleshooting section
- Added 8 example scripts for common use cases

## [1.1.0] - 2026-04-15

### Added
- **CSV Report Format**: Export analysis results as spreadsheet-compatible CSV
- **XML Report Format**: Structured XML output for tool integration
- **Rule Severity Configuration**: Per-rule severity customization
- **Exclude Patterns**: Flexible glob-based file exclusion
- **Analysis Statistics**: Detailed metrics and summary statistics
- **Configuration File Support**: JSON-based project configuration via `.roslyn-guard.json`

### Changed
- **Rule Registry**: Refactored for better extensibility and testing
- **Dependency Injection**: Upgraded to latest Microsoft.Extensions.DependencyInjection
- **Error Handling**: More specific exception types for different error scenarios

### Fixed
- **False Positives**: Improved naming convention detection for generic types
- **Performance**: Optimized Roslyn syntax tree parsing for large files
- **Configuration Validation**: Better validation of rule configurations

## [1.0.0] - 2026-03-20

### Added
- **Initial Release**: Production-ready code analyzer powered by Roslyn
- **Four Core Rules**:
  - LYR001: Layer Dependency Analysis - Enforces architectural layers
  - NAM001: Naming Convention - Validates naming patterns
  - ASY001: Async Pattern Detection - Identifies improper async/await usage
  - NUL001: Null Safety Validation - Enforces nullable reference types
- **Multiple Report Formats**:
  - Text: Human-readable console output
  - JSON: Machine-readable format for tool integration
- **CLI Interface**: Command-line tool with argument parsing
- **Project Analysis**: Automatic discovery of C# files in projects
- **Rule Registry**: Extensible rule system with runtime registration
- **Dependency Injection**: Service configuration and management
- **Cross-Platform Support**: Runs on Windows, macOS, and Linux

## [0.9.0] - 2026-03-10

### Added
- Beta release for community feedback
- Core analyzer infrastructure
- Basic rule implementation framework
- CLI argument parsing

### Known Issues
- Limited error messages
- Performance not optimized for large projects
- No caching support

## [0.8.0] - 2026-02-25

### Added
- Alpha release for testing
- Initial Roslyn integration
- Basic rule engine implementation

## [0.7.0] - 2026-02-10

### Added
- Project planning and architecture design
- Initial repository setup
- Development infrastructure

---

## Upgrade Guide

### From 1.1.0 to 1.2.0

No breaking changes. Migration is straightforward:

```bash
# Simply update to latest version
dotnet tool update --global roslyn-guard-analyzer

# Optionally enable new features in configuration
{
  "webhookConfiguration": {
    "enabled": true,
    "url": "https://your-api.com/analysis"
  }
}
```

### From 1.0.0 to 1.1.0

Configuration format unchanged. Ensure you update for new features:

```bash
# New CSV format available
roslyn-guard-analyzer ./src --format csv --output report.csv

# New rule configuration options
{
  "rules": {
    "LYR001": {
      "enabled": true,
      "severity": "error"
    }
  }
}
```

### From 0.x to 1.0.0

Major changes - see migration guide:

```bash
# CLI arguments changed slightly
# Old: roslyn-guard-analyzer -p ./src
# New: roslyn-guard-analyzer ./src

# New configuration file format
{
  "projectPath": "./src",
  "analysisTimeout": 600,
  "rules": {
    "LYR001": { "enabled": true }
  }
}
```

---

## Roadmap

### Planned for 1.3.0
- [ ] IDE integration (VS Code, Visual Studio)
- [ ] Language Server Protocol (LSP) support
- [ ] Real-time analysis in development environment
- [ ] Persistent analysis cache across runs

### Planned for 1.4.0
- [ ] Plugin system for third-party rules
- [ ] Custom report templates
- [ ] Integration with issue tracking systems
- [ ] Performance profiling improvements

### Future Considerations
- [ ] Support for additional .NET languages (F#, VB.NET)
- [ ] Analysis history and trend tracking
- [ ] Machine learning-based violation prediction
- [ ] Automated violation fixing capabilities

---

## Contributors

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

### Credits
- Built on [Roslyn](https://github.com/dotnet/roslyn) compiler platform
- Uses [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/runtime)
- Inspired by ESLint and StyleLint ecosystems

---

For detailed version history and commits, see [GitHub Releases](https://github.com/sarmkadan/roslyn-guard-analyzer/releases) and [commit log](https://github.com/sarmkadan/roslyn-guard-analyzer/commits/main).
