#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using RoslynGuardAnalyzer.Utilities;
using Xunit;

/// <summary>
/// Property-based tests for string extension methods to ensure edge case handling.
/// Tests round-trip conversions and common edge cases.
/// </summary>
public sealed class StringExtensionsEdgeCaseTests
{
    #region ToPascalCase Edge Cases

    [Theory]
    [InlineData("hello_world", "HelloWorld")]
    [InlineData("hello_world_foo", "HelloWorldFoo")]
    [InlineData("hello__world", "HelloWorld")] // consecutive separators
    [InlineData("hello--world", "HelloWorld")] // consecutive separators
    [InlineData("_hello_world", "HelloWorld")] // leading separator
    [InlineData("hello_world_", "HelloWorld")] // trailing separator
    [InlineData("__hello__world__", "HelloWorld")] // multiple leading/trailing
    [InlineData("hello-world foo", "HelloWorldFoo")] // mixed separators
    [InlineData("HTTPServer", "HTTPServer")] // acronym
    [InlineData("HTTP_server", "HttpServer")] // acronym with separator
    [InlineData("utf8String", "Utf8String")] // digits
    [InlineData("UTF8String", "Utf8String")] // uppercase digits
    [InlineData("IOError", "IoError")] // common acronym
    [InlineData("XMLHttpRequest", "XmlHttpRequest")] // multiple acronyms
    [InlineData("getHTTPResponseCode", "GetHttpResponseCode")] // mixed case
    public void ToPascalCase_HandlesEdgeCases_ReturnsExpected(string input, string expected)
    {
        // Act
        var result = input.ToPascalCase();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    public void ToPascalCase_EmptyOrWhitespace_ReturnsInput(string input)
    {
        // Act
        var result = input.ToPascalCase();

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void ToPascalCase_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullInput = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullInput.ToPascalCase());
    }

    #endregion

    #region ToCamelCase Edge Cases

    [Theory]
    [InlineData("hello_world", "helloWorld")]
    [InlineData("hello_world_foo", "helloWorldFoo")]
    [InlineData("hello__world", "helloWorld")] // consecutive separators
    [InlineData("hello--world", "helloWorld")] // consecutive separators
    [InlineData("_hello_world", "helloWorld")] // leading separator
    [InlineData("hello_world_", "helloWorld")] // trailing separator
    [InlineData("hello-world foo", "helloWorldFoo")] // mixed separators
    [InlineData("HTTPServer", "hTTPServer")] // acronym
    [InlineData("HTTP_server", "httpServer")] // acronym with separator
    [InlineData("utf8String", "utf8String")] // already camelCase
    [InlineData("HelloWorld", "helloWorld")] // already PascalCase
    public void ToCamelCase_HandlesEdgeCases_ReturnsExpected(string input, string expected)
    {
        // Act
        var result = input.ToCamelCase();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    public void ToCamelCase_EmptyOrWhitespace_ReturnsInput(string input)
    {
        // Act
        var result = input.ToCamelCase();

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void ToCamelCase_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullInput = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullInput.ToCamelCase());
    }

    #endregion

    #region ToSnakeCase Edge Cases

    [Theory]
    [InlineData("HelloWorld", "hello_world")]
    [InlineData("HelloWorldFoo", "hello_world_foo")]
    [InlineData("AnalysisService", "analysis_service")]
    [InlineData("IOError", "io_error")] // acronym
    [InlineData("XMLHttpRequest", "xml_http_request")] // multiple acronyms
    [InlineData("utf8String", "utf8_string")] // digits
    [InlineData("UTF8String", "utf8_string")] // uppercase digits
    [InlineData("hello_world", "hello_world")] // already snake_case
    [InlineData("hello__world", "hello__world")] // consecutive separators (preserved)
    [InlineData("__hello__world__", "__hello__world__")] // leading/trailing (preserved)
    [InlineData("hello-world", "hello_world")] // convert hyphens to underscores
    [InlineData("hello world", "hello_world")] // convert spaces to underscores
    public void ToSnakeCase_HandlesEdgeCases_ReturnsExpected(string input, string expected)
    {
        // Act
        var result = input.ToSnakeCase();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    public void ToSnakeCase_EmptyOrWhitespace_ReturnsInput(string input)
    {
        // Act
        var result = input.ToSnakeCase();

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void ToSnakeCase_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullInput = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullInput.ToSnakeCase());
    }

    #endregion

    #region ToKebabCase Edge Cases

    [Theory]
    [InlineData("HelloWorld", "hello-world")]
    [InlineData("HelloWorldFoo", "hello-world-foo")]
    [InlineData("AnalysisService", "analysis-service")]
    [InlineData("IOError", "io-error")] // acronym
    [InlineData("XMLHttpRequest", "xml-http-request")] // multiple acronyms
    [InlineData("utf8String", "utf8-string")] // digits
    [InlineData("UTF8String", "utf8-string")] // uppercase digits
    [InlineData("hello-world", "hello-world")] // already kebab-case
    [InlineData("hello--world", "hello-world")] // consecutive separators
    [InlineData("__hello__world__", "__hello__world__")] // leading/trailing (preserved)
    [InlineData("hello_world", "hello-world")] // convert underscores to hyphens
    [InlineData("hello world", "hello-world")] // convert spaces to hyphens
    public void ToKebabCase_HandlesEdgeCases_ReturnsExpected(string input, string expected)
    {
        // Act
        var result = input.ToKebabCase();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    public void ToKebabCase_EmptyOrWhitespace_ReturnsInput(string input)
    {
        // Act
        var result = input.ToKebabCase();

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void ToKebabCase_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullInput = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullInput.ToKebabCase());
    }

    #endregion

    #region Round-trip Tests

    [Theory]
    [InlineData("hello_world")]
    [InlineData("hello_world_foo")]
    [InlineData("hello_world_foo_bar")]
    [InlineData("analysis_service")]
    [InlineData("io_error")]
    [InlineData("xml_http_request")]
    [InlineData("utf8_string")]
    [InlineData("get_http_response_code")]
    public void ToPascalCase_ToSnakeCase_RoundTripPreservesMeaning(string snakeCaseInput)
    {
        // Arrange
        var pascalCase = snakeCaseInput.ToPascalCase();

        // Act
        var roundTrip = pascalCase.ToSnakeCase();

        // Assert - round trip should produce equivalent snake_case
        roundTrip.Should().Be(snakeCaseInput);
    }

    [Theory]
    [InlineData("HelloWorld")]
    [InlineData("HelloWorldFoo")]
    [InlineData("AnalysisService")]
    [InlineData("IOError")]
    [InlineData("XMLHttpRequest")]
    [InlineData("utf8String")]
    [InlineData("GetHttpResponseCode")]
    public void ToSnakeCase_ToPascalCase_RoundTripPreservesMeaning(string pascalCaseInput)
    {
        // Arrange
        var snakeCase = pascalCaseInput.ToSnakeCase();

        // Act
        var roundTrip = snakeCase.ToPascalCase();

        // Assert - round trip should produce equivalent PascalCase
        roundTrip.Should().Be(pascalCaseInput);
    }

    #endregion

    #region Culture Sensitivity Tests

    [Fact]
    public void ToPascalCase_CultureInvariant_HandlesTurkishI()
    {
        // Save original culture
        var originalCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

        try
        {
            // Set Turkish culture which has special handling for 'i' and 'I'
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");

            // Act
            var result = "istanbul".ToPascalCase();

            // Assert - should still work correctly with invariant casing
            result.Should().Be("Istanbul");
        }
        finally
        {
            // Restore original culture
            System.Threading.Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void ToSnakeCase_CultureInvariant_HandlesTurkishI()
    {
        // Save original culture
        var originalCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

        try
        {
            // Set Turkish culture which has special handling for 'i' and 'I'
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");

            // Act
            var result = "Istanbul".ToSnakeCase();

            // Assert - should still work correctly with invariant casing
            result.Should().Be("istanbul");
        }
        finally
        {
            // Restore original culture
            System.Threading.Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    #endregion
}