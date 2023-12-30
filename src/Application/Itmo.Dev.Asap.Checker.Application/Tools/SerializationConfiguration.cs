using FluentSerialization;
using FluentSerialization.Models;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;

namespace Itmo.Dev.Asap.Checker.Application.Tools;

internal class SerializationConfiguration : ISerializationConfiguration
{
    public void Configure(ISerializationConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Type<DumpingContentState>().HasTypeKey("Dumping");
        configurationBuilder.Type<StartingCheckingState>().HasTypeKey("StartingChecking");
        configurationBuilder.Type<LoadingResultsState>().HasTypeKey("LoadingResults");
        configurationBuilder.Type<CompletedState>().HasTypeKey("Completed");

        configurationBuilder
            .Type<CheckingTaskExecutionMetadata>()
            .Property(x => x.PreviousState)
            .WithNullValueMode(NullValueMode.Ignore);
    }
}