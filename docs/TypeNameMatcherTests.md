# TypeNameMatcherTests

Unit tests for the `TypeNameMatcher` class, verifying behavior of type name pattern matching including exact matches, wildcards, and fully-qualified name handling.

## API

### `Matches_ExactTypeName_ReturnsTrueCaseInsensitively`

Verifies that an exact type name match succeeds regardless of case differences. The test asserts that the matcher returns `true` when comparing a pattern and input that differ only in casing.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No documented exceptions

---

### `Matches_StarWildcardSuffix_MatchesAllTypesWithGivenPrefix`

Ensures that a wildcard suffix pattern using `*` matches all type names beginning with the specified prefix. The test validates that the matcher correctly interprets and applies the suffix wildcard.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No documented exceptions

---

### `MatchesFullyQualified_WithNamespaceAndTypeName_CombinesBeforeMatching`

Confirms that a fully-qualified type name (including namespace) is correctly combined into a single string before pattern matching occurs. The test checks that the matcher treats the entire qualified name as a single unit during comparison.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No documented exceptions

---
### `NamespaceMatcher_WildcardSegment_MatchesOneOrMoreIntermediateParts`

Validates that a wildcard segment within a namespace pattern matches one or more intermediate parts of a namespace. The test ensures correct behavior when traversing hierarchical namespace structures with wildcards.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No documented exceptions

---

## Usage
