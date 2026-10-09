using System;
using System.Collections.Generic;
using FluentAssertions;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Formatters;
using Xunit;

namespace RoslynGuardAnalyzer.Tests.Formatters;

public class SarifFormatterTests
{
    private readonly SarifFormatter _formatter = new SarifFormatter();

    [Fact]
    public void FormatResult_ThrowsArgumentNullException_WhenResultIsNull()
    {
        // Act
        Action act = () => _formatter.FormatResult(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("result");
    }

    [Fact]
    public void FormatViolations_ThrowsArgumentNullException_WhenViolationsIsNull()
    {
        // Act
        Action act = () => _formatter.FormatViolations(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("violations");
    }

    [Fact]
    public void FormatReport_ThrowsArgumentNullException_WhenReportIsNull()
    {
        // Act
        Action act = () => _formatter.FormatReport(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("report");
    }
}