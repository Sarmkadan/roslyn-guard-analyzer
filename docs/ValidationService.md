# ValidationService

Provides validation utilities for rule configurations, project paths, code elements, and analysis results in the Roslyn Guard Analyzer project. The service offers methods to validate various aspects of analyzer rules and their configurations, ensuring correctness before execution.

## API

### `ValidateRuleConfiguration`

Validates the configuration of a rule, checking for completeness and correctness of its settings.

- **Parameters**: None
- **Return Value**: A tuple `(bool IsValid, List<string> Errors)` where `IsValid` indicates whether the configuration is valid, and `Errors` contains a list of validation error messages if `IsValid` is `false`.
- **Exceptions**: Does not throw exceptions; all errors are returned in the `Errors` list.

### `ValidateRule`

Validates a rule definition, ensuring it meets the required structural and semantic constraints.

- **Parameters**: None
- **Return Value**: A tuple `(bool IsValid, List<string> Errors)` where `IsValid` indicates whether the rule is valid, and `Errors` contains a list of validation error messages if `IsValid` is `false`.
- **Exceptions**: Does not throw exceptions; all errors are returned in the `Errors` list.

### `ValidateProjectPath`

Validates a project file path to ensure it exists and is accessible.

- **Parameters**: None
- **Return Value**: A tuple `(bool IsValid, string? Error)` where `IsValid` indicates whether the path is valid, and `Error` contains a validation error message if `IsValid` is `false`.
- **Exceptions**: Does not throw exceptions; errors are returned in the `Error` string.

### `ValidateCodeElement`

Validates a code element (e.g., class, method, property) for compliance with analyzer rules.

- **Parameters**: None
- **Return Value**: A tuple `(bool IsValid, List<string> Errors)` where `IsValid` indicates whether the code element is valid, and `Errors` contains a list of validation error messages if `IsValid` is `false`.
- **Exceptions**: Does not throw exceptions; all errors are returned in the `Errors` list.

### `ValidateAnalysisResult`

Validates the results of an analysis pass, ensuring they conform to expected formats and constraints.

- **Parameters**: None
- **Return Value**: A tuple `(bool IsValid, List<string> Errors)` where `IsValid` indicates whether the analysis result is valid, and `Errors` contains a list of validation error messages if `IsValid` is `false`.
- **Exceptions**: Does not throw exceptions; all errors are returned in the `Errors` list.

### `IsValidIdentifier`

Static method that checks whether a given string is a valid C# identifier.

- **Parameters**:
  - `identifier` (string): The string to validate.
- **Return Value**: `true` if the string is a valid C# identifier; otherwise, `false`.
- **Exceptions**: Does not throw exceptions.

### `IsPascalCase`

Static method that checks whether a given string follows PascalCase naming conventions.

- **Parameters**:
  - `value` (string): The string to validate.
- **Return Value**: `true` if the string follows PascalCase conventions; otherwise, `false`.
- **Exceptions**: Does not throw exceptions.

### `IsCamelCase`

Static method that checks whether a given string follows camelCase naming conventions.

- **Parameters**:
  - `value` (string): The string to validate.
- **Return Value**: `true` if the string follows camelCase conventions; otherwise, `false`.
- **Exceptions**: Does not throw exceptions.

## Usage

### Example 1: Validating a Rule Configuration
