# ValidationExtensions

A collection of reusable validation extension methods for common scenarios such as string validation, numeric range checks, collection content validation, file system path validation, pattern matching, type compatibility checks, and multi-rule validation with aggregated error reporting.

## API

### `IsValidString`
Determines whether a string is neither `null` nor empty nor consists only of whitespace.

- **Parameters**
  - `value` (`string`) – The string to validate.
- **Return value**
  - `bool` – `true` if the string is valid; otherwise, `false`.
- **Remarks**
  - Returns `false` for `null`, `string.Empty`, or strings containing only whitespace.

### `IsInRange<T>`
Checks whether a value is within the specified inclusive range.

- **Parameters**
  - `value` (`T`) – The value to check.
  - `min` (`T`) – The lower bound (inclusive).
  - `max` (`T`) – The upper bound (inclusive).
- **Return value**
  - `bool` – `true` if `min <= value <= max`; otherwise, `false`.
- **Remarks**
  - Requires `IComparable<T>` on `T`. Throws `ArgumentException` if `min` is greater than `max`.

### `HasItems<T>`
Validates that a collection contains at least one element.

- **Parameters**
  - `collection` (`IEnumerable<T>`) – The collection to inspect.
- **Return value**
  - `bool` – `true` if the collection is non-null and contains at least one item; otherwise, `false`.
- **Remarks**
  - Returns `false` if `collection` is `null` or empty.

### `FilePathExists`
Checks whether the specified file system path points to an existing file.

- **Parameters**
  - `path` (`string`) – The file path to validate.
- **Return value**
  - `bool` – `true` if the path exists and refers to a file; otherwise, `false`.
- **Remarks**
  - Returns `false` if `path` is `null`, empty, or refers to a directory or non-existent location.

### `DirectoryPathExists`
Checks whether the specified file system path points to an existing directory.

- **Parameters**
  - `path` (`string`) – The directory path to validate.
- **Return value**
  - `bool` – `true` if the path exists and refers to a directory; otherwise, `false`.
- **Remarks**
  - Returns `false` if `path` is `null`, empty, or refers to a file or non-existent location.

### `IsOneOf<T>`
Determines whether a value is equal to any of the provided candidates.

- **Parameters**
  - `value` (`T`) – The value to test.
  - `candidates` (`params T[]`) – The set of values to compare against.
- **Return value**
  - `bool` – `true` if `value` matches any element in `candidates`; otherwise, `false`.
- **Remarks**
  - Uses `EqualityComparer<T>.Default` for comparison. Returns `false` if `candidates` is `null` or empty.

### `IsPositive`
Checks whether a numeric value is strictly greater than zero.

- **Parameters**
  - `value` (`decimal`) – The numeric value to validate.
- **Return value**
  - `bool` – `true` if `value > 0`; otherwise, `false`.
- **Remarks**
  - Suitable for financial or counting scenarios where zero is not considered positive.

### `IsNonNegative`
Checks whether a numeric value is greater than or equal to zero.

- **Parameters**
  - `value` (`decimal`) – The numeric value to validate.
- **Return value**
  - `bool` – `true` if `value >= 0`; otherwise, `false`.

### `MatchesPattern`
Determines whether a string matches the specified regular expression pattern.

- **Parameters**
  - `input` (`string`) – The string to match.
  - `pattern` (`string`) – The regular expression pattern.
- **Return value**
  - `bool` – `true` if `input` matches `pattern`; otherwise, `false`.
- **Remarks**
  - Returns `false` if either `input` or `pattern` is `null`. Throws `ArgumentException` if `pattern` is not a valid regular expression.

### `IsAssignableFrom`
Checks whether the specified type is assignable from the given type.

- **Parameters**
  - `baseType` (`Type`) – The base or interface type.
  - `derivedType` (`Type`) – The type to test for assignability.
- **Return value**
  - `bool` – `true` if `derivedType` is assignable to `baseType`; otherwise, `false`.
- **Remarks**
  - Returns `false` if either `baseType` or `derivedType` is `null`.

### `ValidateAll(params Func<bool>[] validators)`
Executes multiple validation functions and aggregates all failures into a list of error messages.

- **Parameters**
  - `validators` (`params Func<bool>[]`) – The validation functions to execute.
- **Return value**
  - `(bool IsValid, List<string> Errors)` – A tuple where `IsValid` indicates overall success, and `Errors` contains all failure messages.
- **Remarks**
  - All validators are executed regardless of intermediate results. Returns `(true, new List<string>())` if no validators fail.

## Usage
