# StringExtensions

The `StringExtensions` class provides a collection of static extension methods for the `string` type, designed to simplify common string manipulation tasks in the context of the `roslyn-guard-analyzer` project. These methods cover casing conversions, truncation, pattern matching, whitespace removal, repetition, and distance calculations, all implemented as pure functions that do not modify the original string.

## API

### `ToPascalCase`

Converts the input string to PascalCase (e.g., `"hello world"` becomes `"HelloWorld"`).

- **Parameters**: `this string value` – the string to convert.
- **Returns**: `string` – the PascalCase representation.
- **Throws**: `ArgumentNullException` if `value` is `null`.

### `ToCamelCase`

Converts the input string to camelCase (e.g., `"hello world"` becomes `"helloWorld"`).

- **Parameters**: `this string value` – the string to convert.
- **Returns**: `string` – the camelCase representation.
- **Throws**: `ArgumentNullException` if `value` is `null`.

### `ToSnakeCase`

Converts the input string to snake_case (e.g., `"hello world"` becomes `"hello_world"`).

- **Parameters**: `this string value` – the string to convert.
- **Returns**: `string` – the snake_case representation.
- **Throws**: `ArgumentNullException` if `value` is `null`.

### `ToKebabCase`

Converts the input string to kebab-case (e.g., `"hello world"` becomes `"hello-world"`).

- **Parameters**: `this string value` – the string to convert.
- **Returns**: `string` – the kebab-case representation.
- **Throws**: `ArgumentNullException` if `value` is `null`.

### `Truncate`

Truncates the string to a specified maximum length, optionally appending a suffix.

- **Parameters**:
  - `this string value` – the string to truncate.
  - `int maxLength` – the maximum number of characters to keep.
  - `string suffix` (optional, default `"..."`) – the string to append if truncation occurs.
- **Returns**: `string` – the truncated string, or the original string if its length does not exceed `maxLength`.
- **Throws**: `ArgumentNullException` if `value` is `null`; `ArgumentOutOfRangeException` if `maxLength` is less than zero.

### `StartsWithAny`

Determines whether the string starts with any of the specified values.

- **Parameters**:
  - `this string value` – the string to check.
  - `params string[] values` – one or more strings to compare against the start.
- **Returns**: `bool` – `true` if the string starts with any element of `values`; otherwise `false`.
- **Throws**: `ArgumentNullException` if `value` or `values` is `null`.

### `EndsWithAny`

Determines whether the string ends with any of the specified values.

- **Parameters**:
  - `this string value` – the string to check.
  - `params string[] values` – one or more strings to compare against the end.
- **Returns**: `bool` – `true` if the string ends with any element of `values`; otherwise `false`.
- **Throws**: `ArgumentNullException` if `value` or `values` is `null`.

### `CountOccurrences`

Counts the number of non-overlapping occurrences of a substring within the string.

- **Parameters**:
  - `this string value` – the source string.
  - `string substring` – the substring to search for.
- **Returns**: `int` – the count of occurrences.
- **Throws**: `ArgumentNullException` if `value` or `substring` is `null`; `ArgumentException` if `substring` is empty.

### `RemoveWhitespace`

Removes all whitespace characters from the string.

- **Parameters**: `this string value` – the string to process.
- **Returns**: `string` – a new string with all whitespace removed.
- **Throws**: `ArgumentNullException` if `value` is `null`.

### `Repeat`

Repeats the string a specified number of times.

- **Parameters**:
  - `this string value` – the string to repeat.
  - `int count` – the number of times to repeat.
- **Returns**: `string` – the concatenated result.
- **Throws**: `ArgumentNullException` if `value` is `null`; `ArgumentOutOfRangeException` if `count` is less than zero.

### `MatchesPattern`

Checks whether the string matches a given regular expression pattern.

- **Parameters**:
  - `this string value` – the string to test.
  - `string pattern` – a regular expression pattern.
- **Returns**: `bool` – `true` if the entire string matches the pattern; otherwise `false`.
- **Throws**: `ArgumentNullException` if `value` or `pattern` is `null`; `RegexParseException` if the pattern is invalid.

### `IsValidIdentifier`

Determines whether the string is a valid C# identifier (as defined by the language specification).

- **Parameters**: `this string value` – the string to validate.
- **Returns**: `bool` – `true` if the string is a valid identifier; otherwise `false`.
- **Throws**: `ArgumentNullException` if `value` is `null`.

### `LevenshteinDistance`

Computes the Levenshtein distance (edit distance) between two strings.

- **Parameters**:
  - `this string value` – the first string.
  - `string other` – the second string.
- **Returns**: `int` – the minimum number of single-character edits (insertions, deletions, or substitutions) required to change one string into the other.
- **Throws**: `ArgumentNullException` if `value` or `other` is `null`.

## Usage

```csharp
using RoslynGuardAnalyzer;

// Example 1: Casing conversions and truncation
string original = "hello world example";
string pascal = original.ToPascalCase();        // "HelloWorldExample"
string camel = original.ToCamelCase();          // "helloWorldExample"
string snake = original.ToSnakeCase();          // "hello_world_example"
string kebab = original.ToKebabCase();          // "hello-world-example"
string truncated = original.Truncate(10, "..."); // "hello wor..."

// Example 2: Pattern matching and distance
string code = "myVariable";
bool isValid = code.IsValidIdentifier();        // true
bool startsWithAny = code.StartsWithAny("my", "your"); // true
int distance = "kitten".LevenshteinDistance("sitting"); // 3
int occurrences = "ababa".CountOccurrences("aba");      // 1 (non-overlapping)
string repeated = "ab".Repeat(3);                       // "ababab"
string noSpace = " a b c ".RemoveWhitespace();          // "abc"
```

## Notes

- All methods are extension methods and operate on the `string` type. They are static and thread-safe because they do not modify any shared state; each call works on its own input parameters and returns a new string or value.
- The casing conversion methods (`ToPascalCase`, `ToCamelCase`, `ToSnakeCase`, `ToKebabCase`) treat non-alphanumeric characters as word separators and preserve digits. They do not handle special Unicode categories beyond letters and digits; behavior for mixed-script strings is implementation-defined.
- `Truncate` with a `maxLength` of zero returns an empty string (plus the suffix if provided). If `maxLength` is greater than or equal to the string length, the original string is returned unchanged.
- `StartsWithAny` and `EndsWithAny` perform ordinal (culture-insensitive) comparisons. An empty `values` array returns `false`.
- `CountOccurrences` uses ordinal comparison and does not count overlapping matches.
- `RemoveWhitespace` considers all characters for which `Char.IsWhiteSpace` returns `true`.
- `MatchesPattern` uses `Regex.IsMatch` with default options (single-line, culture-invariant). The pattern must match the entire string; for partial matches, wrap the pattern with `.*` as needed.
- `IsValidIdentifier` follows the C# specification for identifiers: it must start with a letter or underscore, followed by letters, digits, or underscores. Unicode escape sequences are not supported; the check is based on `Char.IsLetter` and `Char.IsDigit`.
- `LevenshteinDistance` has O(n*m) time and space complexity, where n and m are the lengths of the two strings. For very long strings, consider performance implications.
