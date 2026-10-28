# GuardOptionsParseResult

Namespace: `RoslynGuardAnalyzer.Rules`

```csharp
public sealed record GuardOptionsParseResult(GuardOptions Options, ImmutableArray<GuardConfigurationError> Errors)
```

The outcome of `GuardOptions.Parse`.

| Member | Description |
|--------|-------------|
| `Options` | The parsed options. Invalid keys carry their default value. |
| `Errors` | One `GuardConfigurationError` per invalid value, in key order. Empty when every value is valid. |
| `HasErrors` | `true` when `Errors` is not empty. |

See [GuardOptions](GuardOptions.md) for the keys and validation rules.
