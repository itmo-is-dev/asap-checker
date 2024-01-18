using Itmo.Dev.Asap.Checker.Application.Contracts.Checking.Notifications;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.Events;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses;

internal class SubjectCourseDumpFinishedHandler : IEventHandler<SubjectCourseDumpFinishedEvent>
{
    private readonly ILogger<SubjectCourseDumpFinishedHandler> _logger;
    private readonly IBackgroundTaskRepository _backgroundTaskRepository;
    private readonly IBackgroundTaskRunner _backgroundTaskRunner;

    public SubjectCourseDumpFinishedHandler(
        ILogger<SubjectCourseDumpFinishedHandler> logger,
        IBackgroundTaskRepository backgroundTaskRepository,
        IBackgroundTaskRunner backgroundTaskRunner)
    {
        _logger = logger;
        _backgroundTaskRepository = backgroundTaskRepository;
        _backgroundTaskRunner = backgroundTaskRunner;
    }

    public async ValueTask HandleAsync(SubjectCourseDumpFinishedEvent evt, CancellationToken cancellationToken)
    {
        var executionMetadata = new CheckingTaskExecutionMetadata
        {
            State = new WaitingContentDumpState(evt.DumpTaskId),
        };

        var checkingTaskQuery = BackgroundTaskQuery.Build(builder => builder
            .WithName(CheckingTask.Name)
            .WithExecutionMetadata(executionMetadata)
            .WithState(BackgroundTaskState.Suspended));

        BackgroundTask? checkingTask = await _backgroundTaskRepository
            .QueryAsync(checkingTaskQuery, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (checkingTask is null)
        {
            _logger.LogCritical(
                "Checking task for content dump = {DumpTaskId} not found",
                evt.DumpTaskId);

            return;
        }

        await _backgroundTaskRunner
            .ProceedBackgroundTask
            .WithId(checkingTask.Id)
            .WithoutExecutionMetadataModification()
            .ProceedAsync(cancellationToken);
    }
}