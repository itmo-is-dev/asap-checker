using Itmo.Dev.Asap.Checker.Application.Contracts.Checking.Notifications;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.Events;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses;

internal class AnalysisCompletedHandler : IEventHandler<AnalysisCompletedEvent>
{
    private readonly IBackgroundTaskRepository _backgroundTaskRepository;
    private readonly ILogger<AnalysisCompletedHandler> _logger;
    private readonly IBackgroundTaskRunner _backgroundTaskRunner;

    public AnalysisCompletedHandler(
        IBackgroundTaskRepository backgroundTaskRepository,
        ILogger<AnalysisCompletedHandler> logger,
        IBackgroundTaskRunner backgroundTaskRunner)
    {
        _backgroundTaskRepository = backgroundTaskRepository;
        _logger = logger;
        _backgroundTaskRunner = backgroundTaskRunner;
    }

    public async ValueTask HandleAsync(AnalysisCompletedEvent evt, CancellationToken cancellationToken)
    {
        var executionMetadata = new CheckingTaskExecutionMetadata
        {
            State = new WaitingAnalysisState(evt.AnalysisId),
        };

        var checkingTaskQuery = BackgroundTaskQuery.Build(builder => builder
            .WithName(CheckingTask.Name)
            .WithExecutionMetadata(executionMetadata)
            .WithState(BackgroundTaskState.Suspended)
            .WithPageSize(1));

        BackgroundTask? checkingTask = await _backgroundTaskRepository
            .QueryAsync(checkingTaskQuery, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (checkingTask is null)
        {
            _logger.LogCritical(
                "Checking task for ban machine analysis id = {AnalysisId} not found",
                evt.AnalysisId);

            return;
        }

        await _backgroundTaskRunner
            .ProceedBackgroundTask
            .WithId(checkingTask.Id)
            .WithoutExecutionMetadataModification()
            .ProceedAsync(cancellationToken);
    }
}