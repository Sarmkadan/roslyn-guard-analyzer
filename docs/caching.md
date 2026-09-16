# Caching strategy and cache keys

The types in `src/RoslynGuardAnalyzer/Caching` provide an optional, process-local
cache and deterministic key helpers. They are not wired into the analyzer's
default execution path; a caller creates a `CacheService`, chooses which inputs
define freshness, generates keys with `CacheKeyGenerator`, and explicitly stores
or invalidates values.

## Storage and expiration

`CacheService` stores entries in an in-memory `Dictionary<string, object>`. Each
entry wraps the value together with an absolute UTC expiration timestamp. The
constructor accepts a default time-to-live, which is one hour when omitted.
`Set(key, value)` uses that default, while `Set(key, value, expiration)` adds the
given duration to `DateTime.UtcNow`. Setting an existing key replaces its value,
type, and expiration.

Expiration is checked on a typed read. `TryGet<T>` returns `false` for a missing,
expired, or differently typed entry, and removes an entry when it observes that
the correctly typed entry has expired. `Get<T>` turns the same miss into a
`KeyNotFoundException`; `GetOrDefault<T>` returns the supplied fallback. The type
argument must match the type used by `Set<T>` exactly: storing a `Derived` value
and requesting `Base` does not produce a hit.

The cache has no size limit or eviction policy other than expiration and explicit
removal. `Count` reports all dictionary entries, including expired entries that
have not been removed. `Remove` deletes one exact key, `Clear` deletes everything,
and `InvalidateByPattern` deletes every key that starts with its argument using an
ordinal, case-insensitive comparison. Despite the method name, this is a literal
prefix comparison, not wildcard or regular-expression matching.

`GetOrComputeAsync<T>` reads first and, on a miss, awaits the supplied factory and
stores its result with the default expiration. Exceptions from the factory are
propagated and no value is cached. There is no per-key locking or request
coalescing, so concurrent misses can run the factory more than once. More
generally, `CacheService` is not thread-safe; callers sharing an instance across
threads must synchronize access.

### Expiration maintenance caveat

`RemoveExpired` and therefore `GetKeys` currently filter candidates through an
`IReflect` check before inspecting their `ExpiresAt` property. The private cache
entry wrappers do not satisfy that check, so these methods do not currently purge
ordinary entries in practice. A correctly typed `TryGet<T>` remains the reliable
lazy-expiration path. `Contains` is implemented as `TryGet<object>` and therefore
normally returns `false` for values stored as a more specific generic type. These
details describe the current behavior and are important when choosing maintenance
or existence checks.

## Key generation

`CacheKeyGenerator` hashes strings with SHA-256 over their UTF-8 bytes, then uses
only the first eight hash bytes. The result is 16 lowercase hexadecimal
characters. This keeps keys compact and deterministic, but the truncation means
the helpers do not provide the full collision resistance of SHA-256. Inputs are
used exactly as supplied: paths, names, and casing are not normalized.

The generated formats are:

| Helper | Key format |
| --- | --- |
| `GenerateProjectAnalysisKey(projectPath, configHash)` | `analysis_project_{pathHash}` plus `_{configHash}` when the optional value is nonempty |
| `GenerateFileAnalysisKey(filePath, fileContentHash)` | `analysis_file_{pathHash}_{fileContentHash}`; the final underscore remains when the optional hash is null |
| `GenerateResultKey(analysisId)` | `result_{analysisId}` |
| `GenerateRuleExecutionKey(ruleName, targetName)` | `rule_exec_{ruleHash}_{targetHash}` |
| `GenerateCodeElementKey(fullTypeName, memberName)` | `element_{typeHash}` plus `_{memberHash}` when the member name is nonempty |
| `CreateCompositeKey(components)` | hash of the components joined with `|` |
| `GeneratePatternKey(prefix)` | `{prefix}_*` |

`ComputeFileHash` applies the same truncated SHA-256 format to file bytes. It
returns the sentinel `not_found` when the path does not exist and `error` for any
exception while checking or reading the file. Callers that use these values in
keys should decide whether caching failures under a shared sentinel is
appropriate.

Most required string inputs are rejected when null or empty with
`ArgumentException.ThrowIfNullOrEmpty`; whitespace-only strings are accepted.
`CreateCompositeKey` rejects a null or empty component array, but does not reject
null, empty, whitespace, or delimiter-containing elements before joining them.
`GeneratePatternKey` performs no validation.

`GeneratePatternKey` produces a display-style wildcard suffix, whereas
`InvalidateByPattern` performs a literal prefix comparison and does not interpret
`*`. Consequently, pass the actual common prefix to invalidation—for example,
`cache.InvalidateByPattern("analysis_file_")`—rather than passing the generated
`"analysis_file_*"` value.

## Recommended composition

Include every input whose change should force recomputation. For file analysis,
combine the path-based key with a content hash; for project analysis, supply a
stable hash or version for the effective configuration. The generator does not
read project configuration or file contents automatically.

```csharp
using RoslynGuardAnalyzer.Caching;

var cache = new CacheService(TimeSpan.FromMinutes(30));
var filePath = "src/Program.cs";
var contentHash = CacheKeyGenerator.ComputeFileHash(filePath);
var key = CacheKeyGenerator.GenerateFileAnalysisKey(filePath, contentHash);

var result = await cache.GetOrComputeAsync(key, async () =>
    await AnalyzeFileAsync(filePath));

// Invalidate all file-analysis entries. Invalidation expects a literal prefix.
cache.InvalidateByPattern("analysis_file_");
```

Keys containing optional caller-provided fragments, such as `configHash` and
`analysisId`, are not escaped or hashed by those particular helpers. Use
`ComputeHash` first when a fragment can be long, sensitive, or contain arbitrary
text. The generated hashes are suitable for cache identity, not for password
storage, authentication, signatures, or other security boundaries.
