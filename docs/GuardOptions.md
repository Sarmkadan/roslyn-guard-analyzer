# GuardOptions

Namespace: `RoslynGuardAnalyzer.Rules`

Validated options for the guard analyzers. Values are read from `.editorconfig` (or any other analyzer config source the compiler exposes) under the `guard.*` keys, validated once per compilation, and reported through diagnostic `GRD001` when invalid.

## Keys

| Key | Type | Default | Consumer | Description |
|-----|------|---------|----------|-------------|
| `guard.null_check.exclude_private` | `true` / `false` | `false` | none yet | Excludes private members from null-check analysis. Validated and exposed, but no Roslyn null-check analyzer consumes it yet. |
| `guard.bounds_check.max_depth` | integer, `1` to `1000` | `1000` | `BoundsCheckAnalyzer` (`BND001`) | Number of enclosing syntax levels searched for a preceding length/bounds guard. The default is the maximum, so the search is effectively unlimited. |
| `guard.ignored_namespaces` | comma-separated namespaces | empty | `BoundsCheckAnalyzer` (`BND001`) | Code whose containing namespace is listed, or is a sub-namespace of a listed one, is not analyzed. An empty value clears the list. |

Example `.editorconfig`:

```ini
root = true

[*.cs]
guard.bounds_check.max_depth = 20
guard.ignored_namespaces = App.Generated, Tests
```

## Validation rules

- **Booleans** accept `true` or `false`, case-insensitively. Anything else is invalid.
- **Integers** are plain decimal digits (no sign, no separators, no whitespace inside the value) within the inclusive range `1` to `1000`.
- **Namespace lists** are split on `,`. Each entry is trimmed, must not be empty, and must be a dot-separated sequence of identifiers. An identifier starts with a letter or `_` and continues with letters, digits or `_`. Duplicates are removed and the configured order is kept. A `@` prefix is not accepted.
- **Unknown `guard.*` keys** are ignored. Roslyn does not expose the full key set of a config file, so typos in key names are not reported.

## Error handling

`GuardOptions.Parse(IReadOnlyDictionary<string, string>)` returns a `GuardOptionsParseResult` containing the resulting `Options` and one `GuardConfigurationError` (key, raw value, reason) per invalid value. An invalid key uses its default value, so analysis still runs. The analyzer reports each error as diagnostic `GRD001` (see below).

Parsing never throws for bad configuration values. It throws `ArgumentNullException` only when the dictionary itself is `null`.

## Diagnostic GRD001

| Property | Value |
|----------|-------|
| ID | `GRD001` |
| Title | Invalid guard analyzer configuration |
| Category | `RoslynGuardAnalyzer.Configuration` |
| Default severity | Warning |
| Location | None (the configuration lives in `.editorconfig`, not in source) |
| Message | `Invalid value '{value}' for '{key}': {reason}; the default is used instead` |

The descriptor is `DiagnosticDescriptors.GuardConfigurationRule`. It is reported once per distinct configuration section per compilation, at compilation end.

## Analyzer integration

`BoundsCheckAnalyzer` reads the options during compilation start and reports `GRD001` for invalid values. It then looks up the options for each element access by syntax tree, so per-directory `.editorconfig` sections apply to the files they cover.

## Example

```csharp
var values = new Dictionary<string, string>
{
    ["guard.bounds_check.max_depth"] = "0",
};

var result = GuardOptions.Parse(values);

result.HasErrors;                          // true
result.Errors[0].Key;                      // "guard.bounds_check.max_depth"
result.Options.BoundsCheckMaxDepth;        // 1000 (default)
```
