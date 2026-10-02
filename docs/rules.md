# Roslyn Guard Analyzer Rules

This document provides a comprehensive list of all the rules enforced by the Roslyn Guard Analyzer, including their IDs, titles, categories, default severities, descriptions, and examples.

## LYR001 - Layer Dependency Violation

| Property | Value |
|----------|-------|
| ID | LYR001 |
| Title | Layer Dependency Violation |
| Category | RoslynGuardAnalyzer.LayerDependency |
| Default Severity | Error |
| Description | Ensures that types in a layer only depend on types in the same or lower layers. |

### Example

