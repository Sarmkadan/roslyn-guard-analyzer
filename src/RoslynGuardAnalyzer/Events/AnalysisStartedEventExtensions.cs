using System;

namespace RoslynGuardAnalyzer.Events
{
    /// <summary>
    /// Provides extension methods for <see cref="AnalysisStartedEvent"/> to validate and format analysis start information.
    /// </summary>
    public static class AnalysisStartedEventExtensions
    {
        /// <summary>
        /// Determines whether the analysis event has a configuration file path specified.
        /// </summary>
        /// <param name="event">The analysis started event to check.</param>
        /// <returns><see langword="true"/> if the configuration file path is not null or empty; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
        public static bool HasConfigFilePath(this AnalysisStartedEvent @event)
        {
            ArgumentNullException.ThrowIfNull(@event);

            return !string.IsNullOrEmpty(@event.ConfigFilePath);
        }

        /// <summary>
        /// Generates a human-readable summary of the analysis start event.
        /// </summary>
        /// <param name="event">The analysis started event.</param>
        /// <returns>A formatted string containing the project path and analysis ID.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
        public static string GetAnalysisSummary(this AnalysisStartedEvent @event)
        {
            ArgumentNullException.ThrowIfNull(@event);

            return $"Analysis started for project '{@event.ProjectPath}' with ID '{@event.AnalysisId}'";
        }

        /// <summary>
        /// Validates that the analysis event contains all required information.
        /// </summary>
        /// <param name="event">The analysis started event to validate.</param>
        /// <returns><see langword="true"/> if the event is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
        public static bool IsValid(this AnalysisStartedEvent @event)
        {
            ArgumentNullException.ThrowIfNull(@event);

            return !string.IsNullOrEmpty(@event.ProjectPath) && !string.IsNullOrEmpty(@event.AnalysisId);
        }
    }
}
