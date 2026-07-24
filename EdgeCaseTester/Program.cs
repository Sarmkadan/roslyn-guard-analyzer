using System;
using System.Globalization;
using RoslynGuardAnalyzer.Utilities;

Console.WriteLine("Testing edge cases for naming convention converters...\n");

// Test acronyms
TestCase("HTTPServer", "PascalCase", nameof(StringExtensions.ToPascalCase));
TestCase("http_server", "snake_case", nameof(StringExtensions.ToSnakeCase));

// Test digits
TestCase("utf8String", "PascalCase", nameof(StringExtensions.ToPascalCase));
TestCase("UTF8String", "snake_case", nameof(StringExtensions.ToSnakeCase));

// Test consecutive separators
TestCase("hello__world", "PascalCase", nameof(StringExtensions.ToPascalCase));
TestCase("hello--world", "PascalCase", nameof(StringExtensions.ToPascalCase));
TestCase("hello__world", "snake_case", nameof(StringExtensions.ToSnakeCase));

// Test leading/trailing separators
TestCase("_hello_world", "PascalCase", nameof(StringExtensions.ToPascalCase));
TestCase("hello_world_", "PascalCase", nameof(StringExtensions.ToPascalCase));
TestCase("__hello__world__", "snake_case", nameof(StringExtensions.ToSnakeCase));

// Test mixed separators
TestCase("hello-world_foo bar", "PascalCase", nameof(StringExtensions.ToPascalCase));

// Test empty parts from consecutive separators
TestCase("hello__world", "camelCase", nameof(StringExtensions.ToCamelCase));

// Test round-trip: snake -> Pascal -> snake
TestRoundTrip("hello_world", "snake_to_pascal_to_snake");
TestRoundTrip("hello_world_foo_bar", "snake_to_pascal_to_snake");
TestRoundTrip("HTTP_server", "acronym_snake_to_pascal_to_snake");
TestRoundTrip("utf8_string", "digits_snake_to_pascal_to_snake");

// Test culture sensitivity (Turkish 'i' issue)
Console.WriteLine("Testing culture sensitivity...");
var originalCulture = CultureInfo.CurrentCulture;
try
{
    // Turkish culture has special handling for 'i' and 'I'
    CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
    TestCase("istanbul", "PascalCase", nameof(StringExtensions.ToPascalCase));
    TestCase("Istanbul", "snake_case", nameof(StringExtensions.ToSnakeCase));
}
finally
{
    CultureInfo.CurrentCulture = originalCulture;
}

Console.WriteLine("\nAll tests completed!");

static void TestCase(string input, string expectedType, string methodName)
{
    Console.WriteLine($"Input: '{input}' -> {expectedType}");

    try
    {
        string result = null;
        switch (methodName)
        {
            case nameof(StringExtensions.ToPascalCase):
                result = input.ToPascalCase();
                break;
            case nameof(StringExtensions.ToCamelCase):
                result = input.ToCamelCase();
                break;
            case nameof(StringExtensions.ToSnakeCase):
                result = input.ToSnakeCase();
                break;
            case nameof(StringExtensions.ToKebabCase):
                result = input.ToKebabCase();
                break;
        }

        Console.WriteLine($"  Result: '{result}'");
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ERROR: {ex.Message}");
        Console.WriteLine();
    }
}

static void TestRoundTrip(string input, string testName)
{
    Console.WriteLine($"Round-trip test: {testName}");
    Console.WriteLine($"  Original: '{input}'");

    try
    {
        string pascal = input.ToPascalCase();
        Console.WriteLine($"  ToPascalCase: '{pascal}'");

        string backToSnake = pascal.ToSnakeCase();
        Console.WriteLine($"  ToSnakeCase: '{backToSnake}'");

        bool success = string.Equals(input, backToSnake, StringComparison.Ordinal);
        Console.WriteLine($"  Round-trip successful: {success}");
        if (!success)
        {
            Console.WriteLine($"  Expected: '{input}', Got: '{backToSnake}'");
        }
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ERROR: {ex.Message}");
        Console.WriteLine();
    }
}
