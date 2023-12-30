using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.Checker.Application.Contracts.Checking.Notifications;
using Itmo.Dev.Asap.Checker.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.Events;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses;

internal class SubjectCourseDumpFinishedHandler : IEventHandler<SubjectCourseDumpFinishedEvent>
{
    private readonly IPersistenceContext _context;
    private readonly ILogger<SubjectCourseDumpFinishedHandler> _logger;
    private readonly IBackgroundTaskRepository _backgroundTaskRepository;
    private readonly IBackgroundTaskRunner _backgroundTaskRunner;

    public SubjectCourseDumpFinishedHandler(
        IPersistenceContext context,
        ILogger<SubjectCourseDumpFinishedHandler> logger,
        IBackgroundTaskRepository backgroundTaskRepository,
        IBackgroundTaskRunner backgroundTaskRunner)
    {
        _context = context;
        _logger = logger;
        _backgroundTaskRepository = backgroundTaskRepository;
        _backgroundTaskRunner = backgroundTaskRunner;
    }

    public async ValueTask HandleAsync(SubjectCourseDumpFinishedEvent evt, CancellationToken cancellationToken)
    {
        SubjectCourseDumpTask? dumpTask = await _context.SubjectCourseDumpTasks
            .QueryAsync(SubjectCourseDumpTaskQuery.Build(x => x.WithTaskId(evt.DumpTaskId)), cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (dumpTask is null)
        {
            _logger.LogCritical(
                "Unknown dump task has finished, id = {Id}",
                dumpTask);

            return;
        }

        var checkingTaskQuery = BackgroundTaskQuery.Build(builder => builder
            .WithName(CheckingTask.Name)
            .WithMetadata(new CheckingTaskMetadata(dumpTask.SubjectCourseId))
            .WithActiveState());

        BackgroundTask? checkingTask = await _backgroundTaskRepository
            .QueryAsync(checkingTaskQuery, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (checkingTask is null)
        {
            _logger.LogCritical(
                "Dump for subject course = {SubjectCourseId}) is finished, but checking task not found",
                dumpTask.SubjectCourseId);

            return;
        }

        var executionMetadata = (CheckingTaskExecutionMetadata)checkingTask.ExecutionMetadata;

        if (executionMetadata.PreviousState is not DumpingContentState)
        {
            _logger.LogCritical(
                "Trying to proceed checking task in invalid state = {State}",
                executionMetadata.State?.GetType());

            return;
        }

        await _backgroundTaskRunner
            .ProceedBackgroundTask
            .WithId(checkingTask.Id)
            .WithoutExecutionMetadataModification()
            .ProceedAsync(cancellationToken);
    }
}