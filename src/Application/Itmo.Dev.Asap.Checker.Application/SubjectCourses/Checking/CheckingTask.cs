using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.StateHandlers;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking;

public class CheckingTask : IBackgroundTask<
    CheckingTaskMetadata,
    CheckingTaskExecutionMetadata,
    EmptyExecutionResult,
    CheckingTaskError>
{
    private readonly IServiceProvider _serviceProvider;

    public CheckingTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static string Name => nameof(CheckingTask);

    public async Task<BackgroundTaskExecutionResult<EmptyExecutionResult, CheckingTaskError>> ExecuteAsync(
        BackgroundTaskExecutionContext<CheckingTaskMetadata, CheckingTaskExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        CheckingTaskMetadata metadata = executionContext.Metadata;
        CheckingTaskExecutionMetadata executionMetadata = executionContext.ExecutionMetadata;

        executionMetadata.State ??= new DumpingContentState();

        var context = new CheckingTaskStateHandlerContext(executionContext.Id, metadata);

        ICheckingTaskStateHandler handler = ResolveStateHandler(executionMetadata.State);
        CheckingTaskStateExecutionResult result = await handler.HandleAsync(context, cancellationToken);

        executionMetadata.PreviousState = executionMetadata.State;
        executionMetadata.State = result.State;

        return result.Result;
    }

    private ICheckingTaskStateHandler ResolveStateHandler(CheckingTaskState taskState)
    {
        return taskState switch
        {
            DumpingContentState state
                => ActivatorUtilities.CreateInstance<DumpingContentStateHandler>(_serviceProvider, state),

            LoadingResultsState state
                => ActivatorUtilities.CreateInstance<LoadingResultsStateHandler>(_serviceProvider, state),

            _ => throw new ArgumentOutOfRangeException(nameof(taskState), taskState, "Could not resolve state handler"),
        };
    }
}