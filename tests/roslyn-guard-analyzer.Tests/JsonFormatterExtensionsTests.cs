// tests/roslyn-guard-analyzer.Tests/JsonFormatterExtensionsTests.cs

using Xunit;
using RoslynGuardAnalyzer.Formatters;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Tests
{
    public class JsonFormatterExtensionsTests
    {
        [Fact]
        public void FormatViolation_HappyPath_ReturnsJson()
        {
            // Arrange
            var formatter = new JsonFormatter();
            var violation = new RuleViolation { RuleId = "rule1", RuleName = "rule1", Message = "message1", FilePath = "path1" };

            // Act
            var result = JsonFormatterExtensions.FormatViolation(formatter, violation);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("{\"ProjectName\":\"", result);
        }

        [Fact]
        public void FormatViolation_NullFormatter_ThrowsArgumentNullException()
        {
            // Arrange
            var violation = new RuleViolation { RuleId = "rule1", RuleName = "rule1", Message = "message1", FilePath = "path1" };

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => JsonFormatterExtensions.FormatViolation(null, violation));
        }

        [Fact]
        public void FormatViolationsBySeverity_HappyPath_ReturnsJson()
        {
            // Arrange
            var formatter = new JsonFormatter();
            var violations = new List<RuleViolation>
            {
                new RuleViolation { RuleId = "rule1", RuleName = "rule1", Message = "message1", FilePath = "path1" },
                new RuleViolation { RuleId = "rule2", RuleName = "rule2", Message = "message2", FilePath = "path2" }
            };

            // Act
            var result = JsonFormatterExtensions.FormatViolationsBySeverity(formatter, violations);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("{\"ProjectName\":\"Violations Analysis\",\"ProjectPath\":\"N/A\",\"AnalysisSucceeded\":true,\"ErrorMessage\":null,\"TotalFilesAnalyzed\":1,\"TotalElementsAnalyzed\":2,\"Violations\":[", result);
        }

        [Fact]
        public void FormatViolationsBySeverity_NullFormatter_ThrowsArgumentNullException()
        {
            // Arrange
            var violations = new List<RuleViolation>
            {
                new RuleViolation { RuleId = "rule1", RuleName = "rule1", Message = "message1", FilePath = "path1" },
                new RuleViolation { RuleId = "rule2", RuleName = "rule2", Message = "message2", FilePath = "path2" }
            };

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => JsonFormatterExtensions.FormatViolationsBySeverity(null, violations));
        }

        [Fact]
        public void FormatViolationsByRule_HappyPath_ReturnsJson()
        {
            // Arrange
            var formatter = new JsonFormatter();
            var violations = new List<RuleViolation>
            {
                new RuleViolation { RuleId = "rule1", RuleName = "rule1", Message = "message1", FilePath = "path1" },
                new RuleViolation { RuleId = "rule2", RuleName = "rule2", Message = "message2", FilePath = "path2" }
            };

            // Act
            var result = JsonFormatterExtensions.FormatViolationsByRule(formatter, violations, "rule1");

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("{\"ProjectName\":\"Rule rule1 Analysis\",\"ProjectPath\":\"N/A\",\"AnalysisSucceeded\":true,\"ErrorMessage\":null,\"TotalFilesAnalyzed\":1,\"TotalElementsAnalyzed\":1,\"Violations\":[", result);
        }

        [Fact]
        public void FormatViolationsByRule_NullFormatter_ThrowsArgumentNullException()
        {
            // Arrange
            var violations = new List<RuleViolation>
            {
                new RuleViolation { RuleId = "rule1", RuleName = "rule1", Message = "message1", FilePath = "path1" },
                new RuleViolation { RuleId = "rule2", RuleName = "rule2", Message = "message2", FilePath = "path2" }
            };

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => JsonFormatterExtensions.FormatViolationsByRule(null, violations, "rule1"));
        }

        [Fact]
        public void FormatViolationsByRule_NullViolations_ThrowsArgumentNullException()
        {
            // Arrange
            var formatter = new JsonFormatter();
            var ruleId = "rule1";

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => JsonFormatterExtensions.FormatViolationsByRule(formatter, null, ruleId));
        }

        [Fact]
        public void FormatViolationsByRule_NullRuleId_ThrowsArgumentException()
        {
            // Arrange
            var formatter = new JsonFormatter();
            var violations = new List<RuleViolation>
            {
                new RuleViolation { RuleId = "rule1", RuleName = "rule1", Message = "message1", FilePath = "path1" },
                new RuleViolation { RuleId = "rule2", RuleName = "rule2", Message = "message2", FilePath = "path2" }
            };

            // Act and Assert
            Assert.Throws<ArgumentException>(() => JsonFormatterExtensions.FormatViolationsByRule(formatter, violations, null));
        }

        [Fact]
        public void FormatViolationSummary_HappyPath_ReturnsJson()
        {
            // Arrange
            var formatter = new JsonFormatter();
            var violations = new List<RuleViolation>
            {
                new RuleViolation { RuleId = "rule1", RuleName = "rule1", Message = "message1", FilePath = "path1" },
                new RuleViolation { RuleId = "rule2", RuleName = "rule2", Message = "message2", FilePath = "path2" }
            };

            // Act
            var result = JsonFormatterExtensions.FormatViolationSummary(formatter, violations);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("{\"ProjectName\":\"Summary\",\"ProjectPath\":\"N/A\",\"AnalysisSucceeded\":true,\"ErrorMessage\":null,\"TotalFilesAnalyzed\":1,\"TotalElementsAnalyzed\":2}", result);
        }

        [Fact]
        public void FormatViolationSummary_NullFormatter_ThrowsArgumentNullException()
        {
            // Arrange
            var violations = new List<RuleViolation>
            {
                new RuleViolation { RuleId = "rule1", RuleName = "rule1", Message = "message1", FilePath = "path1" },
                new RuleViolation { RuleId = "rule2", RuleName = "rule2", Message = "message2", FilePath = "path2" }
            };

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => JsonFormatterExtensions.FormatViolationSummary(null, violations));
        }
    }
}
