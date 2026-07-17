# CacheKeyGeneratorValidation

`CacheKeyGeneratorValidation` is a utility class that provides a set of static validation methods for the various cache key generation functions used throughout the Roslyn Guard Analyzer. Each method returns a read‑only list of validation error messages that describe why a particular key generation call may be invalid. The class is intentionally stateless and thread‑safe, making it suitable for use in concurrent environments such as background task queues or middleware pipelines.

## API

### `public static IReadOnlyList<string> ValidateGenerateProjectAnalysisKey()`

- **Purpose**: Validates the arguments used to generate a cache key for a project analysis result.
- **Parameters**: None.
- **Return Value**: A read‑only list of error messages. An empty list indicates that the key generation arguments are valid.
- **Throws**: None.

### `public static IReadOnlyList<string> ValidateGenerateFileAnalysisKey()`

- **Purpose**: Validates the arguments used to generate a cache key for a file analysis result.
- **Parameters**: None.
- **Return Value**: A read‑only list of error messages. An empty list indicates that the key generation arguments are valid.
- **Throws**: None.

### `public static IReadOnlyList<string> ValidateGenerateResultKey()`

- **Purpose**: Validates the arguments used to generate a cache key for a generic analysis result.
- **Parameters**: None.
- **Return Value**: A read‑only list of error messages. An empty list indicates that the key generation arguments are valid.
- **Throws**: None.

### `public static IReadOnlyList<string> ValidateGenerateRuleExecutionKey()`

- **Purpose**: Validates the arguments used to generate a cache key for a rule execution result.
- **Parameters**: None.
- **Return Value**: A read‑only list of error messages. An empty list indicates that the key generation arguments are valid.
- **Throws**: None.

### `public static IReadOnlyList<string> ValidateGenerateCodeElementKey()`

- **Purpose**: Validates the arguments used to generate a cache key for a code element.
- **Parameters**: None.
- **Return Value**: A read‑only list of error messages. An empty list indicates that the key generation arguments are valid.
- **Throws**: None.

### `public static IReadOnlyList<string> ValidateComputeHash()`

- **Purpose**: Validates the arguments used to compute a hash value for caching purposes.
- **Parameters**: None.
- **Return Value**: A read‑only list of error messages. An empty list indicates that the hash computation arguments are valid.
- **Throws**: None.

### `public static IReadOnlyList<string> ValidateComputeFileHash()`

- **Purpose**: Validates the arguments used to compute a hash for a file.
- **Parameters**: None.
- **Return Value**: A read‑only list of error messages. An empty list indicates that the file hash computation arguments are valid.
- **Throws**: None.

### `public static IReadOnlyList<string> ValidateCreateCompositeKey()`

- **Purpose**: Validates the arguments used to create a composite cache key from multiple components.
- **Parameters**: None.
- **Return Value**: A read‑only list of error messages. An empty list indicates that the composite key creation arguments are valid.
- **Throws**: None.

### `public static IReadOnlyList<string> ValidateGeneratePatternKey()`

- **Purpose**: Validates the arguments used to generate a cache key for a pattern match operation.
- **Parameters**: None.
- **Return Value**: A read‑only list of error messages. An empty list indicates that the pattern key generation arguments are valid.
- **Throws**: None.

## Usage

