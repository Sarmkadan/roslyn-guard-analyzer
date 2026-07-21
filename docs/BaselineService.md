# BaselineService

The `BaselineService` class provides core functionality for persisting, comparing, and constructing baseline data used by the Roslyn Guard Analyzer. It enables loading existing baselines from disk, saving updated baselines, filtering out violations that have already been accepted, and creating baseline objects from collections of rule violations.

## API

### BaselineService()
Initializes a new instance of the `BaselineService`. The constructor takes no parameters and prepares the service for use. It does not throw exceptions under normal conditions.

### Task<Baseline?> LoadBaselineAsync(string filePath, CancellationToken cancellationToken = default)
Attempts to load a `Baseline` from the specified file path. If the file exists and contains valid baseline data, the method returns the deserialized `Baseline` instance; otherwise it returns `null`. The operation is performed asynchronously.  
- **Throws** `ArgumentNullException` if `filePath` is `null`.  
- **Throws** `ArgumentException` if `filePath` is empty or consists only of whitespace.  
- **Throws** `IOException` for I/O‑related errors (e.g., disk failure).  
- **Throws** `UnauthorizedAccessException` if the caller lacks permission to read the file.

### Task SaveBaselineAsync(Baseline baseline, string filePath, CancellationToken cancellationToken = default)
Serializes the supplied `Baseline` to the given file path asynchronously.  
- **Throws** `ArgumentNullException` if `baseline` is `null`.  
- **Throws** `ArgumentNullException` if `filePath` is `null`.  
- **Throws** `ArgumentException` if `filePath` is empty or whitespace.  
- **Throws** `IOException` for I/O‑related errors during write.  
- **Throws** `UnauthorizedAccessException` if the caller lacks permission to write to the location.  
The returned `Task` completes when the write operation finishes.

### List<RuleViolation> FilterNewViolations(IEnumerable<RuleViolation> currentViolations, Baseline baseline)
Compares the `currentViolations` collection against the provided `baseline` and returns a list containing only those violations not present in the baseline. Returns an empty list when there are no new violations.  
- **Throws** `ArgumentNullException` if `currentViolations` is `null`.  
- **Throws** `ArgumentNullException` if `baseline` is `null`.  
The method does not modify the input collections.

### Baseline CreateBaseline(IEnumerable<RuleViolation> violations)
Creates a new `Baseline` instance populated with the supplied `violations`. If `violations` is empty, the resulting baseline contains no entries.  
- **Throws** `ArgumentNullException` if `violations` is `null`.

### Baseline CreateBaseline(IEnumerable<RuleViolation> violations, string projectName)
Creates a new `Baseline` instance populated with the supplied `violations` and associates it with the specified `projectName`.  
- **Throws** `ArgumentNullException` if `violations` is `null`.  
- **Throws** `ArgumentNullException` if `projectName` is `null`.  
- **Throws** `ArgumentException` if `projectName` is empty or consists only of whitespace.

## Usage

```csharp
using RoslynGuardAnalyzer.Services;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class AnalyzerRunner
{
    private readonly BaselineService _baselineService = new BaselineService();

    public async Task RunAnalysisAsync(string baselinePath)
    {
        // Load existing baseline if any
        Baseline? baseline = await _baselineService.LoadBaselineAsync(baselinePath);

        // Obtain current violations from the analyzer (implementation omitted)
        IEnumerable<RuleViolation> currentViolations = GetCurrentViolations();

        // Filter out violations already present in the baseline
        List<RuleViolation> newViolations = _baselineService.FilterNewViolations(
            currentViolations,
            baseline ?? new Baseline());

        // If new violations are found, update the baseline
        if (newViolations.Any())
        {
            Baseline updated = _baselineService.CreateBaseline(currentViolations);
            await _baselineService.SaveBaselineAsync(updated, baselinePath);
        }
    }

    private IEnumerable<RuleViolation> GetCurrentViolations()
    {
        // Placeholder: replace with actual violation retrieval logic
        return new List<RuleViolation>();
    }
}
```

```csharp
using RoslynGuardAnalyzer.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BaselineUpdater
{
    private readonly BaselineService _service = new BaselineService();

    public async Task UpdateBaselineWithProjectNameAsync(
        string filePath,
        IEnumerable<RuleViolation> violations,
        string projectName)
    {
        // Create a baseline that includes project metadata
        Baseline baseline = _service.CreateBaseline(violations, projectName);
        await _service.SaveBaselineAsync(baseline, filePath);
    }
}
```

## Notes

- The service does not maintain mutable state across calls; its methods are thread‑safe for concurrent invocations. However, concurrent writes to the same file path may cause race conditions; external synchronization is required when multiple threads save to the same location.
- `LoadBaselineAsync` returns `null` when the target file does not exist. Callers should treat this as an empty baseline rather than an error.
- `FilterNewViolations` performs a linear comparison; its runtime grows with the size of the input collections. For very large datasets consider pre‑filtering or using hash‑based look‑ups.
- The `CreateBaseline` overloads do not clone the supplied `IEnumerable<RuleViolation>`. The resulting baseline holds references to the violation objects. If the source collection is modified after baseline creation and the violations are mutable, the baseline may reflect those changes. To avoid this, pass an immutable or snapshot collection.
- All asynchronous methods accept an optional `CancellationToken` to support cooperative cancellation. Ignoring the token may cause the operation to run to completion even if a cancellation request has been issued.
- The service performs no internal logging or diagnostics; callers should handle exceptions and log as appropriate for their application.
