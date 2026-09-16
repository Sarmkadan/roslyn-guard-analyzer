# Rule violation suppressions

`SuppressionManager` keeps a set of `SuppressionRecord` objects and uses them to
remove accepted rule violations from a result set. Suppressions are held in
memory and can optionally be saved to, or loaded from, a JSON file.

## What a suppression record describes

A `SuppressionRecord` has the following fields:

| Field | Meaning |
| --- | --- |
| `Id` | Unique key used by the manager. It defaults to a GUID without separators. |
| `RuleId` | Rule to suppress. Matching is case-insensitive. |
| `TargetFile` | Optional file scope. A null, empty, or whitespace value matches every file. |
| `TargetElement` | Optional element scope. A null, empty, or whitespace value matches every element. |
| `Justification` | Audit information explaining why the suppression exists. It does not affect matching. |
| `CreatedAt` | Creation time, defaulting to the current UTC time. It controls listing and persistence order. |
| `ExpiresAt` | Optional UTC expiration time. A record stops matching at this time. |
| `Author` | Audit information identifying the creator. It does not affect matching. |
| `IsActive` | Enables or disables the record without removing it. The default is `true`. |

The manager does not validate these fields when `AddSuppression` is called.
Callers that require validation must do so separately.

## Matching a violation

`SuppressionRecord.Matches` applies each configured scope as an AND condition. A
violation is suppressed only when all of the following are true:

1. `IsActive` is `true`.
2. `RuleId` equals the violation's `RuleId`, ignoring case.
3. `ExpiresAt` is absent or later than `DateTime.UtcNow`.
4. If `TargetFile` is set, it is equivalent to the violation's `FilePath` after
   case-insensitive path normalization. This normalization converts backslashes
   to forward slashes, removes some redundant path segments and separators, and
   ignores case.
5. If `TargetElement` is set, it equals the violation's element metadata,
   ignoring case. The metadata key `ElementName` is checked first; when that key
   is absent, `TargetElement` is checked as a fallback.

Omitting both target fields creates a rule-wide suppression. Setting only
`TargetFile` scopes it to one file. Setting both fields scopes it to one named
element in that file. Line and column numbers, messages, severity, justification,
and author are not considered during matching.

`SuppressionManager.IsSuppressed` returns `true` when any stored record matches.
There is no priority or conflict-resolution step: one matching record is enough.
`FilterSuppressed` evaluates each supplied violation in its original order and
returns a read-only snapshot containing only violations for which
`IsSuppressed` is `false`.

## Adding, replacing, and removing records

The manager stores records in a dictionary keyed by `Id`, using a
case-insensitive key comparison. `AddSuppression` therefore adds a new record or
replaces the existing record whose ID differs only by case. `RemoveSuppression`
also looks up IDs case-insensitively and returns whether a record was removed; an
empty or whitespace ID simply returns `false`.

`GetSuppressions()` returns a read-only snapshot ordered by `CreatedAt`.
Supplying a rule ID filters the snapshot case-insensitively; a null, empty, or
whitespace rule ID means no filter. The manager protects its in-memory dictionary
with a lock, and saving takes a snapshot under that lock before performing file
I/O.

## Example

```csharp
using Microsoft.Extensions.Logging;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Suppressions;

ILogger<SuppressionManager> logger = loggerFactory.CreateLogger<SuppressionManager>();
var suppressions = new SuppressionManager(logger);

suppressions.AddSuppression(new SuppressionRecord
{
    RuleId = "RG0042",
    TargetFile = "src/Services/LegacyService.cs",
    TargetElement = "LoadLegacyData",
    Justification = "Retained until the legacy endpoint is retired.",
    Author = "architecture-team",
    ExpiresAt = DateTime.UtcNow.AddMonths(3)
});

var violation = new RuleViolation
{
    RuleId = "RG0042",
    FilePath = "src/Services/LegacyService.cs"
};
violation.AddMetadata("ElementName", "LoadLegacyData");

bool hidden = suppressions.IsSuppressed(violation); // true
IReadOnlyList<RuleViolation> reportable =
    suppressions.FilterSuppressed(new[] { violation }); // empty
```

The example keeps the record only in memory. Call `SaveAsync` to make it durable.

## JSON persistence

`SaveAsync(filePath)` writes all current records as an indented JSON array,
ordered by `CreatedAt`. It creates the parent directory when necessary. A save
does not remove inactive or expired records.

`LoadAsync(filePath)` reads that array, discards records whose `ExpiresAt` is at
or before the current UTC time, and replaces the in-memory dictionary with the
remaining records. Inactive records are retained, although they do not match.
Duplicate IDs in the file resolve to the last record encountered. Loading a
missing file leaves the current in-memory records unchanged.

```json
[
  {
    "Id": "5d46da7dd33c44d88d17c54be76b67d0",
    "RuleId": "RG0042",
    "TargetFile": "src/Services/LegacyService.cs",
    "TargetElement": "LoadLegacyData",
    "Justification": "Retained until the legacy endpoint is retired.",
    "CreatedAt": "2026-09-16T10:00:00Z",
    "ExpiresAt": "2026-12-16T10:00:00Z",
    "Author": "architecture-team",
    "IsActive": true
  }
]
```

Null or whitespace file paths are rejected before file access. Other failures
during saving or loading—including invalid JSON, I/O errors, and cancellation
raised by the file operation—are logged and not rethrown. If loading fails before
replacement, the current in-memory records remain unchanged. Callers should use
the logs to detect persistence failures rather than relying on an exception.
