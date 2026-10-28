#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="BaselineService"/> covering load/save round trips, filtering and creation.
/// </summary>
public sealed class BaselineServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "baseline-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _ = new BaselineService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("../secret.json")]
    public async Task LoadBaselineAsync_BlankOrTraversalPath_ThrowsArgumentException(string? path)
    {
        // Arrange
        var service = CreateService();

        // Act
        Func<Task> act = () => service.LoadBaselineAsync(path!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task LoadBaselineAsync_MissingOrMalformedFile_ReturnsNull()
    {
        // Arrange
        var service = CreateService();
        Directory.CreateDirectory(_root);
        var malformedPath = Path.Combine(_root, "malformed.json");
        await File.WriteAllTextAsync(malformedPath, "{ not json");

        // Act
        var missing = await service.LoadBaselineAsync(Path.Combine(_root, "missing.json"));
        var malformed = await service.LoadBaselineAsync(malformedPath);

        // Assert
        missing.Should().BeNull();
        malformed.Should().BeNull();
    }

    [Fact]
    public async Task SaveAndLoadBaselineAsync_RoundTrip_CreatesMissingDirectoryAndPreservesViolations()
    {
        // Arrange
        var service = CreateService();
        var violation = CreateViolation("ASY001", "src/Async/Worker.cs");
        var source = service.CreateBaseline("Proj", new List<RuleViolation> { violation });
        var path = Path.Combine(_root, "nested", "baseline.json");

        // Act
        await service.SaveBaselineAsync(source, path);
        var loaded = await service.LoadBaselineAsync(path);

        // Assert
        File.Exists(path).Should().BeTrue();
        loaded.Should().NotBeNull();
        loaded!.ProjectName.Should().Be("Proj");
        loaded.ViolationCount.Should().Be(1);
        loaded.Contains(violation).Should().BeTrue();
    }

    [Fact]
    public async Task SaveBaselineAsync_NullBaseline_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateService();

        // Act
        Func<Task> act = () => service.SaveBaselineAsync(null!, Path.Combine(_root, "baseline.json"));

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("baseline");
    }

    [Fact]
    public void FilterNewViolations_ReturnsOnlyViolationsNotInBaseline()
    {
        // Arrange
        var service = CreateService();
        var known = CreateViolation("LYR001", "src/Services/Known.cs");
        var fresh = CreateViolation("NAM001", "src/Services/Fresh.cs");
        var baseline = service.CreateBaseline("Proj", new List<RuleViolation> { known });
        var violations = new List<RuleViolation> { known, fresh };

        // Act
        var filtered = service.FilterNewViolations(violations, baseline);
        var unfiltered = service.FilterNewViolations(violations, null);
        var emptyInput = service.FilterNewViolations(new List<RuleViolation>(), baseline);

        // Assert
        filtered.Should().ContainSingle().Which.Should().BeSameAs(fresh);
        unfiltered.Should().HaveCount(2);
        emptyInput.Should().BeEmpty();
    }

    [Fact]
    public void CreateBaseline_FromResultAndFromViolations_CopiesProjectNameAndViolations()
    {
        // Arrange
        var service = CreateService();
        var result = new AnalysisResult("Proj", "/repo/Proj");
        result.Violations.Add(CreateViolation("LYR001", "src/A.cs"));
        result.Violations.Add(CreateViolation("NAM001", "src/B.cs"));

        // Act
        var fromResult = service.CreateBaseline(result);
        var fromViolations = service.CreateBaseline("Empty", new List<RuleViolation>());

        // Assert
        fromResult.ProjectName.Should().Be("Proj");
        fromResult.ViolationCount.Should().Be(2);
        fromViolations.ProjectName.Should().Be("Empty");
        fromViolations.ViolationCount.Should().Be(0);
    }

    [Fact]
    public void CreateBaseline_InvalidArguments_ThrowsExpectedExceptions()
    {
        // Arrange
        var service = CreateService();

        // Act
        Action nullResult = () => service.CreateBaseline((AnalysisResult)null!);
        Action blankName = () => service.CreateBaseline("   ", new List<RuleViolation>());
        Action nullViolations = () => service.CreateBaseline("Proj", null!);

        // Assert
        nullResult.Should().Throw<ArgumentNullException>().WithParameterName("result");
        blankName.Should().Throw<ArgumentException>().WithParameterName("projectName");
        nullViolations.Should().Throw<ArgumentNullException>().WithParameterName("violations");
    }

    private BaselineService CreateService() => new(Substitute.For<ILogger<BaselineService>>());

    private static RuleViolation CreateViolation(string ruleId, string filePath) =>
        new(ruleId, "Rule " + ruleId, "Message for " + ruleId, filePath)
        {
            LineNumber = 10,
            CodeSnippet = "var x = 1;"
        };
}
