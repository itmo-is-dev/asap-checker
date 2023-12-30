using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking;
using Itmo.Dev.Platform.BackgroundTasks.Configuration.Builders;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Asap.Checker.Application.Extensions;

public static class BackgroundTaskConfigurationBuilderExtensions
{
    public static IBackgroundTaskConfigurationBuilder AddApplicationBackgroundTasks(
        this IBackgroundTaskConfigurationBuilder builder)
    {
        return builder
            .AddBackgroundTask(x => x
                .WithMetadata<CheckingTaskMetadata>()
                .WithExecutionMetadata<CheckingTaskExecutionMetadata>()
                .WithResult<EmptyExecutionResult>()
                .WithError<CheckingTaskError>()
                .HandleBy<CheckingTask>());
    }
}