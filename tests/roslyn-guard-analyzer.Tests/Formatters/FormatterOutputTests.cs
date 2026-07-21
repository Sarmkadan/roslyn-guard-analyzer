#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Formatters;
using Xunit;

namespace RoslynGuardAnalyzer.Tests.Formatters;

/// <summary>
/// Tests for output formatters (CSV, JSON, HTML) to ensure they produce valid output.
/// </summary>
public sealed class FormatterOutputTests
{
    private static readonly List<RuleViolation> TestViolations = new()
    {
        new RuleViolation("RULE001", "LayerViolation", "Namespace 'Data' violates layer dependency rule", "/src/Data/Repository.cs")
        {
            Severity = SeverityLevel.Error,
            LineNumber = 42,
            ColumnNumber = 15,
            CodeSnippet = "public class Repository { }",
            ProjectName = "TestProject"
        },
        new RuleViolation("RULE002", "NamingConvention", "Class name 'myClass' should be PascalCase", "/src/Utilities/Helper.cs")
        {
            Severity = SeverityLevel.Warning,
            LineNumber = 23,
            ColumnNumber = 8,
            CodeSnippet = "public class myClass { }",
            ProjectName = "TestProject"
        },
        new RuleViolation("RULE003", "NullReference", "Possible null reference exception", "/src/Core/Service.cs")
        {
            Severity = SeverityLevel.Critical,
            LineNumber = 117,
            ColumnNumber = 22,
            CodeSnippet = "return service.GetById(id);",
            ProjectName = "TestProject"
        },
        new RuleViolation("RULE004", "AsyncVoidWarning", "Async void method should be avoided", "/src/Api/Controllers/UserController.cs")
        {
            Severity = SeverityLevel.Info,
            LineNumber = 89,
            ColumnNumber = 5,
            CodeSnippet = "public async void UpdateUser() { }",
            ProjectName = "TestProject"
        }
    };

    private static readonly AnalysisResult TestAnalysisResult = new("TestProject", "/src/TestProject")
    {
        Violations = TestViolations,
        TotalFilesAnalyzed = 15,
        TotalElementsAnalyzed = 247,
        AnalysisSucceeded = true
    };

    private static readonly ViolationReport TestReport = new("Architecture Analysis Report", "TestProject")
    {
        GeneratedAt = DateTime.UtcNow,
        Summary = "Analysis completed successfully",
        DetailedContent = "Detailed analysis of project architecture"
    };

    [Fact]
    public void CsvFormatter_Format_ReturnsNonEmptyString()
    {
        // Arrange
        var formatter = new CsvFormatter();

        // Act
        var result = formatter.FormatResult(TestAnalysisResult);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Rule,Severity,Message,File,Line,Column,Code");
    }

    [Fact]
    public void CsvFormatter_FormatViolations_ReturnsValidCsv()
    {
        // Arrange
        var formatter = new CsvFormatter();

        // Act
        var result = formatter.FormatViolations(TestViolations);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var lines = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(TestViolations.Count + 1); // Header + violations
        lines[0].Should().Be("Rule,Severity,Message,File,Line,Column,Code");
    }

    [Fact]
    public void CsvFormatter_CanFormat_ReturnsTrueForCsv()
    {
        // Arrange
        var formatter = new CsvFormatter();

        // Act
        var canFormat = formatter.CanFormat("csv");

        // Assert
        canFormat.Should().BeTrue();
    }

    [Fact]
    public void CsvFormatter_CanFormat_CaseInsensitive()
    {
        // Arrange
        var formatter = new CsvFormatter();

        // Act & Assert
        formatter.CanFormat("CSV").Should().BeTrue();
        formatter.CanFormat("Csv").Should().BeTrue();
        formatter.CanFormat("CsV").Should().BeTrue();
    }

    [Fact]
    public void CsvFormatter_HandlesCommasInMessage()
    {
        // Arrange
        var violationWithComma = new RuleViolation("RULE005", "CommaTest", "This message contains, a comma", "/src/Test/File.cs")
        {
            Severity = SeverityLevel.Error,
            LineNumber = 1,
            ColumnNumber = 1
        };
        var violations = new List<RuleViolation> { violationWithComma };
        var formatter = new CsvFormatter();

        // Act
        var result = formatter.FormatViolations(violations);

        // Assert
        result.Should().Contain("\"This message contains, a comma\""); // Should be quoted
        result.Should().Contain("CommaTest,Error,\"This message contains, a comma\",/src/Test/File.cs,1,1,N/A");
    }

    [Fact]
    public void CsvFormatter_HandlesQuotesInMessage()
    {
        // Arrange
        var violationWithQuote = new RuleViolation("RULE006", "QuoteTest", "Message with \"quotes\" inside", "/src/Test/File.cs")
        {
            Severity = SeverityLevel.Warning,
            LineNumber = 2,
            ColumnNumber = 1
        };
        var violations = new List<RuleViolation> { violationWithQuote };
        var formatter = new CsvFormatter();

        // Act
        var result = formatter.FormatViolations(violations);

        // Assert
        result.Should().Contain("\"Message with \""); // Should contain escaped quotes in the message
        result.Should().Contain("QuoteTest,Warning,\"Message with");
    }

    [Fact]
    public void CsvFormatter_HandlesNewlinesInMessage()
    {
        // Arrange
        var violationWithNewline = new RuleViolation("RULE007", "NewlineTest", "Message with\nnewline", "/src/Test/File.cs")
        {
            Severity = SeverityLevel.Info,
            LineNumber = 3,
            ColumnNumber = 1
        };
        var violations = new List<RuleViolation> { violationWithNewline };
        var formatter = new CsvFormatter();

        // Act
        var result = formatter.FormatViolations(violations);

        // Assert
        result.Should().Contain("\"Message with\nnewline\""); // Should be quoted
    }

    [Fact]
    public void CsvFormatter_FormatReport_IncludesSummaryStatistics()
    {
        // Arrange
        var formatter = new CsvFormatter();
        TestReport.ViolationGroups = new List<ViolationGroup> { new() { Violations = TestViolations } };

        // Act
        var result = formatter.FormatReport(TestReport);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("SUMMARY");
        result.Should().Contain("Total Violations,");
        result.Should().Contain("SEVERITY SUMMARY");
        result.Should().Contain("VIOLATIONS BY RULE");
        result.Should().Contain("DETAILED VIOLATIONS");
    }

    [Fact]
    public void JsonFormatter_Format_ReturnsValidJson()
    {
        // Arrange
        var formatter = new JsonFormatter();

        // Act
        var result = formatter.FormatResult(TestAnalysisResult);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("{");
        result.Should().EndWith("}");
    }

    [Fact]
    public void JsonFormatter_FormatViolations_ReturnsParseableJson()
    {
        // Arrange
        var formatter = new JsonFormatter();

        // Act
        var result = formatter.FormatViolations(TestViolations);

        // Assert
        result.Should().NotBeNullOrEmpty();

        // Verify it's valid JSON by parsing it
        var parseResult = System.Text.Json.JsonDocument.Parse(result);
        var root = parseResult.RootElement;
        root.TryGetProperty("Count", out var countProp);
        countProp.GetInt32().Should().Be(TestViolations.Count);

        root.TryGetProperty("Violations", out var violationsProp);
        violationsProp.GetArrayLength().Should().Be(TestViolations.Count);
    }

    [Fact]
    public void JsonFormatter_CanFormat_ReturnsTrueForJson()
    {
        // Arrange
        var formatter = new JsonFormatter();

        // Act
        var canFormat = formatter.CanFormat("json");

        // Assert
        canFormat.Should().BeTrue();
    }

    [Fact]
    public void JsonFormatter_CanFormat_CaseInsensitive()
    {
        // Arrange
        var formatter = new JsonFormatter();

        // Act & Assert
        formatter.CanFormat("JSON").Should().BeTrue();
        formatter.CanFormat("Json").Should().BeTrue();
        formatter.CanFormat("JsOn").Should().BeTrue();
    }

    [Fact]
    public void JsonFormatter_FormatReport_IncludesAllRequiredFields()
    {
        // Arrange
        var formatter = new JsonFormatter();
        TestReport.ViolationGroups = new List<ViolationGroup> { new() { Violations = TestViolations } };

        // Act
        var result = formatter.FormatReport(TestReport);

        // Assert
        result.Should().NotBeNullOrEmpty();

        // Verify it's valid JSON
        var parseResult = System.Text.Json.JsonDocument.Parse(result);
        var root = parseResult.RootElement;
        root.TryGetProperty("Title", out var titleProp);
        root.TryGetProperty("ProjectName", out var projectNameProp);
        root.TryGetProperty("TotalViolations", out var totalViolationsProp);
        root.TryGetProperty("SeveritySummary", out var severitySummaryProp);
        root.TryGetProperty("ViolationsByRule", out var violationsByRuleProp);

        titleProp.GetString().Should().Be("Architecture Analysis Report");
        projectNameProp.GetString().Should().Be("TestProject");
        totalViolationsProp.GetInt32().Should().Be(TestViolations.Count);
        severitySummaryProp.GetProperty("Critical").GetInt32().Should().Be(1);
        severitySummaryProp.GetProperty("High").GetInt32().Should().Be(1);
        severitySummaryProp.GetProperty("Medium").GetInt32().Should().Be(1);
        severitySummaryProp.GetProperty("Low").GetInt32().Should().Be(1);
    }

    [Fact]
    public void JsonFormatter_HandlesEmptyViolations()
    {
        // Arrange
        var emptyResult = new AnalysisResult("EmptyProject", "/src/EmptyProject") { Violations = new List<RuleViolation>() };
        var formatter = new JsonFormatter();

        // Act
        var result = formatter.FormatResult(emptyResult);

        // Assert
        result.Should().NotBeNullOrEmpty();

        // Verify it's valid JSON
        var parseResult = System.Text.Json.JsonDocument.Parse(result);
        var root = parseResult.RootElement;
        root.TryGetProperty("ViolationCount", out var violationCountProp);
        violationCountProp.GetInt32().Should().Be(0);
    }

    [Fact]
    public void HtmlFormatter_Format_ReturnsValidHtml()
    {
        // Arrange
        var formatter = new HtmlFormatter();

        // Act
        var result = formatter.FormatResult(TestAnalysisResult);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("<!DOCTYPE html>");
        result.Should().Contain("<html>");
        result.Should().Contain("</html>");
    }

    [Fact]
    public void HtmlFormatter_FormatViolations_ReturnsHtmlWithTable()
    {
        // Arrange
        var formatter = new HtmlFormatter();

        // Act
        var result = formatter.FormatViolations(TestViolations);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("<table");
        result.Should().Contain("</table>");
        result.Should().Contain("<tbody>");
        result.Should().Contain("</tbody>");
    }

    [Fact]
    public void HtmlFormatter_CanFormat_ReturnsTrueForHtml()
    {
        // Arrange
        var formatter = new HtmlFormatter();

        // Act
        var canFormat = formatter.CanFormat("html");

        // Assert
        canFormat.Should().BeTrue();
    }

    [Fact]
    public void HtmlFormatter_CanFormat_CaseInsensitive()
    {
        // Arrange
        var formatter = new HtmlFormatter();

        // Act & Assert
        formatter.CanFormat("HTML").Should().BeTrue();
        formatter.CanFormat("Html").Should().BeTrue();
        formatter.CanFormat("HtMl").Should().BeTrue();
    }

    [Fact]
    public void HtmlFormatter_ContainsDiagnosticIds()
    {
        // Arrange
        var formatter = new HtmlFormatter();

        // Act
        var result = formatter.FormatViolations(TestViolations);

        // Assert
        result.Should().Contain("LayerViolation");
        result.Should().Contain("NamingConvention");
        result.Should().Contain("NullReference");
        result.Should().Contain("AsyncVoidWarning");
    }

    [Fact]
    public void HtmlFormatter_HandlesHtmlSpecialCharacters()
    {
        // Arrange
        var violationWithHtml = new RuleViolation("RULE008", "HtmlTest", "Message with <script>alert('xss')</script>", "/src/Test/File.cs")
        {
            Severity = SeverityLevel.Error,
            LineNumber = 1
        };
        var violations = new List<RuleViolation> { violationWithHtml };
        var formatter = new HtmlFormatter();

        // Act
        var result = formatter.FormatViolations(violations);

        // Assert
        result.Should().NotContain("<script>"); // Should be escaped
        result.Should().Contain("&lt;script&gt;"); // Should be HTML escaped
    }

    [Fact]
    public void HtmlFormatter_FormatReport_ReturnsCompleteHtmlDocument()
    {
        // Arrange
        var formatter = new HtmlFormatter();
        TestReport.ViolationGroups = new List<ViolationGroup> { new() { Violations = TestViolations } };

        // Act
        var result = formatter.FormatReport(TestReport);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("<!DOCTYPE html>");
        result.Should().Contain("<title>");
        result.Should().Contain("Roslyn Guard Analyzer Report");
        result.Should().Contain("Total Violations");
        result.Should().Contain("Affected Files");
    }

    [Fact]
    public void HtmlFormatter_EmptyViolations_ReturnsSuccessMessage()
    {
        // Arrange
        var emptyResult = new AnalysisResult("EmptyProject", "/src/EmptyProject") { Violations = new List<RuleViolation>() };
        var formatter = new HtmlFormatter();

        // Act
        var result = formatter.FormatResult(emptyResult);

        // Assert
        result.Should().Contain("✓ No violations found");
    }

    [Fact]
    public void AllFormatters_ImplementIOutputFormatter()
    {
        // Arrange
        var csvFormatter = new CsvFormatter();
        var jsonFormatter = new JsonFormatter();
        var htmlFormatter = new HtmlFormatter();

        // Act & Assert - all should implement the interface
        csvFormatter.Should().BeAssignableTo<IOutputFormatter>();
        jsonFormatter.Should().BeAssignableTo<IOutputFormatter>();
        htmlFormatter.Should().BeAssignableTo<IOutputFormatter>();
    }

    [Fact]
    public void AllFormatters_HaveUniqueFormatNames()
    {
        // Arrange
        var csvFormatter = new CsvFormatter();
        var jsonFormatter = new JsonFormatter();
        var htmlFormatter = new HtmlFormatter();

        // Act & Assert
        csvFormatter.Format.Should().Be("csv");
        jsonFormatter.Format.Should().Be("json");
        htmlFormatter.Format.Should().Be("html");

        // Ensure they're all different
        csvFormatter.Format.Should().NotBe(jsonFormatter.Format);
        csvFormatter.Format.Should().NotBe(htmlFormatter.Format);
        jsonFormatter.Format.Should().NotBe(htmlFormatter.Format);
    }
}