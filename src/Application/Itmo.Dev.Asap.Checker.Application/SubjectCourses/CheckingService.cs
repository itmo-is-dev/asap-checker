using Itmo.Dev.Asap.Checker.Application.Abstractions.Core;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.Checker.Application.Contracts.Checking;
using Itmo.Dev.Asap.Checker.Application.Models;
using Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;
using Itmo.Dev.Asap.Checker.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking;
using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.Common.Models;
using Itmo.Dev.Platform.Locking;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses;

internal class CheckingService : ICheckingService
{
    private readonly IBackgroundTaskRepository _backgroundTaskRepository;
    private readonly ILockingService _lockingService;
    private readonly ICoreSubjectCourseService _coreSubjectCourseService;
    private readonly IBackgroundTaskRunner _backgroundTaskRunner;
    private readonly IPersistenceContext _context;

    public CheckingService(
        IBackgroundTaskRepository backgroundTaskRepository,
        ILockingService lockingService,
        ICoreSubjectCourseService coreSubjectCourseService,
        IBackgroundTaskRunner backgroundTaskRunner,
        IPersistenceContext context)
    {
        _backgroundTaskRepository = backgroundTaskRepository;
        _lockingService = lockingService;
        _coreSubjectCourseService = coreSubjectCourseService;
        _backgroundTaskRunner = backgroundTaskRunner;
        _context = context;
    }

    public async Task<StartChecking.Response> StartCheckingAsync(
        StartChecking.Request request,
        CancellationToken cancellationToken)
    {
        bool subjectCourseExists = await _coreSubjectCourseService
            .SubjectCourseExistsAsync(request.SubjectCourseId, cancellationToken);

        if (subjectCourseExists is false)
            return new StartChecking.Response.SubjectCourseNotFound();

        var taskMetadata = new CheckingTaskMetadata(request.SubjectCourseId);

        await using ILockHandle lockHandle = await _lockingService.AcquireAsync(taskMetadata, cancellationToken);

        var taskQuery = BackgroundTaskQuery.Build(builder => builder
            .WithName(CheckingTask.Name)
            .WithMetadata(taskMetadata)
            .WithActiveState());

        bool isAlreadyInProgress = await _backgroundTaskRepository
            .QueryAsync(taskQuery, cancellationToken)
            .AnyAsync(cancellationToken);

        if (isAlreadyInProgress)
            return new StartChecking.Response.AlreadyInProgress();

        BackgroundTaskId backgroundTaskId = await _backgroundTaskRunner
            .StartBackgroundTask
            .WithMetadata(taskMetadata)
            .WithExecutionMetadata(new CheckingTaskExecutionMetadata())
            .RunWithAsync<CheckingTask>(cancellationToken);

        BackgroundTask backgroundTask = await _backgroundTaskRepository
            .QueryAsync(BackgroundTaskQuery.Build(x => x.WithId(backgroundTaskId)), cancellationToken)
            .SingleAsync(cancellationToken);

        return new StartChecking.Response.Success(new SubjectCourseCheckingTask(
            new CheckingId(backgroundTaskId.Value),
            backgroundTask.CreatedAt,
            backgroundTask.State is BackgroundTaskState.Completed));
    }

    public async Task<GetCheckingTasks.Response> GetCheckingTasksAsync(
        GetCheckingTasks.Request request,
        CancellationToken cancellationToken)
    {
        var query = BackgroundTaskQuery.Build(builder =>
        {
            if (request.IsActive)
                builder.WithActiveState();

            return builder
                .WithName(CheckingTask.Name)
                .WithMetadata(new CheckingTaskMetadata(request.SubjectCourseId))
                .WithPageSize(request.PageSize)
                .WithCursor(request.PageToken?.CreatedAt ?? DateTimeOffset.MaxValue)
                .WithOrderDirection(OrderDirection.Descending);
        });

        SubjectCourseCheckingTask[] tasks = await _backgroundTaskRepository
            .QueryAsync(query, cancellationToken)
            .Select(x => new SubjectCourseCheckingTask(
                new CheckingId(x.Id.Value),
                x.CreatedAt,
                x.State is BackgroundTaskState.Completed))
            .ToArrayAsync(cancellationToken);

        GetCheckingTasks.PageToken? pageToken = tasks.Length.Equals(request.PageSize)
            ? new GetCheckingTasks.PageToken(tasks[^1].CreatedAt)
            : null;

        return new GetCheckingTasks.Response(tasks, pageToken);
    }

    public async Task<GetCheckingTaskResults.Response> GetCheckingResultsAsync(
        GetCheckingTaskResults.Request request,
        CancellationToken cancellationToken)
    {
        var query = CheckingResultDataQuery.Build(builder => builder
            .WithCheckingId(new CheckingId(request.TaskId))
            .WithAssignmentIds(request.AssignmentIds)
            .WithGroupIds(request.GroupIds)
            .WithPageSize(request.PageSize)
            .WithFirstSubmissionId(request.PageToken?.FirstSubmissionId)
            .WithSecondSubmissionId(request.PageToken?.SecondSubmissionId));

        SubmissionPairCheckingResult[] results = await _context.CheckingResults
            .QueryDataAsync(query, cancellationToken)
            .ToArrayAsync(cancellationToken);

        GetCheckingTaskResults.PageToken? pageToken = results.Length.Equals(request.PageSize)
            ? MapToPageToken(results[^1])
            : null;

        return new GetCheckingTaskResults.Response(results, pageToken);
    }

    public async Task<GetCheckingTaskResultCodeBlocks.Response> GetCheckingResultCodeBlocksAsync(
        GetCheckingTaskResultCodeBlocks.Request request,
        CancellationToken cancellationToken)
    {
        var query = CheckingResultCodeBlocksQuery.Build(builder => builder
            .WithCheckingId(new CheckingId(request.TaskId))
            .WithFirstSubmissionId(request.FirstSubmissionId)
            .WithSecondSubmissionId(request.SecondSubmissionId)
            .WithPageSize(request.PageSize)
            .WithCursor(request.Cursor));

        SimilarCodeBlocks[] codeBlocks = await _context.CheckingResults
            .QueryCodeBlocksAsync(query, cancellationToken)
            .ToArrayAsync(cancellationToken);

        return new GetCheckingTaskResultCodeBlocks.Response(codeBlocks);
    }

    private static GetCheckingTaskResults.PageToken MapToPageToken(SubmissionPairCheckingResult result)
        => new GetCheckingTaskResults.PageToken(result.FirstSubmission.Id, result.SecondSubmission.Id);
}