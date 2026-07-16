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
