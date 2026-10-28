# GuardConfigurationError

Namespace: `RoslynGuardAnalyzer.Rules`

```csharp
public sealed record GuardConfigurationError(string Key, string Value, string Reason)
```

Describes one invalid `guard.*` configuration value.

| Member | Description |
|--------|-------------|
| `Key` | The configuration key, for example `guard.bounds_check.max_depth`. |
| `Value` | The raw value as written in configuration. |
| `Reason` | A short explanation of the expected form, for example `expected an integer from 1 to 1000`. |

Produced by `GuardOptions.Parse`. The analyzer reports each one as diagnostic `GRD001`. See [GuardOptions](GuardOptions.md).
