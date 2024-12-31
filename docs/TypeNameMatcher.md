# TypeNameMatcher

A utility class for matching type names against patterns, supporting both simple name and fully-qualified name matching. It provides flexible filtering and matching capabilities for type names in reflection and analysis scenarios.

## API

### `public TypeNameMatcher`

Initializes a new instance of the `TypeNameMatcher` class with default settings.

### `public bool Matches(string typeName)`

Determines whether the specified type name matches the configured pattern.

- **Parameters**
  - `typeName`: The type name to match against the pattern.
- **Returns**
  - `true` if the type name matches the pattern; otherwise, `false`.
- **Remarks**
  - The matching behavior depends on whether `MatchesFullyQualified` is set. If `true`, the entire fully-qualified name must match; otherwise, only the simple type name is considered.

### `public bool MatchesFullyQualified { get; set; }`

Gets or sets a value indicating whether the matcher should match against the fully-qualified type name instead of just the simple type name.

- **Default Value**
  - `false`
- **Remarks**
  - When `true`, the `Matches` method compares the entire fully-qualified name against the configured pattern.

### `public IEnumerable<string> Filter { get; }`

Gets the collection of type name patterns used for filtering.

- **Remarks**
  - Patterns are case-sensitive and support simple wildcards (e.g., `*`, `?`) for flexible matching.

### `public static bool MatchesAny(string typeName, IEnumerable<string> patterns)`

Determines whether the specified type name matches any of the given patterns.

- **Parameters**
  - `typeName`: The type name to match against the patterns.
  - `patterns`: The collection of patterns to match against.
- **Returns**
  - `true` if the type name matches any of the patterns; otherwise, `false`.
- **Remarks**
  - Patterns are evaluated in order, and the first match determines the result.

### `public override string ToString()`

Returns a string representation of the current matcher, including its patterns and matching mode.

- **Returns**
  - A string describing the matcher's configuration.

### `public NamespaceMatcher Namespace { get; }`

Gets the namespace matcher associated with this type name matcher.

- **Remarks**
  - The namespace matcher can be used to further refine matching by namespace.

### `public bool Matches(string typeName)`

Determines whether the specified type name matches the configured pattern, considering the associated namespace matcher.

- **Parameters**
  - `typeName`: The type name to match against the pattern.
- **Returns**
  - `true` if the type name and namespace match the configured patterns; otherwise, `false`.

### `public override string ToString()`

Returns a string representation of the namespace matcher, including its patterns.

- **Returns**
  - A string describing the namespace matcher's configuration.

## Usage

### Example 1: Basic Type Name Matching
