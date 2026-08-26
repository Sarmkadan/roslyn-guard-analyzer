## BaselineService

The BaselineService class is responsible for managing baseline files that store known violations. It provides methods to load and save baselines, as well as filter new violations not present in the baseline.

### Example usage:

```csharp
public async Task<Baseline?> LoadBaselineAsync(string filePath)
public async Task SaveBaselineAsync(Baseline baseline, string filePath)
public List<RuleViolation> FilterNewViolations(List<RuleViolation> violations, Baseline? baseline, TimeSpan baselineExpiration = default)
public Baseline CreateBaseline(AnalysisResult result)
public Baseline CreateBaseline(string projectName, List<RuleViolation> violations)
```

These methods can be used to manage baselines and filter new violations in a .NET application.