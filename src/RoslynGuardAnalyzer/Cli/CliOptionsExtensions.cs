namespace RoslynGuardAnalyzer.Cli;

/// <summary>
/// Provides extension methods for <see cref="CliOptions"/>.
/// </summary>
public static class CliOptionsExtensions
{
    /// <summary>
    /// Determines whether the CLI options require a project path.
    /// </summary>
    /// <param name="options">The CLI options.</param>
    /// <returns><c>true</c> if a project path is required; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    public static bool RequiresProjectPath(this CliOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return string.IsNullOrEmpty(options.ProjectPath) && string.IsNullOrEmpty(options.FilePath);
    }

    /// <summary>
    /// Validates the output settings.
    /// </summary>
    /// <param name="options">The CLI options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the output settings are invalid.</exception>
    public static void ValidateOutputSettings(this CliOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.GenerateReport && string.IsNullOrEmpty(options.ReportType))
        {
            throw new ArgumentException("Report type is required when generating a report.", nameof(options));
        }

        if (!string.IsNullOrEmpty(options.OutputFile) && options.OutputFormat == "console")
        {
            throw new ArgumentException("Output file is not compatible with console output format.", nameof(options));
        }
    }

    /// <summary>
    /// Gets the maximum degree of parallelism.
    /// </summary>
    /// <param name="options">The CLI options.</param>
    /// <returns>The maximum degree of parallelism.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    public static int GetMaxDegreeOfParallelism(this CliOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.MaxParallelThreads > 0 ? options.MaxParallelThreads : Environment.ProcessorCount;
    }
}
