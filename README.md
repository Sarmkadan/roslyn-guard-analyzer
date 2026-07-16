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

### Usage Example 739