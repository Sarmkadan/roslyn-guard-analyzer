namespace RoslynGuardAnalyzer.Cli;

/// <summary>
/// Provides extension methods for <see cref="CliOptions"/> to validate and extract derived values.
/// </summary>
public static class CliOptionsExtensions
{
    /// <summary>
    /// Determines whether the CLI options require a project path for analysis.
    /// Returns <c>true</c> when either no project path nor file path is specified,
    /// indicating that the user needs to provide a target for analysis.
    /// </summary>
    /// <param name="options">The CLI options.</param>
    /// <returns><c>true</c> if a project or file path is required; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    public static bool RequiresProjectPath(this CliOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return string.IsNullOrEmpty(options.ProjectPath) && string.IsNullOrEmpty(options.FilePath);
    }

    /// <summary>
    /// Validates the output settings for consistency and completeness.
    /// </summary>
    /// <param name="options">The CLI options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when:
    /// - Report type is required but not specified when generating a report
    /// - Output file is specified with console output format (incompatible combination)
    /// </exception>
    public static void ValidateOutputSettings(this CliOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.GenerateReport && string.IsNullOrEmpty(options.ReportType))
        {
            throw new ArgumentException(
                "Report type is required when generating a report.",
                nameof(options));
        }

        if (!string.IsNullOrEmpty(options.OutputFile) && options.OutputFormat == "console")
        {
            throw new ArgumentException(
                "Output file is not compatible with console output format.",
                nameof(options));
        }
    }

    /// <summary>
    /// Gets the maximum degree of parallelism based on the configured value.
    /// If the configured value is positive, uses that value; otherwise, returns
    /// the processor count as a sensible default.
    /// </summary>
    /// <param name="options">The CLI options containing the parallelism configuration.</param>
    /// <returns>The maximum degree of parallelism to use for analysis operations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    public static int GetMaxDegreeOfParallelism(this CliOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.MaxParallelThreads > 0
            ? options.MaxParallelThreads
            : Environment.ProcessorCount;
    }
}