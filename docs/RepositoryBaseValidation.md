# RepositoryBaseValidation

`RepositoryBaseValidation` provides a set of static helper methods that perform validation on identifiers, entities, collections of entities, and predicates used by repository implementations. The class centralizes guard‑clause logic, returning detailed error messages or throwing exceptions when validation fails, thereby ensuring consistent validation behavior across the data‑access layer.

## API

| Member | Purpose | Parameters | Return Value | Throws |
|--------|---------|------------|--------------|--------|
| `public static IReadOnlyList<string> Validate<T>(T entity)` | Validates a single entity of type `T` according to the repository’s rules. | `entity` – the instance to validate. | A read‑only list of validation error messages. The list is empty when the entity is valid. | None. |
| `public static bool IsValid<T>(T entity)` | Determines whether a single entity of type `T` passes validation. | `entity` – the instance to check. | `true` if the entity is valid; otherwise `false`. | None. |
| `public static void EnsureValid<T>(T entity)` | Validates a single entity and throws if any validation errors are found. | `entity` – the instance to validate. | *void* | `ArgumentException` (or a derived exception) containing the validation errors when the entity is invalid. |
| `public static IReadOnlyList<string> ValidateId(string id)` | Validates a repository identifier (e.g., a primary key). | `id` – the identifier to validate. | A read‑only list of validation error messages. Empty when the identifier is valid. | None. |
| `public static IReadOnlyList<string> ValidateEntity<T>(T entity)` | Alias for `Validate<T>`; validates a single entity. | `entity` – the instance to validate. | A read‑only list of validation error messages. | None. |
| `public static IReadOnlyList<string> ValidateEntities<T>(IEnumerable<T> entities)` | Validates a collection of entities, aggregating errors from each element. | `entities` – the collection to validate. | A read‑only list of validation error messages. The list contains errors from all invalid items; empty when all items are valid. | None. |
| `public static IReadOnlyList<string> ValidatePredicate<T>(Func<T, bool> predicate)` | Validates a predicate used for filtering or querying repository data. | `predicate` – the delegate to validate. | A read‑only list of validation error messages. Empty when the predicate is considered valid. | None. |
| `public static void EnsureValidId(string id)` | Validates an identifier and throws if it is invalid. | `id` – the identifier to validate. | *void* | `ArgumentException` (or a derived exception) when the identifier fails validation. |
| `public static void EnsureValidEntity<T>(T entity)` | Validates a single entity and throws on failure. | `entity` – the instance to validate. | *void* | `ArgumentException` (or a derived exception) when the entity is invalid. |
| `public static void EnsureValidEntities<T>(IEnumerable<T> entities)` | Validates a collection of entities and throws if any are invalid. | `entities` – the collection to validate. | *void* | `ArgumentException` (or a derived exception) when one or more entities fail validation. |
| `public static void EnsureValidPredicate<T>(Func<T, bool> predicate)` | Validates a predicate and throws if it does not meet the required criteria. | `predicate` – the delegate to validate. | *void* | `ArgumentException` (or a derived exception) when the predicate is invalid. |

### General Behaviour

* All `Validate*` methods never throw; they return a list of error messages that callers can inspect.
* All `EnsureValid*` methods perform the same validation as their `Validate*` counterparts but raise an exception when the returned error list is non‑empty.
* The generic type parameter `T` is unconstrained; validation logic is based on conventions defined elsewhere in the repository implementation (e.g., required properties, key formats).
* Null arguments are treated as invalid and result in an appropriate error message (or exception for the `EnsureValid*` overloads).

## Usage

### Example 1 – Validating a single entity before insertion

