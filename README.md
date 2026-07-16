# Roslyn Guard Analyzer

...

## StringExtensions

The `StringExtensions` class provides a comprehensive set of utility extension methods for string manipulation, naming convention conversion, and validation. It includes methods for converting between different naming conventions (PascalCase, camelCase, snake_case, kebab-case), truncating strings, checking string patterns, and performing fuzzy string matching.

### Usage Example

// Convert between different naming conventions
var pascalCase = "hello_world".ToPascalCase();
Console.WriteLine(pascalCase); // Output: HelloWorld

var camelCase = "hello_world".ToCamelCase();
Console.WriteLine(camelCase); // Output: helloWorld

var snakeCase = "HelloWorld".ToSnakeCase();
Console.WriteLine(snakeCase); // Output: hello_world

var kebabCase = "HelloWorld".ToKebabCase();
Console.WriteLine(kebabCase); // Output: hello-world

// Truncate a string
var truncated = "This is a long string that needs truncation".Truncate(20);
Console.WriteLine(truncated); // Output: This is a long str...

// Check if string starts or ends with specific patterns
var startsWith = "HelloWorld".StartsWithAny("hello", "world", "test");
Console.WriteLine(startsWith); // Output: True

var endsWith = "HelloWorld".EndsWithAny("world", "test", "foo");
Console.WriteLine(endsWith); // Output: True

// Count occurrences of a substring
var count = "hello_hello_hello".CountOccurrences("hello");
Console.WriteLine(count); // Output: 3

// Remove whitespace from a string
var noWhitespace = "Hello World With Spaces".RemoveWhitespace();
Console.WriteLine(noWhitespace); // Output: HelloWorldWithSpaces

// Repeat a string
var repeated = "abc".Repeat(3);
Console.WriteLine(repeated); // Output: abcabcabc

// Validate a string matches a pattern
var isMatch = "test123".MatchesPattern("^[a-z]+[0-9]+");
Console.WriteLine(isMatch); // Output: True

// Check if a string is a valid C# identifier
var isValid = "MyVariable".IsValidIdentifier();
Console.WriteLine(isValid); // Output: True

// Calculate Levenshtein distance for fuzzy matching
var distance = "kitten".LevenshteinDistance("sitting");
Console.WriteLine(distance); // Output: 3

## PathNormalizer

The `PathNormalizer` type contains static methods for normalizing and comparing paths.

Example usage:
```csharp
string normalizedPath = PathNormalizer.Normalize("/home/user/relative/path");
string[] normalizedPaths = PathNormalizer.NormalizeMany(new string[] { "path1", "path2" });
bool arePathsEqual = PathNormalizer.ArePathsEqual("path1", "path2");
string relativePath = PathNormalizer.GetRelativePath("/home/user/absolute/path", "/home/user/base/path");
bool isAbsolute = PathNormalizer.IsAbsolute("/home/user/absolute/path");
string combinedPath = PathNormalizer.Combine("/home/user/base/path", "relative/path");
string directoryName = PathNormalizer.GetDirectoryName("/home/user/absolute/path");
string fileName = PathNormalizer.GetFileName("/home/user/absolute/path");
string extension = PathNormalizer.GetExtension("/home/user/absolute/path");
bool hasExtension = PathNormalizer.HasExtension("/home/user/absolute/path");
```

## FileSystemHelper

The `FileSystemHelper` class provides utility methods for file system operations with built-in error handling and exclusion patterns. It helps discover C# files and project files while automatically excluding common build artifacts like `bin`, `obj`, `.git`, and other directories. The class includes methods for file existence checks, reading/writing files asynchronously, and retrieving file metadata such as size and last modified time.


### Usage Example

```csharp
using RoslynGuardAnalyzer.Utilities;

// Find all C# files in a directory (excluding bin, obj, .git, etc.)
string[] csharpFiles = FileSystemHelper.FindCSharpFiles("/path/to/project");
Console.WriteLine($"Found {csharpFiles.Length} C# files");

// Find all project files (.csproj and .fsproj)
string[] projectFiles = FileSystemHelper.FindProjectFiles("/path/to/project");
Console.WriteLine($"Found {projectFiles.Length} project files");

// Check if a file exists
bool fileExists = FileSystemHelper.FileExists("/path/to/file.cs");
Console.WriteLine(fileExists ? "File exists" : "File does not exist");

// Read a file asynchronously
string? fileContent = await FileSystemHelper.ReadFileAsync("/path/to/file.cs");
if (fileContent != null)
{
    Console.WriteLine($"File content length: {fileContent.Length}");
}

// Write a file asynchronously
bool writeSuccess = await FileSystemHelper.WriteFileAsync("/path/to/newfile.cs", "public class NewClass { }");
Console.WriteLine(writeSuccess ? "File written successfully" : "Failed to write file");

// Get file metadata
long fileSize = FileSystemHelper.GetFileSize("/path/to/file.cs");
Console.WriteLine($"File size: {fileSize} bytes");

DateTime? lastModified = FileSystemHelper.GetLastModifiedTime("/path/to/file.cs");
if (lastModified.HasValue)
{
    Console.WriteLine($"Last modified: {lastModified.Value}");
}

// Check if a directory exists
bool dirExists = FileSystemHelper.DirectoryExists("/path/to/directory");
Console.WriteLine(dirExists ? "Directory exists" : "Directory does not exist");
```

## ReflectionHelper

The `ReflectionHelper` class provides utility methods for reflection operations on types and members. It simplifies extracting metadata from code elements for analysis, including checking type hierarchies, method properties, attribute discovery, and interface implementation verification.



### Usage Example

```csharp
using System;
using System.Linq;
using System.Reflection;
using RoslynGuardAnalyzer.Utilities;

// Analyze a sample type
public class SampleClass : IDisposable
{
    public string Name { get; set; }
    public int Value { get; set; }
    public void Dispose() { }
    public void Method1(int param) { }
    public static void StaticMethod() { }
}

public class Program
{
    public static void Main()
    {
        Type type = typeof(SampleClass);
        
        // Get all public methods
        var methods = ReflectionHelper.GetPublicMethods(type);
        Console.WriteLine($"Public methods: {methods.Count()}");
        
        // Get all public properties
        var properties = ReflectionHelper.GetPublicProperties(type);
        Console.WriteLine($"Public properties: {properties.Count()}");
        
        // Get all public fields
        var fields = ReflectionHelper.GetPublicFields(type);
        Console.WriteLine($"Public fields: {fields.Count()}");
        
        // Check if type implements interface
        bool implementsIDisposable = ReflectionHelper.ImplementsInterface(type, typeof(IDisposable));
        Console.WriteLine($"Implements IDisposable: {implementsIDisposable}");
        
        // Check if type is a subclass
        bool isSubclass = ReflectionHelper.IsSubclassOf(type, typeof(object));
        Console.WriteLine($"Is subclass of object: {isSubclass}");
        
        // Get attributes
        var obsoleteAttrs = ReflectionHelper.GetAttributes<ObsoleteAttribute>(type.GetMethod("Dispose"));
        Console.WriteLine($"Has Obsolete attribute: {obsoleteAttrs.Any()}");
        
        // Check if method is async
        var method = type.GetMethod("Method1");
        bool isAsync = ReflectionHelper.IsAsync(method);
        Console.WriteLine($"Method1 is async: {isAsync}");
        
        // Check if method is virtual
        bool isVirtual = ReflectionHelper.IsVirtual(method);
        Console.WriteLine($"Method1 is virtual: {isVirtual}");
        
        // Get parameter count and names
        int paramCount = ReflectionHelper.GetParameterCount(method);
        var paramNames = ReflectionHelper.GetParameterNames(method);
        Console.WriteLine($"Method1 has {paramCount} parameters: {string.Join(", ", paramNames)}");
        
        // Get type information
        bool isValueType = ReflectionHelper.IsValueType(type);
        bool isAbstract = ReflectionHelper.IsAbstract(type);
        bool isSealed = ReflectionHelper.IsSealed(type);
        Console.WriteLine($"Is value type: {isValueType}, Is abstract: {isAbstract}, Is sealed: {isSealed}");
        
        // Get base type
        Type? baseType = ReflectionHelper.GetBaseType(type);
        Console.WriteLine($"Base type: {baseType?.Name ?? "null"}");
        
        // Get inheritance hierarchy
        var hierarchy = ReflectionHelper.GetInheritanceHierarchy(type);
        Console.WriteLine($"Inheritance hierarchy: {string.Join(" → ", hierarchy.Select(t => t.Name))}");
    }
}
``` 739
## ValidationExtensions

The `ValidationExtensions` class provides a comprehensive set of extension methods for common validation scenarios in C# applications. It offers fluent validation patterns with detailed error messages for strings, collections, file paths, numeric ranges, and type compatibility checks. Each method follows a consistent pattern returning a boolean success indicator along with an optional error message.

## FormatterRegistry

The `FormatterRegistry` class serves as a centralized registry for output formatters, enabling dynamic registration and lookup of formatters by format identifier. It provides methods to create a registry with default formatters, register custom formatters, and query supported formats. This registry is particularly useful when you need to support multiple output formats (JSON, CSV, HTML, etc.) in your application and want to provide a flexible way to extend formatting capabilities.

### Usage Example

```csharp
using System;
using System.Linq;
using RoslynGuardAnalyzer.Formatters;

// Create a registry with default formatters (JSON, CSV, HTML)
var registry = FormatterRegistry.CreateWithDefaults();

Console.WriteLine($"Supported formats: {string.Join(", ", registry.GetSupportedFormats())}");
Console.WriteLine($"Total formatters: {registry.Count}");

// Register a custom formatter
registry.Register(new CustomFormatter());

// Check if a format is supported
bool supportsJson = registry.IsFormatSupported("json");
Console.WriteLine($"JSON format supported: {supportsJson}");

// Get a formatter (returns null if not found)
var jsonFormatter = registry.GetFormatter("json");
if (jsonFormatter != null)
{
    Console.WriteLine($"JSON formatter type: {jsonFormatter.GetType().Name}");
}

// Get a formatter or throw if not found
try
{
    var csvFormatter = registry.GetFormatterOrThrow("csv");
    Console.WriteLine($"CSV formatter type: {csvFormatter.GetType().Name}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

// Example custom formatter class
public class CustomFormatter : IOutputFormatter
{
    public string Format => "custom";
    
    public string FormatOutput(object data)
    {
        return $"Custom formatted output: {data}";
    }
}
```

## AnalysisFilterBuilder

The `AnalysisFilterBuilder` class provides a fluent API for creating filters to selectively process Roslyn analysis results. It allows filtering violations by severity, rule identifiers, file paths, line numbers, message content, and custom predicates. Filters can be chained together to create complex filtering logic, and the built filter can be applied to collections of violations or converted to a predicate function.

### Usage Example

```csharp
using System;
using System.Linq;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Utilities;

// Sample violations to filter
var violations = new RuleViolation[]
{
    new("CA1822", "Program.cs", 10, SeverityLevel.Warning, "Make method static"),
    new("CA1051", "Program.cs", 15, SeverityLevel.Error, "Do not declare visible instance fields"),
    new("CA1822", "Startup.cs", 25, SeverityLevel.Info, "Make method static"),
    new("CA1711", "Models/User.cs", 42, SeverityLevel.Warning, "Identifiers should not have incorrect suffix"),
    new("CA1051", "Models/User.cs", 50, SeverityLevel.Critical, "Do not declare visible instance fields")
};

// Create a filter that:
// - Includes only errors and critical violations
// - Filters to violations in Program.cs
// - Starts from line 10
var filter = new AnalysisFilterBuilder()
    .MinimumSeverity("Error")
    .ByFile("Program.cs")
    .FromLine(10)
    .Build();

// Apply the filter
var filtered = violations.Where(filter).ToList();
Console.WriteLine($"Found {filtered.Count} violations:");
foreach (var violation in filtered)
{
    Console.WriteLine($"  {violation.RuleName} at {violation.FilePath}:{violation.LineNumber} - {violation.Severity}");
}

// Alternative: Use the Apply method directly
var criticalErrors = new AnalysisFilterBuilder()
    .BySeverity(SeverityLevel.Critical)
    .Apply(violations);

Console.WriteLine($"Critical errors: {criticalErrors.Count()}");

// Complex filter with custom predicate
var complexFilter = new AnalysisFilterBuilder()
    .BySeverity("Warning")
    .ContainsMessage("static")
    .Where(v => v.LineNumber < 50)
    .Build();
```




## PerformanceAnalyzer



The `PerformanceAnalyzer` class provides comprehensive performance monitoring and analysis capabilities for tracking execution time metrics across different components of an application. It records timing data for operations, calculates statistics (minimum, maximum, average execution times), identifies performance bottlenecks, and generates detailed reports. The analyzer helps developers identify slow operations and optimize application performance by providing insights into execution patterns and timing distributions.




### Usage Example



```csharp
using System;
using RoslynGuardAnalyzer.Utilities;

// Create a performance analyzer for a specific component
var analyzer = new PerformanceAnalyzer(
    componentName: "CodeAnalysisEngine",
    totalTimeMs: 0,
    minTimeMs: 0,
    maxTimeMs: 0,
    averageTimeMs: 0,
    executionCount: 0,
    percentageOfTotal: 0
);

// Record timing for an operation
analyzer.RecordTiming(150);  // Record 150ms execution time
analyzer.RecordTiming(210);  // Record 210ms execution time
analyzer.RecordTiming(95);   // Record 95ms execution time

// Get metrics for the component
var metrics = analyzer.GetMetricsForComponent();
if (metrics != null)
{
    Console.WriteLine($"Component: {metrics.ComponentName}");
    Console.WriteLine($"Total time: {metrics.TotalTimeMs}ms");
    Console.WriteLine($"Average time: {metrics.AverageTimeMs}ms");
    Console.WriteLine($"Execution count: {metrics.ExecutionCount}");
    Console.WriteLine($"Min time: {metrics.MinTimeMs}ms");
    Console.WriteLine($"Max time: {metrics.MaxTimeMs}ms");
    Console.WriteLine($"Percentage of total: {metrics.PercentageOfTotal}%");
}

// Get all performance metrics
var allMetrics = analyzer.GetAllMetrics();
Console.WriteLine($"Total components tracked: {allMetrics.Count}");

// Identify performance bottlenecks (top 5 slowest operations)
var bottlenecks = analyzer.GetBottlenecks(5);
Console.WriteLine($"Top {bottlenecks.Count} bottlenecks:");
foreach (var bottleneck in bottlenecks)
{
    Console.WriteLine($"  {bottleneck.ComponentName}: {bottleneck.AverageTimeMs}ms avg");
}

// Generate a comprehensive performance report
string report = analyzer.GenerateReport();
Console.WriteLine(report);

// Clear recorded metrics when needed
analyzer.Clear();

// Check if component has data
if (analyzer.HasComponent)
{
    Console.WriteLine("Performance data available");
}

// Get total time across all components
long totalTime = analyzer.GetTotalTimeMs();
Console.WriteLine($"Total execution time: {totalTime}ms");
```






### Usage Example

```csharp
using System;
using System.Collections.Generic;
using RoslynGuardAnalyzer.Utilities;

// Validate a string is not null or empty
bool isValidString = "test value".IsValidString(out string? stringError);
Console.WriteLine(isValidString ? "String is valid" : $"String error: {stringError}");

// Validate a value is within range
bool isInRange = 42.IsInRange(1, 100, out string? rangeError);
Console.WriteLine(isInRange ? "Value is in range" : $"Range error: {rangeError}");

// Validate a collection has items
var items = new List<string> { "item1", "item2" };
bool hasItems = items.HasItems(out string? collectionError);
Console.WriteLine(hasItems ? "Collection has items" : $"Collection error: {collectionError}");

// Validate a file path exists
bool fileExists = @"./test.txt".FilePathExists(out string? fileError);
Console.WriteLine(fileExists ? "File exists" : $"File error: {fileError}");

// Validate a directory path exists
bool dirExists = @"./testdir".DirectoryPathExists(out string? dirError);
Console.WriteLine(dirExists ? "Directory exists" : $"Directory error: {dirError}");

// Validate a value is one of allowed values
bool isOneOf = "option1".IsOneOf(new[] { "option1", "option2", "option3" }, out string? oneOfError);
Console.WriteLine(isOneOf ? "Value is allowed" : $"OneOf error: {oneOfError}");

// Validate a numeric value is positive
bool isPositive = 5.IsPositive(out string? positiveError);
Console.WriteLine(isPositive ? "Value is positive" : $"Positive error: {positiveError}");

// Validate a numeric value is non-negative
bool isNonNegative = 0.IsNonNegative(out string? nonNegativeError);
Console.WriteLine(isNonNegative ? "Value is non-negative" : $"Non-negative error: {nonNegativeError}");

// Validate a string matches a pattern
bool matchesPattern = "test123".MatchesPattern("^[a-z]+[0-9]+", out string? patternError);
Console.WriteLine(matchesPattern ? "Pattern matches" : $"Pattern error: {patternError}");

// Validate type assignability
bool isAssignable = typeof(string).IsAssignableFrom(typeof(object), out string? assignableError);
Console.WriteLine(isAssignable ? "Type is assignable" : $"Assignable error: {assignableError}");

// Validate multiple conditions at once
var validationResult = ValidationExtensions.ValidateAll(
    ("test".IsValidString(out _), "String validation failed"),
    (42.IsInRange(1, 100, out _), "Range validation failed"),
    (new[] { 1, 2, 3 }.HasItems(out _), "Collection validation failed")
);
Console.WriteLine(validationResult.IsValid ? "All validations passed" : $"Validations failed: {string.Join(", ", validationResult.Errors)}");
```
