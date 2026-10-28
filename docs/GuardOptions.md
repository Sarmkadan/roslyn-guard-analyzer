# GuardOptions

Namespace: `RoslynGuardAnalyzer.Rules`

Validated options for the guard analyzers. Values are read from `.editorconfig` (or any other analyzer config source the compiler exposes) under the `guard.*` keys, validated once per compilation, and reported through diagnostic `GRD001` when invalid.

## Keys

| Key | Type | Default | Consumer | Description |
|-----|------|---------|----------|-------------|
| `guard.null_check.exclude_private` | `true` / `false` | `false` | none yet | Excludes private members from null-check analysis. Validated and exposed, but no Roslyn null-check analyzer consumes it yet. |
| `guard.bounds_check.max_depth` | integer, `1` to `1000` | `1000` | `BoundsCheckAnalyzer` (`BND001`) | Number of enclosing syntax levels searched for a preceding length/bounds guard. The default is the maximum, so the search is effectively unlimited. |
| `guard.ignored_namespaces` | comma-separated namespaces | empty | `BoundsCheckAnalyzer` (`BND001`) | Code whose containing namespace is listed, or is a sub-namespace of a listed one, is not analyzed. An empty value clears the list. |
| `guard.bounds_check.allowed_indexer_types` | comma-separated type names, or `[]` | `[], System.Collections.Generic.List, System.Collections.Generic.IList, System.Span, System.ReadOnlySpan, System.Memory, System.ReadOnlyMemory` | `BoundsCheckAnalyzer` (`BND001`) | Types whose element access is checked. Each entry is a namespace-qualified name without type arguments, such as `System.String`; `[]` stands for arrays. The list must not be empty. |
| `guard.bounds_check.check_spans` | `true` / `false` | `true` | `BoundsCheckAnalyzer` (`BND001`) | Whether `Span<T>` and `ReadOnlySpan<T>` are checked. A span type is checked only when it is also in `allowed_indexer_types`. |
| `guard.bounds_check.severity` | `silent`, `suggestion`, `warning`, `error` | `warning` | `BoundsCheckAnalyzer` (`BND001`) | Severity that `BND001` is reported with: `silent` maps to hidden, `suggestion` to info. Compiler `dotnet_diagnostic.BND001.severity` settings still apply on top of this value. |

Example `.editorconfig`:

```ini
root = true

[*.cs]
guard.bounds_check.max_depth = 20
guard.ignored_namespaces = App.Generated, Tests
guard.bounds_check.allowed_indexer_types = [], System.Collections.Generic.List, System.String
guard.bounds_check.check_spans = false
guard.bounds_check.severity = error
```

## Validation rules

- **Booleans** accept `true` or `false`, case-insensitively. Anything else is invalid.
- **Integers** are plain decimal digits (no sign, no separators, no whitespace inside the value) within the inclusive range `1` to `1000`.
- **Namespace lists** are split on `,`. Each entry is trimmed, must not be empty, and must be a dot-separated sequence of identifiers. An identifier starts with a letter or `_` and continues with letters, digits or `_`. Duplicates are removed and the configured order is kept. A `@` prefix is not accepted.
- **Type lists** (`guard.bounds_check.allowed_indexer_types`) use the same splitting, trimming and de-duplication as namespace lists. Each entry is either `[]` or a namespace-qualified name whose segments are identifiers, such as `System.String`. Names with type arguments (`List<int>`) are rejected. An empty value is invalid, because it would disable the check for every type.
- **Severity** (`guard.bounds_check.severity`) accepts `silent`, `suggestion`, `warning` or `error`, case-insensitively.
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

The descriptor is `DiagnosticDescriptors.GuardConfigurationRule`. It is reported once per distinct error per compilation, at compilation end.

## Analyzer integration

`BoundsCheckAnalyzer` parses the options once per distinct `.editorconfig` section during compilation start and caches the result. Each element access then looks up its section's cached options by syntax tree, so per-directory sections apply to the files they cover. Roslyn only accepts diagnostics at compilation end, so `GRD001` is reported there; identical errors from several sections are reported once.

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
