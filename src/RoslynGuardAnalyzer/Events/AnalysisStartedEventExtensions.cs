using System;

namespace RoslynGuardAnalyzer.Events
{
    public static class AnalysisStartedEventExtensions
    {
        public static bool HasConfigFilePath(this AnalysisStartedEvent @event)
        {
            return !string.IsNullOrEmpty(@event.ConfigFilePath);
        }

        public static string GetAnalysisSummary(this AnalysisStartedEvent @event)
        {
            return $"Analysis started for project '{@event.ProjectPath}' with ID '{@event.AnalysisId}'";
        }

        public static bool IsValid(this AnalysisStartedEvent @event)
        {
            return !string.IsNullOrEmpty(@event.ProjectPath) && !string.IsNullOrEmpty(@event.AnalysisId);
        }
    }
}
