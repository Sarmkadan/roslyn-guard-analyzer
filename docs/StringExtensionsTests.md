# StringExtensionsTests

This test class contains unit tests for the string extension methods defined in the `StringExtensions` helper within the roslyn‑guard‑analyzer project. Each test verifies a specific transformation or calculation behavior under controlled input conditions.

## API

### ToPascalCase_UnderscoreSeparatedInput_ReturnsPascalCase
- **Purpose**: Verifies that the `ToPascalCase` extension correctly converts an underscore‑separated string to PascalCase.
- **Parameters**: None.
- **Return Value**: `void`.
- **Exceptions**: Throws an exception of type `AssertFailedException` (or the test framework’s equivalent) if the resulting string does not match the expected PascalCase value.

### ToCamelCase_HyphenSeparatedInput_ReturnsCamelCase
- **Purpose**: Verifies that the `ToCamelCase` extension correctly converts a hyphen‑separated string to camelCase.
- **Parameters**: None.
- **Return Value**: `void`.
- **Exceptions**: Throws an exception if the conversion does not produce the expected camelCase string.

### ToSnakeCase_PascalCaseInput_InsertsUnderscoreBeforeUpperCaseTransitions
- **Purpose**: Verifies that the `ToSnakeCase` extension inserts underscores before each uppercase letter (except the first) in a PascalCase input, yielding snake_case output.
- **Parameters**: None.
- **Return Value**: `void`.
- **Exceptions**: Throws an exception if the produced snake_case string does not match the expected result.

### LevenshteinDistance_IdenticalStrings_ReturnsZero
- **Purpose**: Verifies that the `LevenshteinDistance` extension returns zero when comparing two identical strings.
- **Parameters**: None.
- **Return Value**: `void`.
- **Exceptions**: Throws an exception if the distance is not zero for identical inputs.

### CountOccurrences_NonOverlappingSubstring_ReturnsCorrectCount
- **Purpose**: Verifies that the `CountOccurrences` extension correctly counts non‑overlapping occurrences of a substring within a source string.
- **Parameters**: None.
- **Return Value**: `void`.
- **Exceptions**: Throws an exception if the counted value differs from the expected non‑overlapping count.

## Usage

The following examples demonstrate how the extension methods tested by this class are used in production code. The test class itself is not instantiated; it merely exercises the extensions.

```csharp
using RoslynGuardAnalyzer.Extensions; // namespace containing StringExtensions

string input = "hello_world_example";
string pascal = input.ToPascalCase(); // Returns "HelloWorldExample"
```

```csharp
string phrase = "the-quick-brown-fox";
string camel = phrase.ToCamelCase(); // Returns "theQuickBrownFox"
```

## Notes

- **Null handling**: The extension methods are designed to treat a `null` source string as an empty input; they return an appropriate default (e.g., empty string for transformations, zero for distance/count) rather than throwing a `NullReferenceException`. However, passing `null` as the substring argument to `CountOccurrences` results in an `ArgumentNullException`.
- **Empty strings**: All methods handle empty inputs gracefully—transformations return an empty string, `LevenshteinDistance` returns the length of the other string (zero if both are empty), and `CountOccurrences` returns zero.
- **Culture sensitivity**: The case‑conversion extensions operate invariant‑culturally; they do not depend on the current thread’s culture.
- **Thread safety**: The extension methods contain no mutable state and are therefore thread‑safe. Consequently, the test methods, which only invoke these pure functions, can be executed in parallel without risk of interference, assuming the test framework does not share mutable fixtures between tests.
