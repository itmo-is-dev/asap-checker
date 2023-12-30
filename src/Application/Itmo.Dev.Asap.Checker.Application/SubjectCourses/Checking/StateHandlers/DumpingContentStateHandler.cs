using Itmo.Dev.Asap.Checker.Application.Abstractions.Github;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Github.Results;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.Checker.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.StateHandlers;

public class DumpingContentStateHandler : ICheckingTaskStateHandler
{
    private readonly DumpingContentState _state;
    private readonly IGithubSubjectCourseService _githubSubjectCourseService;
    private readonly ISubjectCourseDumpTaskRepository _subjectCourseDumpTaskRepository;
    private readonly ILogger<DumpingContentStateHandler> _logger;

    public DumpingContentStateHandler(
        DumpingContentState state,
        IGithubSubjectCourseService githubSubjectCourseService,
        ISubjectCourseDumpTaskRepository subjectCourseDumpTaskRepository,
        ILogger<DumpingContentStateHandler> logger)
    {
        _githubSubjectCourseService = githubSubjectCourseService;
        _subjectCourseDumpTaskRepository = subjectCourseDumpTaskRepository;
        _logger = logger;
        _state = state;
    }

    public async Task<CheckingTaskStateExecutionResult> HandleAsync(
        CheckingTaskStateHandlerContext context,
        CancellationToken cancellationToken)
    {
        StartContentDumpResult result = await _githubSubjectCourseService
            .StartContentDumpAsync(context.Metadata.SubjectCourseId, cancellationToken);

#pragma warning disable IDE0008
        if (TryHandleStartDumpResult(result, out var success, out var error) is false)
#pragma warning restore IDE0008
        {
            return new CheckingTaskStateExecutionResult(_state, error);
        }

        var subjectCourseDumpTask = new SubjectCourseDumpTask(context.Metadata.SubjectCourseId, success.TaskId);
        await _subjectCourseDumpTaskRepository.AddAsync(subjectCourseDumpTask, cancellationToken);

        return new CheckingTaskStateExecutionResult(
            new StartingCheckingState(success.TaskId),
            new BackgroundTaskExecutionResult<EmptyExecutionResult, CheckingTaskError>.Suspended());
    }

    private bool TryHandleStartDumpResult(
        StartContentDumpResult result,
        [NotNullWhen(true)] out StartContentDumpResult.Success? success,
        [NotNullWhen(false)] out BackgroundTaskExecutionResult<EmptyExecutionResult, CheckingTaskError>? error)
    {
        if (result is StartContentDumpResult.AlreadyInProgress)
        {
            const string message = "Dump already in progress";

            success = null;

            error = new BackgroundTaskExecutionResult<EmptyExecutionResult, CheckingTaskError>.Failure(
                new CheckingTaskError(message));

            return false;
        }

        if (result is StartContentDumpResult.SubjectCourseNotFound)
        {
            const string message = "Subject course not found";

            success = null;

            error = new BackgroundTaskExecutionResult<EmptyExecutionResult, CheckingTaskError>.Failure(
                new CheckingTaskError(message));
        }

        if (result is not StartContentDumpResult.Success s)
        {
            const string message = "Failed to start subject course content dump";

            _logger.LogCritical(
                "Received unexpected result when starting content dump, type = {Type}",
                result.GetType());

            success = null;

            error = new BackgroundTaskExecutionResult<EmptyExecutionResult, CheckingTaskError>.Failure(
                new CheckingTaskError(message));

            return false;
        }

        success = s;
        error = null;

        return true;
    }
}