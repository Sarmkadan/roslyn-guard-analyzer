# SuppressionManager

The `SuppressionManager` class serves as the central orchestrator for managing rule violation suppressions within the Roslyn Guard Analyzer ecosystem. It provides an in-memory registry for tracking which diagnostics have been explicitly suppressed, supports persistence operations to save and load suppression states asynchronously, and offers utility methods to filter active violations based on the current suppression configuration. This component ensures that known false positives or accepted risks are consistently ignored during analysis runs while maintaining a durable record of these decisions.

## API

### `public SuppressionManager()`
Initializes a new instance of the `SuppressionManager` class. The constructor creates an empty internal collection of suppressions, ready to accept records via `AddSuppression` or populated via `LoadAsync`.

### `public void AddSuppression(SuppressionRecord record)`
Registers a new suppression record in the manager.
*   **Parameters**:
    *   `record`: The `SuppressionRecord` instance containing the details of the suppression (e.g., rule ID, target location, justification).
*   **Return Value**: None.
*   **Exceptions**: Throws an exception if `record` is null or if the record violates internal validation constraints (e.g., duplicate key conflicts depending on implementation specifics).

### `public bool RemoveSuppression(SuppressionRecord record)`
Attempts to remove a specific suppression record from the manager.
*   **Parameters**:
    *   `record`: The `SuppressionRecord` to remove. Matching is typically based on the unique identity of the suppression (such as rule ID and target hash).
*   **Return Value**: Returns `true` if the record was found and successfully removed; otherwise, returns `false`.
*   **Exceptions**: Throws an exception if `record` is null.

### `public IReadOnlyList<SuppressionRecord> GetSuppressions()`
Retrieves a read-only snapshot of all currently registered suppressions.
*   **Parameters**: None.
*   **Return Value**: An `IReadOnlyList<SuppressionRecord>` containing all active suppression records. The list reflects the state at the time of the call.
*   **Exceptions**: None.

### `public bool IsSuppressed(RuleViolation violation)`
Determines whether a specific rule violation is currently suppressed by any existing record in the manager.
*   **Parameters**:
    *   `violation`: The `RuleViolation` instance to check against the suppression list.
*   **Return Value**: Returns `true` if a matching suppression exists for the given violation; otherwise, returns `false`.
*   **Exceptions**: Throws an exception if `violation` is null.

### `public IReadOnlyList<RuleViolation> FilterSuppressed(IEnumerable<RuleViolation> violations)`
Filters a collection of rule violations, returning only those that are *not* suppressed.
*   **Parameters**:
    *   `violations`: The enumerable collection of `RuleViolation` objects to filter.
*   **Return Value**: An `IReadOnlyList<RuleViolation>` containing only the violations that returned `false` for `IsSuppressed`.
*   **Exceptions**: Throws an exception if `violations` is null.

### `public async Task SaveAsync(string filePath)`
Asynchronously serializes the current list of suppressions to a file.
*   **Parameters**:
    *   `filePath`: The absolute or relative path to the file where suppression data should be written.
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Exceptions**: Throws `IOException` or `UnauthorizedAccessException` if the file cannot be written due to permissions, disk errors, or invalid paths.

### `public async Task LoadAsync(string filePath)`
Asynchronously reads suppression data from a file and populates the manager.
*   **Parameters**:
    *   `filePath`: The path to the file containing serialized suppression data.
*   **Return Value**: A `Task` representing the asynchronous operation. Existing in-memory suppressions may be cleared or merged depending on implementation strategy upon load.
*   **Exceptions**: Throws `FileNotFoundException` if the path does not exist, or `FormatException`/`SerializationException` if the file content is corrupted or incompatible.

## Usage

### Example 1: Initializing and Filtering Violations
This example demonstrates loading existing suppressions from disk, adding a new dynamic suppression, and filtering a list of detected violations.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoslynGuardAnalyzer;

public class AnalysisRunner
{
    public async Task RunAnalysisAsync(string suppressionFile, IEnumerable<RuleViolation> detectedViolations)
    {
        var manager = new SuppressionManager();

        // Load existing suppressions from disk
        if (System.IO.File.Exists(suppressionFile))
        {
            await manager.LoadAsync(suppressionFile);
        }

        // Dynamically suppress a specific known false positive
        var newSuppression = new SuppressionRecord
        {
            RuleId = "RG0042",
            TargetHash = "a1b2c3d4",
            Reason = "Accepted architectural deviation"
        };
        
        manager.AddSuppression(newSuppression);

        // Filter out suppressed violations before reporting
        var activeViolations = manager.FilterSuppressed(detectedViolations);

        Console.WriteLine($"Total detected: {detectedViolations.Count()}");
        Console.WriteLine($"Active after suppression: {activeViolations.Count}");
        
        // Persist the new suppression state
        await manager.SaveAsync(suppressionFile);
    }
}
```

### Example 2: Manual Suppression Management
This example illustrates checking individual violation status and manually removing a suppression.

```csharp
using System;
using System.Linq;
using RoslynGuardAnalyzer;

public class SuppressionAuditor
{
    public void AuditSuppressions(SuppressionManager manager, RuleViolation specificViolation)
    {
        // Check if a specific violation is suppressed
        if (manager.IsSuppressed(specificViolation))
        {
            Console.WriteLine($"Violation {specificViolation.RuleId} is currently suppressed.");

            // Retrieve all suppressions to find the specific record
            var allSuppressions = manager.GetSuppressions();
            var matchingRecord = allSuppressions.FirstOrDefault(s => 
                s.RuleId == specificViolation.RuleId && 
                s.TargetHash == specificViolation.Hash);

            if (matchingRecord != null)
            {
                // Remove the suppression if the justification is no longer valid
                bool removed = manager.RemoveSuppression(matchingRecord);
                if (removed)
                {
                    Console.WriteLine("Suppression record removed. Violation will now be reported.");
                }
            }
        }
        else
        {
            Console.WriteLine("Violation is active and will be reported.");
        }
    }
}
```

## Notes

*   **Thread Safety**: The public interface of `SuppressionManager` does not imply intrinsic thread safety. Concurrent calls to modification methods (`AddSuppression`, `RemoveSuppression`, `LoadAsync`, `SaveAsync`) while reading (`GetSuppressions`, `IsSuppressed`, `FilterSuppressed`) may result in race conditions or inconsistent snapshots. External synchronization (e.g., `lock` statements) is required when accessing the same instance from multiple threads.
*   **Persistence Overwrites**: The behavior of `LoadAsync` regarding existing in-memory data should be treated as potentially resetting the state. It is recommended to instantiate a new `SuppressionManager` for each load operation or ensure the application logic accounts for state replacement upon loading.
*   **Equality Matching**: The effectiveness of `RemoveSuppression` and `IsSuppressed` relies on the equality implementation of `SuppressionRecord` and `RuleViolation`. Ensure that the properties used for identification (typically Rule ID and Target Location/Hash) are consistent between the recorded suppression and the incoming violation.
*   **File I/O Errors**: As `SaveAsync` and `LoadAsync` perform disk operations, callers must handle potential I/O exceptions. Transient file locks by other processes (e.g., an IDE holding the suppression file open) may cause these operations to fail.
