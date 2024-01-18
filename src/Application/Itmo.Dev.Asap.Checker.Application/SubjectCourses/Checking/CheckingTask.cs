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

        executionMetadata.State ??= new StartingState();

        var context = new CheckingTaskStateHandlerContext(executionContext.Id, metadata);

        do
        {
            CheckingTaskStateExecutionResult result = await HandleAsync(
                executionMetadata.State,
                context,
                cancellationToken);

            if (result is CheckingTaskStateExecutionResult.Finished finished)
            {
                executionMetadata.State = finished.State;
                continue;
            }

            if (result is CheckingTaskStateExecutionResult.FinishedWithResult finishedWithResult)
            {
                executionMetadata.State = finishedWithResult.State;
                return finishedWithResult.Result;
            }

            return new BackgroundTaskExecutionResult<EmptyExecutionResult, CheckingTaskError>.Failure(
                new CheckingTaskError($"Invalid handler result = {result}"));
        }
        while (true);
    }

    private ValueTask<CheckingTaskStateExecutionResult> HandleAsync(
        CheckingTaskState taskState,
        CheckingTaskStateHandlerContext context,
        CancellationToken cancellationToken)
    {
        return taskState switch
        {
            StartingState state => ActivatorUtilities
                .CreateInstance<StartingStateHandler>(_serviceProvider)
                .HandleAsync(state, context, cancellationToken),

            DumpingContentState state => ActivatorUtilities
                .CreateInstance<DumpingContentStateHandler>(_serviceProvider)
                .HandleAsync(state, context, cancellationToken),

            WaitingContentDumpState state => ActivatorUtilities
                .CreateInstance<WaitingContentDumpStateHandler>(_serviceProvider)
                .HandleAsync(state, context, cancellationToken),

            StartingAnalysisState state => ActivatorUtilities
                .CreateInstance<StartingAnalysisStateHandler>(_serviceProvider)
                .HandleAsync(state, context, cancellationToken),

            WaitingAnalysisState state => ActivatorUtilities
                .CreateInstance<WaitingAnalysisStateHandler>(_serviceProvider)
                .HandleAsync(state, context, cancellationToken),

            LoadingResultsState state => ActivatorUtilities
                .CreateInstance<LoadingResultsStateHandler>(_serviceProvider)
                .HandleAsync(state, context, cancellationToken),

            _ => throw new ArgumentOutOfRangeException(nameof(taskState), taskState, "Could not resolve state handler"),
        };
    }
}