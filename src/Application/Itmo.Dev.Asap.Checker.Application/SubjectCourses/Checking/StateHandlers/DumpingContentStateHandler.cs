using Itmo.Dev.Asap.Checker.Application.Abstractions.Github;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Github.Results;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.StateHandlers;

internal class DumpingContentStateHandler : ICheckingTaskStateHandler<DumpingContentState>
{
    private readonly IGithubSubjectCourseService _githubSubjectCourseService;
    private readonly ILogger<DumpingContentStateHandler> _logger;

    public DumpingContentStateHandler(
        IGithubSubjectCourseService githubSubjectCourseService,
        ILogger<DumpingContentStateHandler> logger)
    {
        _githubSubjectCourseService = githubSubjectCourseService;
        _logger = logger;
    }

    public async ValueTask<CheckingTaskStateExecutionResult> HandleAsync(
        DumpingContentState state,
        CheckingTaskStateHandlerContext context,
        CancellationToken cancellationToken)
    {
        StartContentDumpResult result = await _githubSubjectCourseService
            .StartContentDumpAsync(context.Metadata.SubjectCourseId, cancellationToken);

        if (result is StartContentDumpResult.Success success)
        {
            return new CheckingTaskStateExecutionResult.FinishedWithResult(
                new WaitingContentDumpState(success.TaskId),
                BackgroundTaskExecutionResult.Suspended.ForEmptyResult().ForError<CheckingTaskError>());
        }

        if (result is StartContentDumpResult.AlreadyInProgress)
        {
            var error = new CheckingTaskError("Dump already in progress");

            return new CheckingTaskStateExecutionResult.FinishedWithResult(
                state,
                BackgroundTaskExecutionResult.Failure.ForEmptyResult().WithError(error));
        }

        if (result is StartContentDumpResult.SubjectCourseNotFound)
        {
            var error = new CheckingTaskError("Subject course not found");

            return new CheckingTaskStateExecutionResult.FinishedWithResult(
                state,
                BackgroundTaskExecutionResult.Failure.ForEmptyResult().WithError(error));
        }

        {
            _logger.LogCritical(
                "Received unexpected result when starting content dump, type = {Type}",
                result.GetType());

            var error = new CheckingTaskError("Failed to start subject course content dump");

            return new CheckingTaskStateExecutionResult.FinishedWithResult(
                state,
                BackgroundTaskExecutionResult.Failure.ForEmptyResult().WithError(error));
        }
    }
}