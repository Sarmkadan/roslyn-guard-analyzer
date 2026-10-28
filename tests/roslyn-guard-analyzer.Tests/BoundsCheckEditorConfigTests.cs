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
using RoslynGuardAnalyzer.Rules;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Integration tests for <see cref="BoundsCheckAnalyzer"/> driven by real <c>.editorconfig</c> text. The text is parsed
/// by Roslyn's <see cref="AnalyzerConfig"/> and <see cref="AnalyzerConfigSet"/>, so section matching, comments and value
/// trimming behave as they do during a build.
/// </summary>
public sealed class BoundsCheckEditorConfigTests
{
    static BoundsCheckEditorConfigTests()
    {
        // ReferenceAssemblies.Net.Net80 restores reference packages through NuGet, which throws
        // when HOME is unset (e.g. minimal CI or service environments). Fall back to the user profile.
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME")))
        {
            Environment.SetEnvironmentVariable("HOME", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }
    }

    private const string EditorConfigPath = "/.editorconfig";
    private const string SourcePath = "/src/Test0.cs";

    // One access per default-checked type, plus a string access that is not checked by default.
    private const string MixedSource = """
        using System;
        using System.Collections.Generic;

        class C
        {
            void M(int[] array, List<int> list, Span<int> span, ReadOnlySpan<int> readOnlySpan, string text, int i)
            {
                _ = array[i];
                _ = list[i];
                _ = span[i];
                _ = readOnlySpan[i];
                _ = text[i];
            }
        }
        """;

    private static readonly string[] DefaultReported = { "array", "list", "readOnlySpan", "span" };

    // ==================== Missing configurations ====================

    [Fact]
    public async Task MissingEditorConfig_UsesDefaults_ReportsDefaultTypesAtWarning()
    {
        // Arrange & Act
        var diagnostics = await AnalyzeAsync(editorConfig: null, MixedSource);

        // Assert
        ReportedExpressions(diagnostics).Should().Equal(DefaultReported);
        Bnd(diagnostics).Should().OnlyContain(d => d.Severity == DiagnosticSeverity.Warning);
        Grd(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public async Task EditorConfigWithoutGuardKeys_UsesDefaults()
    {
        // Arrange
        const string editorConfig = """
            root = true

            [*.cs]
            dotnet_diagnostic.CA1000.severity = warning
            """;

        // Act
        var diagnostics = await AnalyzeAsync(editorConfig, MixedSource);

        // Assert
        ReportedExpressions(diagnostics).Should().Equal(DefaultReported);
        Grd(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public async Task SectionForOtherFileType_DoesNotApplyToCSharpFile()
    {
        // Arrange
        const string editorConfig = """
            root = true

            [*.vb]
            guard.bounds_check.severity = error
            guard.bounds_check.allowed_indexer_types = System.String
            """;

        // Act
        var diagnostics = await AnalyzeAsync(editorConfig, MixedSource);

        // Assert
        ReportedExpressions(diagnostics).Should().Equal(DefaultReported);
        Bnd(diagnostics).Should().OnlyContain(d => d.Severity == DiagnosticSeverity.Warning);
        Grd(diagnostics).Should().BeEmpty();
    }

    // ==================== Valid configurations ====================

    [Fact]
    public async Task AllowedIndexerTypes_RestrictsCheckedTypes()
    {
        // Arrange
        const string editorConfig = """
            root = true

            [*.cs]
            guard.bounds_check.allowed_indexer_types = [], System.String
            """;

        // Act
        var diagnostics = await AnalyzeAsync(editorConfig, MixedSource);

        // Assert
        ReportedExpressions(diagnostics).Should().Equal("array", "text");
        Grd(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public async Task AllowedIndexerTypes_AddsTypeOutsideDefaults()
    {
        // Arrange
        const string editorConfig = """
            root = true

            [*.cs]
            guard.bounds_check.allowed_indexer_types = [], System.Collections.Generic.List, System.String
            """;

        // Act
        var diagnostics = await AnalyzeAsync(editorConfig, MixedSource);

        // Assert
        ReportedExpressions(diagnostics).Should().Equal("array", "list", "text");
    }

    [Fact]
    public async Task CheckSpansFalse_SkipsSpanAndReadOnlySpan()
    {
        // Arrange
        const string editorConfig = """
            root = true

            [*.cs]
            guard.bounds_check.check_spans = false
            """;

        // Act
        var diagnostics = await AnalyzeAsync(editorConfig, MixedSource);

        // Assert
        ReportedExpressions(diagnostics).Should().Equal("array", "list");
    }

    [Fact]
    public async Task CheckSpansTrue_WithSpanTypesNotInAllowedList_SkipsSpans()
    {
        // Arrange: a span type must be in the allowed list to be checked, whatever check_spans says.
        const string editorConfig = """
            root = true

            [*.cs]
            guard.bounds_check.allowed_indexer_types = [], System.String
            guard.bounds_check.check_spans = true
            """;

        // Act
        var diagnostics = await AnalyzeAsync(editorConfig, MixedSource);

        // Assert
        ReportedExpressions(diagnostics).Should().Equal("array", "text");
    }

    [Theory]
    [InlineData("silent", DiagnosticSeverity.Hidden)]
    [InlineData("suggestion", DiagnosticSeverity.Info)]
    [InlineData("warning", DiagnosticSeverity.Warning)]
    [InlineData("error", DiagnosticSeverity.Error)]
    public async Task SeverityOverride_ReportsEveryDiagnosticWithSelectedSeverity(string value, DiagnosticSeverity expected)
    {
        // Arrange
        var editorConfig = $"""
            root = true

            [*.cs]
            guard.bounds_check.severity = {value}
            """;

        // Act
        var diagnostics = await AnalyzeAsync(editorConfig, MixedSource);

        // Assert
        Bnd(diagnostics).Should().HaveCount(DefaultReported.Length)
            .And.OnlyContain(d => d.Severity == expected);
        Grd(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public async Task ValidConfiguration_AppliesEveryKeyTogether()
    {
        // Arrange
        const string editorConfig = """
            root = true

            [*.cs]
            guard.bounds_check.allowed_indexer_types = [], System.Collections.Generic.List
            guard.bounds_check.check_spans = false
            guard.bounds_check.severity = error
            guard.bounds_check.max_depth = 20
            guard.ignored_namespaces = App.Generated
            """;

        // Act
        var diagnostics = await AnalyzeAsync(editorConfig, MixedSource);

        // Assert
        ReportedExpressions(diagnostics).Should().Equal("array", "list");
        Bnd(diagnostics).Should().OnlyContain(d => d.Severity == DiagnosticSeverity.Error);
        Grd(diagnostics).Should().BeEmpty();
    }

    // ==================== Invalid configurations ====================

    [Fact]
    public async Task InvalidSeverity_ReportsErrorNamingKeyAndValue_AndUsesWarning()
    {
        // Arrange
        const string editorConfig = """
            root = true

            [*.cs]
            guard.bounds_check.severity = fatal
            """;

        // Act
        var diagnostics = await AnalyzeAsync(editorConfig, MixedSource);

        // Assert
        Grd(diagnostics).Should().ContainSingle()
            .Which.GetMessage().Should().Be(
                "Invalid value 'fatal' for 'guard.bounds_check.severity': expected 'silent', 'suggestion', 'warning' or 'error'; the default is used instead");
        ReportedExpressions(diagnostics).Should().Equal(DefaultReported);
        Bnd(diagnostics).Should().OnlyContain(d => d.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public async Task EmptyAllowedIndexerTypes_ReportsErrorAndUsesDefaultTypes()
    {
        // Arrange
        const string editorConfig = """
            root = true

            [*.cs]
            guard.bounds_check.allowed_indexer_types =
            """;

        // Act
        var diagnostics = await AnalyzeAsync(editorConfig, MixedSource);

        // Assert
        Grd(diagnostics).Should().ContainSingle()
            .Which.GetMessage().Should().Be(
                "Invalid value '' for 'guard.bounds_check.allowed_indexer_types': must list at least one type; the default is used instead");
        ReportedExpressions(diagnostics).Should().Equal(DefaultReported);
    }

    [Fact]
    public async Task IndexerTypeWithTypeArguments_ReportsErrorNamingEntry()
    {
        // Arrange
        const string editorConfig = """
            root = true

            [*.cs]
            guard.bounds_check.allowed_indexer_types = System.Collections.Generic.List<int>
            """;

        // Act
        var diagnostics = await AnalyzeAsync(editorConfig, MixedSource);

        // Assert
        Grd(diagnostics).Should().ContainSingle()
            .Which.GetMessage().Should().Be(
                "Invalid value 'System.Collections.Generic.List<int>' for 'guard.bounds_check.allowed_indexer_types': " +
                "'System.Collections.Generic.List<int>' is not a valid type name (use a namespace-qualified name without type arguments, or '[]'); " +
                "the default is used instead");
        ReportedExpressions(diagnostics).Should().Equal(DefaultReported);
    }

    [Fact]
    public async Task InvalidCheckSpans_ReportsErrorAndKeepsSpanChecking()
    {
        // Arrange
        const string editorConfig = """
            root = true

            [*.cs]
            guard.bounds_check.check_spans = no
            """;

        // Act
        var diagnostics = await AnalyzeAsync(editorConfig, MixedSource);

        // Assert
        Grd(diagnostics).Should().ContainSingle()
            .Which.GetMessage().Should().Be(
                "Invalid value 'no' for 'guard.bounds_check.check_spans': expected 'true' or 'false'; the default is used instead");
        ReportedExpressions(diagnostics).Should().Equal(DefaultReported);
    }

    [Fact]
    public async Task MultipleInvalidKeys_ReportOneErrorPerKey()
    {
        // Arrange
        const string editorConfig = """
            root = true

            [*.cs]
            guard.bounds_check.check_spans = no
            guard.bounds_check.severity = fatal
            guard.bounds_check.max_depth = 0
            """;

        // Act
        var diagnostics = await AnalyzeAsync(editorConfig, MixedSource);

        // Assert
        Grd(diagnostics).Select(d => d.GetMessage()).Should().BeEquivalentTo(
            "Invalid value 'no' for 'guard.bounds_check.check_spans': expected 'true' or 'false'; the default is used instead",
            "Invalid value 'fatal' for 'guard.bounds_check.severity': expected 'silent', 'suggestion', 'warning' or 'error'; the default is used instead",
            "Invalid value '0' for 'guard.bounds_check.max_depth': expected an integer from 1 to 1000; the default is used instead");
    }

    [Fact]
    public async Task InvalidSectionAcrossFiles_ReportsErrorOnceForCompilation()
    {
        // Arrange: two files share one .editorconfig section, so the invalid value is validated once.
        const string editorConfig = """
            root = true

            [*.cs]
            guard.bounds_check.severity = fatal
            """;

        // Act
        var diagnostics = await AnalyzeTreesAsync(
            editorConfig,
            ("/src/A.cs", MixedSource),
            ("/src/B.cs", MixedSource));

        // Assert
        Grd(diagnostics).Should().ContainSingle();
        Bnd(diagnostics).Should().HaveCount(DefaultReported.Length * 2);
    }

    // ==================== Helpers ====================

    private static Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string? editorConfig, string source) =>
        AnalyzeTreesAsync(editorConfig, (SourcePath, source));

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeTreesAsync(string? editorConfig, params (string Path, string Source)[] files)
    {
        var references = await ReferenceAssemblies.Net.Net80.ResolveAsync(LanguageNames.CSharp, CancellationToken.None);
        var trees = files.Select(file => CSharpSyntaxTree.ParseText(file.Source, path: file.Path));
        var compilation = CSharpCompilation.Create(
            "BoundsCheckEditorConfigTests",
            trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var provider = new EditorConfigOptionsProvider(editorConfig);
        var analyzerOptions = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty, provider);
        var withAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(new BoundsCheckAnalyzer()),
            new CompilationWithAnalyzersOptions(analyzerOptions, onAnalyzerException: null, concurrentAnalysis: false, logAnalyzerExecutionTime: false));

        return await withAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    private static IEnumerable<Diagnostic> Bnd(ImmutableArray<Diagnostic> diagnostics) =>
        diagnostics.Where(d => d.Id == BoundsCheckAnalyzer.DiagnosticId);

    private static IEnumerable<Diagnostic> Grd(ImmutableArray<Diagnostic> diagnostics) =>
        diagnostics.Where(d => d.Id == DiagnosticDescriptors.GuardConfigurationRule.Id);

    /// <summary>
    /// Gets the collection expression named in each BND001 message, sorted, so assertions do not depend on report order.
    /// </summary>
    private static string[] ReportedExpressions(ImmutableArray<Diagnostic> diagnostics) =>
        Bnd(diagnostics)
            .Select(d => d.GetMessage().Split('\'')[1])
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

    /// <summary>
    /// Serves options from a parsed <c>.editorconfig</c> through the same <see cref="AnalyzerConfigSet"/> lookup the compiler uses.
    /// A null text stands for a missing <c>.editorconfig</c>.
    /// </summary>
    private sealed class EditorConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigSet _configSet;

        public EditorConfigOptionsProvider(string? editorConfig)
        {
            var configs = editorConfig is null
                ? ImmutableArray<AnalyzerConfig>.Empty
                : ImmutableArray.Create(AnalyzerConfig.Parse(editorConfig, EditorConfigPath));

            _configSet = AnalyzerConfigSet.Create(configs, out _);
        }

        public override AnalyzerConfigOptions GlobalOptions =>
            new AnalyzerOptionsView(_configSet.GlobalConfigOptions.AnalyzerOptions);

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) =>
            new AnalyzerOptionsView(_configSet.GetOptionsForSourcePath(tree.FilePath).AnalyzerOptions);

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) =>
            new AnalyzerOptionsView(_configSet.GetOptionsForSourcePath(textFile.Path).AnalyzerOptions);
    }

    private sealed class AnalyzerOptionsView : AnalyzerConfigOptions
    {
        private readonly IReadOnlyDictionary<string, string> _values;

        public AnalyzerOptionsView(IReadOnlyDictionary<string, string> values)
        {
            _values = values;
        }

        public override bool TryGetValue(string key, out string value) => _values.TryGetValue(key, out value!);
    }
}
