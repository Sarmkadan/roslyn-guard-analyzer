# TypeNameMatcherTestsExtensions

Provides fluent assertion extension methods for verifying the behavior of a `TypeNameMatcher` instance in unit tests.

## API

### ShouldMatchTypes
**Purpose** – Asserts that the matcher matches exactly the supplied set of types (no more, no less).  
**Parameters** –  
- `this TypeNameMatcher matcher` – The matcher to evaluate.  
- `IEnumerable<Type> expectedTypes` – The types that are expected to be matched.  
**Return value** – `void`.  
**Throws** –  
- `ArgumentNullException` if `matcher` or `expectedTypes` is `null`.  
- An assertion exception (e.g., `Xunit.AssertException`) if the matcher does not match all `expectedTypes` or matches any additional type.

### ShouldMatchNamespace
**Purpose** – Asserts that the matcher matches at least one type whose namespace equals the specified namespace and does not match types outside that namespace.  
**Parameters** –  
- `this TypeNameMatcher matcher` – The matcher to evaluate.  
- `string @namespace` – The namespace to check against.  
**Return value** – `void`.  
**Throws** –  
- `ArgumentNullException` if `matcher` or `@namespace` is `null`.  
- An assertion exception if no matched type resides in `@namespace` or if a matched type lies outside that namespace.

### ShouldMatchFullyQualifiedWithVariations
**Purpose** – Asserts that the matcher matches the given fully qualified type name, allowing for common variations such as generic arity, nullable annotations, or assembly version differences.  
**Parameters** –  
- `this TypeNameMatcher matcher` – The matcher to evaluate.  
- `string fullyQualifiedName` – The fully qualified name to match.  
- `bool includeVariations` (optional, default `true`) – When `true`, variations like `Type`2, `Type?`, and differing assembly versions are considered equivalent.  
**Return value** – `void`.  
**Throws** –  
- `ArgumentNullException` if `matcher` or `fullyQualifiedName` is `null`.  
- An assertion exception if the matcher does not match the name under the allowed variation rules.

### ShouldMatchNothing
**Purpose** – Asserts that the matcher does not match any type.  
**Parameters** –  
- `this TypeNameMatcher matcher` – The matcher to evaluate.  
**Return value** – `void`.  
**Throws** –  
- `ArgumentNullException` if `matcher` is `null`.  
- An assertion exception if the matcher matches one or more types.

## Usage

```csharp
// Verify that a matcher recognizes a specific set of types.
var matcher = TypeNameMatcher.Create(
    typeof(System.Collections.Generic.List<>),
    typeof(System.Linq.Enumerable));
matcher.ShouldMatchTypes(new[]
{
    typeof(System.Collections.Generic.List<>),
    typeof(System.Linq.Enumerable)
});
```

```csharp
// Verify namespace‑only matching and ensure no matches after clearing.
var matcher = TypeNameMatcher.ForNamespace("MyCompany.MyLib");
matcher.ShouldMatchNamespace("MyCompany.MyLib");

// After removing all patterns, the matcher should match nothing.
matcher.Clear();
matcher.ShouldMatchNothing();
```

## Notes

- All extension methods are stateless; they do not alter the matcher instance.  
- Passing `null` for the matcher or any required argument results in an `ArgumentNullException`.  
- The methods throw a unit‑testing assertion exception when the condition is not met, making them suitable for use with frameworks such as xUnit, NUnit, or MSTest.  
- Because the methods contain no static state, they are thread‑safe when invoked on distinct matcher instances concurrently. If a single `TypeNameMatcher` instance is shared and mutated across threads, external synchronization is required.  
- `ShouldMatchFullyQualifiedWithVariations` treats generic arity suffixes (e.g., `Type`2), nullable annotations (`Type?`), and differing assembly version numbers as equivalent when `includeVariations` is `true`. Setting this parameter to `false` disables those leniencies.  
- `ShouldMatchNothing` succeeds only when the matcher contains no patterns; any added pattern will cause the method to fail.
