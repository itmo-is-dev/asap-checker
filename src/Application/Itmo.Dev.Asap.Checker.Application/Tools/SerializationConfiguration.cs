using FluentSerialization;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;

namespace Itmo.Dev.Asap.Checker.Application.Tools;

internal class SerializationConfiguration : ISerializationConfiguration
{
    public void Configure(ISerializationConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Type<CompletedState>().HasTypeKey("Completed");
        configurationBuilder.Type<DumpingContentState>().HasTypeKey("DumpingContent");
        configurationBuilder.Type<LoadingResultsState>().HasTypeKey("LoadingResults");
        configurationBuilder.Type<StartingAnalysisState>().HasTypeKey("StartingAnalysis");
        configurationBuilder.Type<StartingState>().HasTypeKey("Starting");
        configurationBuilder.Type<WaitingAnalysisState>().HasTypeKey("WaitingAnalysis");
        configurationBuilder.Type<WaitingContentDumpState>().HasTypeKey("WaitingContentDump");
    }
}