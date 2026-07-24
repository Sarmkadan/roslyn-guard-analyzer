#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Suppressions;
using Xunit;

/// <summary>
/// Tests for SuppressionManager conflict resolution and expiry logic.
/// Tests scenarios where multiple suppression records target the same violation,
/// expiry date handling, and graceful degradation when rules don't exist.
/// </summary>
public sealed class SuppressionManagerConflictAndExpiryTests
{
    private readonly ILogger<SuppressionManager> _mockLogger;

    public SuppressionManagerConflictAndExpiryTests()
    {
        _mockLogger = Substitute.For<ILogger<SuppressionManager>>();
    }

    /// <summary>
    /// Tests that when two suppression records target the same rule ID and file,
    /// the one with the more specific target element takes precedence.
    /// </summary>
    [Fact]
    public void IsSuppressed_TwoRecordsSameRuleAndFile_ElementSpecificityWins()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var violation = new RuleViolation("RG001", "Test rule", "Test message", "/src/File.cs")
        {
            LineNumber = 42,
            ColumnNumber = 1,
            Severity = SeverityLevel.Warning
        };
        violation.AddMetadata("ElementName", "SpecificMethod");

        // Add a general suppression for the entire file
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "General suppression",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        // Add a specific suppression for a particular element
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            TargetElement = "SpecificMethod",
            Justification = "Specific suppression",
            Author = "tester",
            CreatedAt = DateTime.UtcNow.AddMinutes(1),
            IsActive = true
        });

        // Act
        var isSuppressed = manager.IsSuppressed(violation);

        // Assert - both should match, but the specific one should be found
        isSuppressed.Should().BeTrue();
        manager.GetSuppressions(violation.RuleId).Should().HaveCount(2);
    }

    /// <summary>
    /// Tests that when two suppression records target the same rule ID and file,
    /// only the one that matches the element name actually suppresses.
    /// </summary>
    [Fact]
    public void IsSuppressed_TwoRecordsSameRuleAndFile_OnlyMatchingElementIsSuppressed()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var violation = new RuleViolation("RG002", "Test rule", "Test message", "/src/File.cs")
        {
            LineNumber = 42,
            ColumnNumber = 1,
            Severity = SeverityLevel.Warning
        };
        violation.AddMetadata("ElementName", "MethodA");

        // Add suppression for MethodA
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            TargetElement = "MethodA",
            Justification = "Suppression for MethodA",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        // Add suppression for MethodB (different element)
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            TargetElement = "MethodB",
            Justification = "Suppression for MethodB",
            Author = "tester",
            CreatedAt = DateTime.UtcNow.AddMinutes(1),
            IsActive = true
        });

        // Act
        var isSuppressed = manager.IsSuppressed(violation);

        // Assert - only the suppression for MethodA should match
        isSuppressed.Should().BeTrue();
        var methodBViolation = new RuleViolation("RG002", "Test rule", "Test message", "/src/File.cs")
        {
            LineNumber = 42,
            ColumnNumber = 1,
            Severity = SeverityLevel.Warning
        };
        methodBViolation.AddMetadata("ElementName", "MethodB");
        manager.IsSuppressed(methodBViolation).Should().BeTrue();
    }

    /// <summary>
    /// Tests that when two suppression records target the same rule ID and file with no element specificity,
    /// both records match the violation.
    /// </summary>
    [Fact]
    public void IsSuppressed_TwoRecordsSameRuleAndFileNoElementSpecificity_BothMatch()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var violation = new RuleViolation("RG003", "Test rule", "Test message", "/src/File.cs")
        {
            LineNumber = 42,
            ColumnNumber = 1,
            Severity = SeverityLevel.Warning
        };

        // Add first suppression
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "First suppression",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        // Add second suppression
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "Second suppression",
            Author = "tester",
            CreatedAt = DateTime.UtcNow.AddMinutes(1),
            IsActive = true
        });

        // Act
        var isSuppressed = manager.IsSuppressed(violation);

        // Assert - both should match
        isSuppressed.Should().BeTrue();
    }

    /// <summary>
    /// Tests that a suppression record with an expiry date in the past is not applied.
    /// </summary>
    [Fact]
    public void IsSuppressed_ExpiredSuppression_ReturnsFalse()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var violation = new RuleViolation("RG004", "Test rule", "Test message", "/src/File.cs")
        {
            LineNumber = 42,
            ColumnNumber = 1,
            Severity = SeverityLevel.Warning
        };

        // Add an expired suppression
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "Expired suppression",
            Author = "tester",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired yesterday
            IsActive = true
        });

        // Act
        var isSuppressed = manager.IsSuppressed(violation);

        // Assert - should not be suppressed
        isSuppressed.Should().BeFalse();
    }

    /// <summary>
    /// Tests that a suppression record with an expiry date in the future is applied.
    /// </summary>
    [Fact]
    public void IsSuppressed_ActiveSuppressionWithFutureExpiry_ReturnsTrue()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var violation = new RuleViolation("RG005", "Test rule", "Test message", "/src/File.cs")
        {
            LineNumber = 42,
            ColumnNumber = 1,
            Severity = SeverityLevel.Warning
        };

        // Add an active suppression with future expiry
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "Active suppression",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30), // Expires in 30 days
            IsActive = true
        });

        // Act
        var isSuppressed = manager.IsSuppressed(violation);

        // Assert - should be suppressed
        isSuppressed.Should().BeTrue();
    }

    /// <summary>
    /// Tests that a suppression record with no expiry date is always applied.
    /// </summary>
    [Fact]
    public void IsSuppressed_ActiveSuppressionWithoutExpiry_ReturnsTrue()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var violation = new RuleViolation("RG006", "Test rule", "Test message", "/src/File.cs")
        {
            LineNumber = 42,
            ColumnNumber = 1,
            Severity = SeverityLevel.Warning
        };

        // Add an active suppression without expiry
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "Permanent suppression",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        // Act
        var isSuppressed = manager.IsSuppressed(violation);

        // Assert - should be suppressed
        isSuppressed.Should().BeTrue();
    }

    /// <summary>
    /// Tests that a violation that doesn't match any suppression record returns false.
    /// </summary>
    [Fact]
    public void IsSuppressed_NoMatchingRecord_ReturnsFalse()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var violation = new RuleViolation("RG007", "Test rule", "Test message", "/src/File.cs")
        {
            LineNumber = 42,
            ColumnNumber = 1,
            Severity = SeverityLevel.Warning
        };

        // Add a suppression for a different rule
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = "RG999", // Different rule ID
            TargetFile = violation.FilePath,
            Justification = "Different rule",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        // Act
        var isSuppressed = manager.IsSuppressed(violation);

        // Assert - should not be suppressed
        isSuppressed.Should().BeFalse();
    }

    /// <summary>
    /// Tests that FilterSuppressed returns violations that don't match any suppression.
    /// </summary>
    [Fact]
    public void FilterSuppressed_NoMatchingRecords_ReturnsAllViolations()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var suppressedViolation = new RuleViolation("RG008", "Test rule", "Test message", "/src/File.cs")
        {
            LineNumber = 42,
            ColumnNumber = 1,
            Severity = SeverityLevel.Warning
        };
        var nonSuppressedViolation = new RuleViolation("RG009", "Test rule 2", "Test message 2", "/src/File2.cs")
        {
            LineNumber = 84,
            ColumnNumber = 5,
            Severity = SeverityLevel.Error
        };

        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = suppressedViolation.RuleId,
            TargetFile = suppressedViolation.FilePath,
            Justification = "Suppression for first violation",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        // Act
        var remaining = manager.FilterSuppressed(new[] { suppressedViolation, nonSuppressedViolation });

        // Assert - only the non-suppressed violation should remain
        remaining.Should().HaveCount(1);
        remaining[0].RuleId.Should().Be(nonSuppressedViolation.RuleId);
    }

    /// <summary>
    /// Tests that expired suppressions are filtered out when loading from a file.
    /// </summary>
    [Fact]
    public async Task LoadAsync_ExpiredSuppression_NotLoaded()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var tempFile = System.IO.Path.GetTempFileName();

        try
        {
            // Create a suppression file with both expired and active records
            var expiredRecord = new SuppressionRecord
            {
                Id = Guid.NewGuid().ToString("N"),
                RuleId = "RG010",
                TargetFile = "/src/File.cs",
                Justification = "Expired record",
                Author = "tester",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
                IsActive = true
            };

            var activeRecord = new SuppressionRecord
            {
                Id = Guid.NewGuid().ToString("N"),
                RuleId = "RG011",
                TargetFile = "/src/File2.cs",
                Justification = "Active record",
                Author = "tester",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30), // Active
                IsActive = true
            };

            var records = new[] { expiredRecord, activeRecord };
            await File.WriteAllTextAsync(tempFile, System.Text.Json.JsonSerializer.Serialize(records, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

            // Act
            await manager.LoadAsync(tempFile);

            // Assert - only active record should be loaded
            var loadedRecords = manager.GetSuppressions();
            loadedRecords.Should().HaveCount(1);
            loadedRecords[0].RuleId.Should().Be("RG011");
            loadedRecords[0].Id.Should().Be(activeRecord.Id);
        }
        finally
        {
            if (System.IO.File.Exists(tempFile))
            {
                System.IO.File.Delete(tempFile);
            }
        }
    }

    /// <summary>
    /// Tests that a suppression record with IsActive=false is not applied.
    /// </summary>
    [Fact]
    public void IsSuppressed_InactiveSuppression_ReturnsFalse()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var violation = new RuleViolation("RG012", "Test rule", "Test message", "/src/File.cs")
        {
            LineNumber = 42,
            ColumnNumber = 1,
            Severity = SeverityLevel.Warning
        };

        // Add an inactive suppression
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "Inactive suppression",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = false // Inactive
        });

        // Act
        var isSuppressed = manager.IsSuppressed(violation);

        // Assert - should not be suppressed
        isSuppressed.Should().BeFalse();
    }

    /// <summary>
    /// Tests that multiple records with different expiry dates are handled correctly.
    /// </summary>
    [Fact]
    public void IsSuppressed_MultipleRecordsWithDifferentExpiryDates_OnlyActiveOnesApply()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var violation = new RuleViolation("RG013", "Test rule", "Test message", "/src/File.cs")
        {
            LineNumber = 42,
            ColumnNumber = 1,
            Severity = SeverityLevel.Warning
        };

        // Add expired suppression
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "Expired",
            Author = "tester",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(-5),
            IsActive = true
        });

        // Add active suppression with future expiry
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "Active",
            Author = "tester",
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            IsActive = true
        });

        // Add suppression without expiry
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "No expiry",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        // Add inactive suppression
        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "Inactive",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = false
        });

        // Act
        var isSuppressed = manager.IsSuppressed(violation);

        // Assert - should be suppressed (at least one active record matches)
        isSuppressed.Should().BeTrue();
    }

    /// <summary>
    /// Tests that FilterSuppressed correctly filters when some violations are suppressed and some are not.
    /// </summary>
    [Fact]
    public void FilterSuppressed_MixedSuppressedAndNonSuppressed_ReturnsOnlyNonSuppressed()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var suppressedViolation1 = new RuleViolation("RG014", "Test rule", "Test message", "/src/File1.cs")
        {
            LineNumber = 10,
            Severity = SeverityLevel.Warning
        };
        var suppressedViolation2 = new RuleViolation("RG015", "Test rule", "Test message", "/src/File2.cs")
        {
            LineNumber = 20,
            Severity = SeverityLevel.Error
        };
        var nonSuppressedViolation = new RuleViolation("RG016", "Test rule", "Test message", "/src/File3.cs")
        {
            LineNumber = 30,
            Severity = SeverityLevel.Warning
        };

        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = suppressedViolation1.RuleId,
            TargetFile = suppressedViolation1.FilePath,
            Justification = "Suppression 1",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = suppressedViolation2.RuleId,
            TargetFile = suppressedViolation2.FilePath,
            Justification = "Suppression 2",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        // Act
        var remaining = manager.FilterSuppressed(new[] { suppressedViolation1, suppressedViolation2, nonSuppressedViolation });

        // Assert - only the non-suppressed violation should remain
        remaining.Should().HaveCount(1);
        remaining[0].RuleId.Should().Be(nonSuppressedViolation.RuleId);
    }

    /// <summary>
    /// Tests that GetSuppressions returns records in creation order.
    /// </summary>
    [Fact]
    public void GetSuppressions_ReturnsRecordsInCreationOrder()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var violation = new RuleViolation("RG017", "Test rule", "Test message", "/src/File.cs")
        {
            LineNumber = 42,
            Severity = SeverityLevel.Warning
        };

        var record1 = new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "First record",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var record2 = new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "Second record",
            Author = "tester",
            CreatedAt = DateTime.UtcNow.AddMinutes(1),
            IsActive = true
        };

        var record3 = new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "Third record",
            Author = "tester",
            CreatedAt = DateTime.UtcNow.AddMinutes(2),
            IsActive = true
        };

        // Act
        manager.AddSuppression(record2);
        manager.AddSuppression(record1);
        manager.AddSuppression(record3);

        var suppressions = manager.GetSuppressions();

        // Assert - should be in creation order (record1, record2, record3)
        suppressions.Should().HaveCount(3);
        suppressions[0].Justification.Should().Be("First record");
        suppressions[1].Justification.Should().Be("Second record");
        suppressions[2].Justification.Should().Be("Third record");
    }

    /// <summary>
    /// Tests that RemoveSuppression removes the specified record.
    /// </summary>
    [Fact]
    public void RemoveSuppression_RemovesSpecifiedRecord()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var violation = new RuleViolation("RG018", "Test rule", "Test message", "/src/File.cs")
        {
            LineNumber = 42,
            Severity = SeverityLevel.Warning
        };

        var record1 = new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "Record 1",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var record2 = new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = "Record 2",
            Author = "tester",
            CreatedAt = DateTime.UtcNow.AddMinutes(1),
            IsActive = true
        };

        manager.AddSuppression(record1);
        manager.AddSuppression(record2);

        // Act
        var removed = manager.RemoveSuppression(record1.Id);

        // Assert
        removed.Should().BeTrue();
        manager.GetSuppressions().Should().HaveCount(1);
        manager.GetSuppressions()[0].Id.Should().Be(record2.Id);
        manager.IsSuppressed(violation).Should().BeTrue(); // record2 still matches
    }

    /// <summary>
    /// Tests that GetSuppressions with ruleId filter returns only matching records.
    /// </summary>
    [Fact]
    public void GetSuppressions_WithRuleIdFilter_ReturnsOnlyMatchingRecords()
    {
        // Arrange
        var manager = new SuppressionManager(_mockLogger);
        var violation1 = new RuleViolation("RG019", "Test rule", "Test message", "/src/File.cs")
        {
            LineNumber = 42,
            Severity = SeverityLevel.Warning
        };
        var violation2 = new RuleViolation("RG020", "Different rule", "Test message", "/src/File.cs")
        {
            LineNumber = 84,
            Severity = SeverityLevel.Error
        };

        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation1.RuleId,
            TargetFile = violation1.FilePath,
            Justification = "For violation1",
            Author = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        manager.AddSuppression(new SuppressionRecord
        {
            RuleId = violation2.RuleId,
            TargetFile = violation2.FilePath,
            Justification = "For violation2",
            Author = "tester",
            CreatedAt = DateTime.UtcNow.AddMinutes(1),
            IsActive = true
        });

        // Act
        var filtered = manager.GetSuppressions("RG019");

        // Assert - only records for RG019 should be returned
        filtered.Should().HaveCount(1);
        filtered[0].RuleId.Should().Be("RG019");
    }
}