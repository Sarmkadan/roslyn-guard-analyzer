# FormatterRegistry

A central registry that maps output format identifiers to `IOutputFormatter` implementations, enabling the analyzer to select the appropriate formatter for a given output target.

## API

### `public static FormatterRegistry CreateWithDefaults`
**Purpose**  
Creates a new `FormatterRegistry` instance prepopulated with the set of formatters shipped by the library.

**Parameters**  
None.

**Return value**  
A fully initialized `FormatterRegistry` ready for use.

**Exceptions**  
None.

### `public void Register(IOutputFormatter formatter)`
**Purpose**  
Adds a custom formatter to the registry, making it available for subsequent lookup operations.

**Parameters**  
- `formatter`: The formatter instance to register. Must not be `null`.

**Return value**  
None.

**Exceptions**  
- `ArgumentNullException` if `formatter` is `null`.  
- `InvalidOperationException` if a formatter for the same format identifier is already registered.

### `public IOutputFormatter? GetFormatter(string format)`
**Purpose**  
Retrieves the formatter associated with the specified format identifier, if one exists.

**Parameters**  
- `format`: The format identifier to look up (case‑sensitive).

**Return value**  
The matching `IOutputFormatter`, or `null` when no formatter is registered for `format`.

**Exceptions**  
- `ArgumentNullException` if `format` is `null`.

### `public bool IsFormatSupported(string format)`
**Purpose**  
Determines whether a formatter is registered for the given format identifier.

**Parameters**  
- `format`: The format identifier to test.

**Return value**  
`true` if a formatter exists for `format`; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` if `format` is `null`.

### `public IEnumerable<string> GetSupportedFormats()`
**Purpose**  
Enumerates all format identifiers for which formatters are currently registered.

**Parameters**  
None.

**Return value**  
A read‑only collection of format strings. The order is not guaranteed.

**Exceptions**  
None.

### `public IOutputFormatter GetFormatterOrThrow(string format)`
**Purpose**  
Retrieves the formatter for the specified format identifier, throwing an exception if none is found.

**Parameters**  
- `format`: The format identifier to look up.

**Return value**  
The `IOutputFormatter` associated with `format`.

**Exceptions**  
- `ArgumentNullException` if `format` is `null`.  
- `KeyNotFoundException` (or a derived type) when no formatter is registered for `format`.

## Usage

### Example 1: Building a registry with default formatters and adding a custom one
```csharp
using RoslynGuardAnalyzer.Formatting;

// Create a registry with the library's built‑in formatters.
var registry = FormatterRegistry.CreateWithDefaults();

// Register a custom JSON formatter for the "json" format.
registry.Register(new CustomJsonFormatter());

// Retrieve a formatter for CSV output.
if (registry.TryGetFormatter("csv", out var csvFormatter))
{
    var output = csvFormatter.Format(data);
}
else
{
    // Handle missing formatter.
}
```

### Example 2: Safe lookup with fallback and throwing lookup
```csharp
using RoslynGuardAnalyzer.Formatting;

var registry = FormatterRegistry.CreateWithDefaults();

string requestedFormat = GetUserRequestedFormat(); // e.g., "xml"

// Safe lookup – returns null if unsupported.
IOutputFormatter? formatter = registry.GetFormatter(requestedFormat);
if (formatter == null)
{
    formatter = registry.GetFormatterOrThrow("txt"); // fallback to plain text
}

string result = formatter.Format(someObject);
```

## Notes

- The registry is **not thread‑safe** for concurrent modifications. If multiple threads may call `Register` simultaneously, external synchronization is required. Lookup methods (`GetFormatter`, `IsFormatSupported`, `GetFormatterOrThrow`, `GetSupportedFormats`) are safe to invoke concurrently **provided** the registry is not being modified at the same time.
- Registering a formatter for a format that already exists will cause `Register` to throw; to replace an existing formatter, first remove the old entry (if such a method exists) or recreate the registry.
- `GetFormatterOrThrow` is intended for scenarios where the absence of a formatter is considered a programming error; otherwise, prefer `GetFormatter` and handle the `null` result.
- The collection returned by `GetSupportedFormats` reflects the state of the registry at the moment of the call; subsequent registrations will not affect the already‑returned enumeration. Iterating the collection while the registry is being modified may lead to undefined behavior.
