# CodeElement

`CodeElement` represents a single code construct—such as a method, class, property, or field—extracted from a source file during Roslyn-based analysis. It serves as the primary data transfer object within the `roslyn-guard-analyzer` project, capturing structural metadata, visibility, async/static/abstract modifiers, dependencies, suppression directives, and a computed complexity score. Instances are typically produced by a Roslyn syntax walker or analyzer and consumed by guard-rule evaluators, reporting sinks, or serialization layers.

## API

### `string Id`
A unique, stable identifier for this code element. The value is typically derived from a combination of the file path and the syntax node’s document-relative position, ensuring that re-analysis of the same source produces the same identifier as long as the element’s location remains unchanged.

### `string Name`
The short, unqualified name of the code element as written in source. For a method `void DoWork()`, this is `"DoWork"`; for a class `class OrderService`, this is `"OrderService"`. Does not include namespace or containing-type prefixes.

### `CodeElementType ElementType`
An enumeration value indicating the syntactic category of the element. Expected members include `Method`, `Constructor`, `Property`, `Field`, `Event`, `Class`, `Interface`, `Struct`, `Enum`, and `Record`. Consumers use this to branch guard-rule logic per element kind.

### `string FilePath`
The absolute or relative path to the source file containing the element. The path format depends on the project’s build-time context (e.g., rooted project-relative paths during CI analysis). Always populated; never null or empty for a successfully analyzed element.

### `int StartLineNumber`
The one-based line number where the element’s declaration begins. For multi-line declarations, this points to the first line of the signature or header.

### `int EndLineNumber`
The one-based line number where the element’s declaration ends. For a single-line property such as `public int Id { get; set; }`, `StartLineNumber` and `EndLineNumber` are equal.

### `string Namespace`
The fully qualified namespace that contains the element, using `.` as a separator (e.g., `"MyApp.Services.Ordering"`). For top-level statements or global-scope elements, this may be an empty string or a compiler-generated wrapper namespace depending on the project configuration.

### `string? ParentName`
The unqualified name of the immediately enclosing type, or `null` when the element is a top-level type or a namespace member without a containing class/struct. For a method `Calculate` inside `OrderService`, this is `"OrderService"`.

### `string? FullyQualifiedName`
The fully assembly-qualified logical name, combining namespace, containing types, and the element’s own name with standard `.` separators (e.g., `"MyApp.Services.Ordering.OrderService.Calculate"`). `null` when the element does not reside within a named type hierarchy (e.g., top-level functions in a console program before C# 10’s implicit `Program` class normalization). Guard rules that need globally unique identification should prefer `Id` for stability; this property is a human-readable convenience.

### `List<string> Attributes`
The list of attribute short names or fully qualified names applied to the element, in the order they appear in source. Each entry is the name portion of the attribute (e.g., `"HttpGet"`, `"Obsolete"`). Attribute arguments are not captured. The list is empty when no attributes are present; never null.

### `List<string> Dependencies`
The set of type or member names that this element references in its body or signature, extracted from syntax and symbol resolution. Entries are typically fully qualified type names (e.g., `"System.Threading.Tasks.Task"`). Used by guard rules that validate allowed dependency graphs. The list is empty when the element has no resolvable dependencies; never null.

### `List<string> SuppressDirectives`
Roslyn `#pragma warning disable` directives and `[SuppressMessage]` attribute targets collected for this element. Each entry is the warning ID string (e.g., `"CS1591"`, `"CA1062"`). Guard analyzers consult this list to skip elements that have been explicitly opted out. The list is empty when no suppression applies; never null.

### `bool IsPublic`
`true` when the element has `public` accessibility (or is implicitly public, such as interface members); `false` for `internal`, `protected`, `private`, or any compound accessibility that is not equivalent to `public`. Guard rules that enforce API surface hardening key off this flag.

### `bool IsAsync`
`true` when the element’s declaration includes the `async` modifier. For methods, this indicates a task-returning asynchronous method. For other element types the value is always `false`.

### `bool IsStatic`
`true` when the element is declared `static`. Applies to methods, fields, properties, classes, and local functions. Extension methods are both `IsStatic` and have `this` on the first parameter, though the latter is reflected in `Parameters`.

### `bool IsAbstract`
`true` when the element is declared `abstract` (methods, properties, classes) or is a member of an interface (implicitly abstract). `false` for concrete members, virtual members without `abstract`, and non-method elements that cannot be abstract.

### `string? ReturnType`
The fully qualified return type name for methods and properties, or `null` for constructors, fields, classes, and other elements that do not have a return type. For `async Task<int> GetCountAsync()`, this is `"System.Threading.Tasks.Task<int>"`.

### `List<string> Parameters`
The list of parameter type-name pairs in declaration order, formatted as `"TypeName parameterName"` (e.g., `"int orderId"`, `"System.String? customerName"`). Empty for parameterless members and non-invocable elements; never null.

### `int Complexity`
A computed cyclomatic or cognitive complexity score derived from the element’s syntax body. The exact algorithm is determined by the analyzer configuration (e.g., counting branches, loops, and logical operators). A value of 1 typically indicates a trivial linear member. Guard rules may threshold on this value to flag overly complex code.

### `DateTime AnalyzedAt`
The UTC timestamp at which the analysis that produced this `CodeElement` completed. Used for cache invalidation, audit trails, and diff-based reporting across multiple analysis runs.

## Usage

### Example 1: Filtering public async methods with high complexity

```csharp
IEnumerable<CodeElement> elements = analyzerResults.Elements;

var highRiskMethods = elements
    .Where(e => e.ElementType == CodeElementType.Method)
    .Where(e => e.IsPublic && e.IsAsync)
    .Where(e => e.Complexity > 15)
    .Where(e => !e.SuppressDirectives.Contains("CA1506"))
    .ToList();

foreach (var method in highRiskMethods)
{
    Console.WriteLine(
        $"High-complexity async method: {method.FullyQualifiedName} " +
        $"(complexity {method.Complexity}, file {method.FilePath}:{method.StartLineNumber})");
}
```

### Example 2: Building a dependency graph and checking for unauthorized dependencies

```csharp
var publicApiElements = elements
    .Where(e => e.IsPublic)
    .Where(e => e.ElementType == CodeElementType.Method ||
                e.ElementType == CodeElementType.Property)
    .ToList();

var forbiddenDependencies = new HashSet<string>
{
    "System.Data.SqlClient.SqlConnection",
    "System.Web.HttpContext"
};

foreach (var element in publicApiElements)
{
    var violations = element.Dependencies
        .Where(d => forbiddenDependencies.Contains(d))
        .ToList();

    if (violations.Any())
    {
        Console.WriteLine(
            $"Element {element.FullyQualifiedName} depends on forbidden types: " +
            $"{string.Join(", ", violations)}");
    }
}
```

## Notes

- **Nullability of reference-type members**: `ParentName`, `FullyQualifiedName`, and `ReturnType` are nullable strings. Consumers must guard against `null` before calling instance methods such as `StartsWith` or before using the value in string formatting that expects a non-null value. `Attributes`, `Dependencies`, `SuppressDirectives`, and `Parameters` are never null; they return empty lists when no data is present.
- **Line-number accuracy**: `StartLineNumber` and `EndLineNumber` reflect the analyzer’s view of the syntax tree at the time of analysis. If source files are edited between analysis and consumption, these numbers may be stale. They are intended for reporting and navigation hints, not for programmatic source edits without re-analysis.
- **Complexity semantics**: The `Complexity` value is a snapshot produced by a configurable metric provider. Different analyzer configurations may produce different absolute numbers for the same source. Guard rules should compare against thresholds defined in the same configuration context rather than assuming a universal scale.
- **Thread safety**: `CodeElement` is a plain data object with no internal synchronization. After an instance is fully populated and published, concurrent reads from multiple threads are safe provided no thread mutates the lists (`Attributes`, `Dependencies`, `SuppressDirectives`, `Parameters`) or reassigns properties. The typical lifecycle—construct, populate, seal, and distribute—ensures safe sharing. If a consumer needs to modify an instance for local bookkeeping, it should create a copy or wrap the instance in a synchronization construct.
- **Identifier stability**: The `Id` property is designed to be stable across re-analysis of the same source location. It should not be used as a persistence key across source refactorings that move or rename the element; for that purpose, a separate durable identifier (e.g., a GUID assigned at element creation) would be required.
- **Suppression directive format**: Entries in `SuppressDirectives` are the raw diagnostic IDs as they appear in `#pragma warning disable` or `[SuppressMessage]`. They are not normalized (e.g., `"CA1062"` vs `"CA 1062"`). Guard rules should compare using ordinal case-insensitive matching after trimming to avoid mismatches caused by whitespace variations in source.
