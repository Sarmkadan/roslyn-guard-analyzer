#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using RoslynGuardAnalyzer.Rules;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Integration tests that run <see cref="BoundsCheckAnalyzer"/> with <c>guard.*</c> settings supplied through
/// <see cref="AnalyzerConfigOptionsProvider"/>, the same channel the compiler uses for <c>.editorconfig</c> values.
/// </summary>
public sealed class GuardOptionsConfigurationTests
{
    static GuardOptionsConfigurationTests()
    {
        // ReferenceAssemblies.Net.Net80 restores reference packages through NuGet, which throws
        // when HOME is unset (e.g. minimal CI or service environments). Fall back to the user profile.
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME")))
        {
            Environment.SetEnvironmentVariable("HOME", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }
    }

    private const string UnguardedNamespaceSource = """
        namespace App.Generated
        {
            class C
            {
                void M(int[] arr, int index)
                {
                    var x = arr[index];
                }
            }
        }
        """;

    // In the source below, the access sits six ancestor levels below the if statement that guards it:
    // EqualsValueClause, VariableDeclarator, VariableDeclaration, LocalDeclarationStatement, Block, IfStatement.
    private const string GuardedBySurroundingIfSource = """
        class C
        {
            void M(int[] arr, int i)
            {
                if (arr.Length > i)
                {
                    var x = arr[i];
                }
            }
        }
        """;

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source, params (string Key, string Value)[] settings)
    {
        var references = await ReferenceAssemblies.Net.Net80.ResolveAsync(LanguageNames.CSharp, CancellationToken.None);
        var tree = CSharpSyntaxTree.ParseText(source, path: "/0/Test0.cs");
        var compilation = CSharpCompilation.Create(
            "GuardOptionsTests",
            new[] { tree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var provider = new DictionaryConfigOptionsProvider(settings.ToDictionary(s => s.Key, s => s.Value));
        var analyzerOptions = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty, provider);
        var withAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(new BoundsCheckAnalyzer()),
            new CompilationWithAnalyzersOptions(analyzerOptions, onAnalyzerException: null, concurrentAnalysis: false, logAnalyzerExecutionTime: false));

        return await withAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    private static Diagnostic[] Ids(ImmutableArray<Diagnostic> diagnostics, string id) =>
        diagnostics.Where(d => d.Id == id).ToArray();

    // ==================== guard.ignored_namespaces ====================

    [Fact]
    public async Task IgnoredNamespaces_ExactNamespace_SuppressesDiagnostic()
    {
        // Arrange & Act
        var diagnostics = await AnalyzeAsync(UnguardedNamespaceSource, (GuardOptions.IgnoredNamespacesKey, "App.Generated"));

        // Assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task IgnoredNamespaces_ParentNamespace_SuppressesSubNamespaceDiagnostic()
    {
        // Arrange & Act
        var diagnostics = await AnalyzeAsync(UnguardedNamespaceSource, (GuardOptions.IgnoredNamespacesKey, "App"));

        // Assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task IgnoredNamespaces_PartialSegmentMatch_DoesNotSuppressDiagnostic()
    {
        // Arrange & Act
        var diagnostics = await AnalyzeAsync(UnguardedNamespaceSource, (GuardOptions.IgnoredNamespacesKey, "App.Gen"));

        // Assert
        Ids(diagnostics, BoundsCheckAnalyzer.DiagnosticId).Should().ContainSingle();
    }

    [Fact]
    public async Task IgnoredNamespaces_InvalidValue_ReportsConfigurationErrorAndKeepsDiagnostic()
    {
        // Arrange & Act
        var diagnostics = await AnalyzeAsync(UnguardedNamespaceSource, (GuardOptions.IgnoredNamespacesKey, "App..Generated"));

        // Assert
        Ids(diagnostics, BoundsCheckAnalyzer.DiagnosticId).Should().ContainSingle();
        var configError = Ids(diagnostics, DiagnosticDescriptors.GuardConfigurationRule.Id).Should().ContainSingle().Subject;
        configError.GetMessage().Should().Be(
            "Invalid value 'App..Generated' for 'guard.ignored_namespaces': 'App..Generated' is not a valid namespace name; the default is used instead");
        configError.Location.Should().Be(Location.None);
    }

    // ==================== guard.bounds_check.max_depth ====================

    [Theory]
    [InlineData("1")]
    [InlineData("5")]
    public async Task BoundsCheckMaxDepth_GuardBeyondDepth_ReportsDiagnostic(string maxDepth)
    {
        // Arrange & Act
        var diagnostics = await AnalyzeAsync(GuardedBySurroundingIfSource, (GuardOptions.BoundsCheckMaxDepthKey, maxDepth));

        // Assert
        Ids(diagnostics, BoundsCheckAnalyzer.DiagnosticId).Should().ContainSingle();
    }

    [Theory]
    [InlineData("6")]
    [InlineData("1000")]
    public async Task BoundsCheckMaxDepth_GuardWithinDepth_SuppressesDiagnostic(string maxDepth)
    {
        // Arrange & Act
        var diagnostics = await AnalyzeAsync(GuardedBySurroundingIfSource, (GuardOptions.BoundsCheckMaxDepthKey, maxDepth));

        // Assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task BoundsCheckMaxDepth_NotConfigured_UsesUnlimitedSearch()
    {
        // Arrange & Act
        var diagnostics = await AnalyzeAsync(GuardedBySurroundingIfSource);

        // Assert
        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1001")]
    [InlineData("many")]
    public async Task BoundsCheckMaxDepth_InvalidValue_ReportsConfigurationErrorAndUsesDefault(string value)
    {
        // Arrange & Act: the invalid value falls back to the default (effectively unlimited),
        // so the guard is still found and only the configuration error is reported.
        var diagnostics = await AnalyzeAsync(GuardedBySurroundingIfSource, (GuardOptions.BoundsCheckMaxDepthKey, value));

        // Assert
        diagnostics.Should().ContainSingle()
            .Which.GetMessage().Should().Be(
                $"Invalid value '{value}' for 'guard.bounds_check.max_depth': expected an integer from 1 to 1000; the default is used instead");
        diagnostics.Single().Id.Should().Be(DiagnosticDescriptors.GuardConfigurationRule.Id);
    }

    // ==================== guard.null_check.exclude_private ====================

    [Fact]
    public async Task InvalidBooleanValue_ReportsConfigurationErrorForNullCheckKey()
    {
        // Arrange: no Roslyn null-check analyzer consumes this option yet, so the source is clean and the
        // only expected diagnostic is the configuration error.
        const string source = """
            class C
            {
                void M(int[] arr)
                {
                    var length = arr.Length;
                }
            }
            """;

        // Act
        var diagnostics = await AnalyzeAsync(source, (GuardOptions.NullCheckExcludePrivateKey, "maybe"));

        // Assert
        diagnostics.Should().ContainSingle()
            .Which.GetMessage().Should().Be(
                "Invalid value 'maybe' for 'guard.null_check.exclude_private': expected 'true' or 'false'; the default is used instead");
    }

    [Fact]
    public async Task ValidConfiguration_ReportsNoConfigurationError()
    {
        // Arrange & Act
        var diagnostics = await AnalyzeAsync(
            UnguardedNamespaceSource,
            (GuardOptions.NullCheckExcludePrivateKey, "true"),
            (GuardOptions.BoundsCheckMaxDepthKey, "20"),
            (GuardOptions.IgnoredNamespacesKey, "App.Generated, Tests"));

        // Assert
        diagnostics.Should().BeEmpty();
    }

    /// <summary>
    /// Serves the same values for every syntax tree, standing in for a single <c>.editorconfig</c> section.
    /// </summary>
    private sealed class DictionaryConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions _options;

        public DictionaryConfigOptionsProvider(IReadOnlyDictionary<string, string> values)
        {
            _options = new DictionaryConfigOptions(values);
        }

        public override AnalyzerConfigOptions GlobalOptions => _options;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _options;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _options;
    }

    private sealed class DictionaryConfigOptions : AnalyzerConfigOptions
    {
        private readonly IReadOnlyDictionary<string, string> _values;

        public DictionaryConfigOptions(IReadOnlyDictionary<string, string> values)
        {
            _values = values;
        }

        public override bool TryGetValue(string key, out string value) => _values.TryGetValue(key, out value!);
    }
}
