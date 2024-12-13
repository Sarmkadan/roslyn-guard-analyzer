# SuppressionManagerExtensions

Provides extension methods for managing source‑generated suppressions in the Roslyn Guard Analyzer. These methods operate on a `SuppressionManager` instance to add, query, remove, and clean up suppressions associated with diagnostic rules.

## API

### AddSuppression
Adds a new suppression to the manager.

- **Purpose:** Registers a suppression for a specific diagnostic descriptor at a given source location, enabling the analyzer to ignore matching diagnostics.
- **Parameters:**  
  - `this SuppressionManager manager` – The manager to which the suppression is added.  
  - `SuppressionDescriptor descriptor` – Describes the diagnostic rule being suppressed.  
  - `Location location` – The source location where the suppression applies.
- **Return value:** A `SuppressionRecord` representing the added suppression; can be used later to reference or remove this specific entry.
- **Exceptions:**  
  - `ArgumentNullException` if `manager`, `descriptor`, or `location` is `null`.  
  - `InvalidOperationException` if the suppression cannot be added because the manager is in a disposed state.

### RemoveSuppressionsByRuleId
Removes all suppressions associated with a given rule identifier.

- **Purpose:** Clears suppressions for a specific diagnostic rule, allowing those diagnostics to be reported again.
- **Parameters:**  
  - `this SuppressionManager manager` – The manager from which suppressions are removed.  
  - `string ruleId` – The identifier of the diagnostic rule whose suppressions should be removed.
- **Return value:** The number of suppressions that were removed.
- **Exceptions:**  
  - `ArgumentNullException` if `manager` or `ruleId` is `null`.  
  - `ArgumentException` if `ruleId` is empty or whitespace.

### HasAnySuppressed
Determines whether the manager currently contains any suppressions.

- **Purpose:** Provides a quick check for the presence of any active suppressions.
- **Parameters:**  
  - `this SuppressionManager manager` – The manager to inspect.
- **Return value:** `true` if at least one suppression is present; otherwise `false`.
- **Exceptions:**  
  - `ArgumentNullException` if `manager` is `null`.

### GetSuppressionCount
Retrieves the total number of suppressions stored in the manager.

- **Purpose:** Useful for diagnostics or logging to know how many suppressions are active.
- **Parameters:**  
  - `this SuppressionManager manager` – The manager to query.
- **Return value:** An `int` indicating the count of suppressions.
- **Exceptions:**  
  - `ArgumentNullException` if `manager` is `null`.

### ExportActiveSuppressions
Returns a read‑only list of all currently active suppressions.

- **Purpose:** Allows callers to inspect or serialize the current suppression set.
- **Parameters:**  
  - `this SuppressionManager manager` – The manager whose suppressions are exported.
- **Return value:** An `IReadOnlyList<SuppressionRecord>` containing all active suppressions. The list is a snapshot; subsequent changes to the manager do not affect the returned list.
- **Exceptions:**  
  - `ArgumentNullException` if `manager` is `null`.

### CleanupExpiredSuppressions
Removes suppressions that have passed their expiration timestamp.

- **Purpose:** Periodically purges stale suppressions so they no longer affect analysis.
- **Parameters:**  
  - `this SuppressionManager manager` – The manager to clean up.
- **Return value:** The number of suppressions removed as expired and removed.
- **Exceptions:**  
  - `ArgumentNullException` if `manager` is `null`.

### HasActiveSuppressionsForRule
Checks whether any active suppression exists for a specific rule identifier.

- **Purpose:** Enables rule‑specific suppression checks without enumerating all suppressions.
- **Parameters:**  
  - `this SuppressionManager manager` – The manager to inspect.  
  - `string ruleId` – The identifier of the diagnostic rule to check.
- **Return value:** `true` if at least one active suppression matches `ruleId`; otherwise `false`.
- **Exceptions:**  
  - `ArgumentNullException` if `manager` or `ruleId` is `null`.  
  - `ArgumentException` if `ruleId` is empty or whitespace.

## Usage

```csharp
using Microsoft.CodeAnalysis;
using RoslynGuardAnalyzer.Suppression;

// Assume `manager` is an existing SuppressionManager instance.
var descriptor = new SuppressionRuleDescriptor("CA1234", "Do not use deprecated API");
var location   = someSyntaxNode.GetLocation();

// Add a suppression and keep the record for possible later removal.
SuppressionRecord record = SuppressionManagerExtensions.AddSuppression(manager, descriptor, location);

// Quick check: are there any suppressions at all?
bool any = SuppressionManagerExtensions.HasAnySuppressed(manager); // true
```

```csharp
using Microsoft.CodeAnalysis;
using RoslynGuardAnalyzer.Suppression;

// Remove all suppressions for a given rule and report how many were cleared.
int removed = SuppressionManagerExtensions.RemoveSuppressionsByRuleId(manager, "CA1234");
// removed now holds the count of suppressions that were deleted.

// Export the remaining active suppressions for logging or inspection.
IReadOnlyList<SuppressionRecord> active = SuppressionManagerExtensions.ExportActiveSuppressions(manager);
foreach (var r in active)
{
    Console.WriteLine($"Active suppression: {r.Descriptor.Id} at {r.Location}");
}

// Determine whether any suppressions remain for a specific rule.
bool stillPresent = SuppressionManagerExtensions.HasActiveSuppressionsForRule(manager, "CA5678");
```

## Notes

- All extension methods operate on the supplied `SuppressionManager` instance; they do not modify static state.
- If the same `(descriptor, location)` pair is added multiple times, each call creates a distinct `SuppressionRecord`. Duplicate suppressions are allowed and counted separately.
- `RemoveSuppressionsByRuleId` and `CleanupExpiredSuppressions` return the number of items removed; a return value of `0` indicates nothing matched the criteria.
- The methods are safe to invoke concurrently on **different** `SuppressionManager` instances. Concurrent access to the **same** instance requires external synchronization because the underlying `SuppressionManager` is not thread‑safe.
- Passing `null` for any argument results in an `ArgumentNullException`. Empty or whitespace rule identifiers trigger an `ArgumentException`.
- The snapshot returned by `ExportActiveSuppressions` reflects the state of the manager at the moment of the call; subsequent additions or removals do not alter the returned list.
