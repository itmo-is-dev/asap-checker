using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;
using Itmo.Dev.Asap.Checker.Application.Contracts.Checking.Notifications;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.Events;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses;

internal class CheckingCompletedHandler : IEventHandler<CheckingCompletedEvent>
{
    private readonly IBackgroundTaskRepository _backgroundTaskRepository;
    private readonly ILogger<CheckingCompletedHandler> _logger;
    private readonly IBackgroundTaskRunner _backgroundTaskRunner;

    public CheckingCompletedHandler(
        IBackgroundTaskRepository backgroundTaskRepository,
        ILogger<CheckingCompletedHandler> logger,
        IBackgroundTaskRunner backgroundTaskRunner)
    {
        _backgroundTaskRepository = backgroundTaskRepository;
        _logger = logger;
        _backgroundTaskRunner = backgroundTaskRunner;
    }

    public async ValueTask HandleAsync(CheckingCompletedEvent evt, CancellationToken cancellationToken)
    {
        var executionMetadata = new CheckingTaskExecutionMetadata
        {
            State = new LoadingResultsState(new CheckingId(evt.CheckingId)),
        };

        var checkingTaskQuery = BackgroundTaskQuery.Build(builder => builder
            .WithName(CheckingTask.Name)
            .WithExecutionMetadata(executionMetadata)
            .WithActiveState());

        BackgroundTask? checkingTask = await _backgroundTaskRepository
            .QueryAsync(checkingTaskQuery, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (checkingTask is null)
        {
            _logger.LogCritical(
                "Checking task for ban machine checking id = {CheckingId} not found",
                evt.CheckingId);

            return;
        }

        await _backgroundTaskRunner
            .ProceedBackgroundTask
            .WithId(checkingTask.Id)
            .WithoutExecutionMetadataModification()
            .ProceedAsync(cancellationToken);
    }
}