# ServiceCollectionExtensionsValidation

A static utility class providing validation methods for service collection configurations in the `roslyn-guard-analyzer` project. These methods enforce constraints on configuration values such as data directories, integers, log levels, and report formats, returning validation errors or ensuring validity through exceptions.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate()
```

Validates the current service collection configuration. Returns a list of error messages describing any validation failures. Does not throw exceptions; errors are returned as strings.

---

### IsValid

```csharp
public static bool IsValid()
```

Checks whether the current service collection configuration is valid. Returns `true` if no validation errors exist, `false` otherwise.

---

### EnsureValid

```csharp
public static void EnsureValid()
```

Validates the current service collection configuration and throws an exception if any errors are found. The exception message contains all validation errors.

---

### ValidateDataDirectory

```csharp
public static IReadOnlyList<string> ValidateDataDirectory(string path, string parameterName)
```

Validates that the specified `path` is a valid data directory. Parameters:
- `path`: The directory path to validate.
- `parameterName`: The name of the parameter being validated (used in error messages).

Returns a list of error messages if the path is invalid (e.g., null, empty, or non-existent directory).

---

### ValidatePositiveInt

```csharp
public static IReadOnlyList<string> ValidatePositiveInt(int value, string parameterName)
```

Validates that the specified `value` is a positive integer. Parameters:
- `value`: The integer to validate.
- `parameterName`: The name of the parameter being validated.

Returns error messages if the value is less than or equal to zero.

---

### ValidateLogLevel

```csharp
public static IReadOnlyList<string> ValidateLogLevel(string level, string parameterName)
```

Validates that the specified `level` is a recognized log level. Parameters:
- `level`: The log level string to validate (e.g., "Debug", "Info").
- `parameterName`: The name of the parameter being validated.

Returns error messages if the level is not supported.

---

### ValidateReportFormat

```csharp
public static IReadOnlyList<string> ValidateReportFormat(string format, string parameterName)
```

Validates that the specified `format` is a valid report output format. Parameters:
- `format`: The format string to validate (e.g., "Json", "Xml").
- `parameterName`: The name of the parameter being validated.

Returns error messages if the format is not recognized.

## Usage

### Example 1: Validating Configuration Values

```csharp
var errors = new List<string>();

errors.AddRange(ServiceCollectionExtensionsValidation.ValidatePositiveInt(
    configuration.MaxRetryAttempts, 
    nameof(configuration.MaxRetryAttempts)
));

errors.AddRange(ServiceCollectionExtensionsValidation.ValidateLogLevel(
    configuration.LogLevel, 
    nameof(configuration.LogLevel)
));

if (errors.Any())
{
    throw new InvalidOperationException($"Configuration errors: {string.Join(", ", errors)}");
}
```

### Example 2: Ensuring Valid Service Collection

```csharp
ServiceCollectionExtensionsValidation.EnsureValid();
```

This method is typically called during application startup to enforce configuration validity before services are registered.

## Notes

- **Edge Cases**:
  - `ValidatePositiveInt` considers zero as invalid.
  - `ValidateDataDirectory` may return errors for non-absolute paths or paths with invalid characters.
  - `ValidateLogLevel` and `ValidateReportFormat` are case-sensitive and require exact matches to supported values.

- **Thread Safety**:
  - All methods are thread-safe as they do not modify shared state and rely only on input parameters.
  - Concurrent calls to `Validate` or `IsValid` will not interfere with each other.
