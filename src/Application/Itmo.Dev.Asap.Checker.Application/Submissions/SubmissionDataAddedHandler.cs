using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence;
using Itmo.Dev.Asap.Checker.Application.Contracts.Submissions;
using Itmo.Dev.Asap.Checker.Application.Models;
using Itmo.Dev.Asap.Checker.Application.Models.Submissions;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.Events;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Checker.Application.Submissions;

public class SubmissionDataAddedHandler : IEventHandler<SubmissionDataAddedEvent>
{
    private readonly IPersistenceContext _context;
    private readonly IBackgroundTaskRepository _backgroundTaskRepository;
    private readonly ILogger<SubmissionDataAddedHandler> _logger;

    public SubmissionDataAddedHandler(
        IPersistenceContext context,
        IBackgroundTaskRepository backgroundTaskRepository,
        ILogger<SubmissionDataAddedHandler> logger)
    {
        _context = context;
        _backgroundTaskRepository = backgroundTaskRepository;
        _logger = logger;
    }

    public async ValueTask HandleAsync(SubmissionDataAddedEvent evt, CancellationToken cancellationToken)
    {
        IEnumerable<CheckingTaskExecutionMetadata> metadata = evt.SubmissionData
            .Select(x => x.DumpTaskId)
            .Distinct()
            .Select(x => new CheckingTaskExecutionMetadata { State = new WaitingContentDumpState(x) });

        var query = BackgroundTaskQuery.Build(builder => builder
            .WithName(CheckingTask.Name)
            .WithExecutionMetadatas(metadata)
            .WithState(BackgroundTaskState.Suspended));

        Dictionary<DumpTaskId, CheckingId> checkings = await _backgroundTaskRepository
            .QueryAsync(query, cancellationToken)
            .ToDictionaryAsync(GetKey, x => new CheckingId(x.Id.Value), cancellationToken);

        SubmissionData[] data = Map(evt.SubmissionData, checkings).ToArray();

        await _context.SubmissionData.AddRangeAsync(data, cancellationToken);
    }

    private static DumpTaskId GetKey(BackgroundTask task)
    {
        var executionMetadata = (CheckingTaskExecutionMetadata)task.ExecutionMetadata;
        var state = (WaitingContentDumpState)executionMetadata.State!;

        return state.DumpTaskId;
    }

    private IEnumerable<SubmissionData> Map(
        IEnumerable<SubmissionDataAddedEvent.Data> data,
        IReadOnlyDictionary<DumpTaskId, CheckingId> checkings)
    {
        foreach (SubmissionDataAddedEvent.Data submission in data)
        {
            if (checkings.TryGetValue(submission.DumpTaskId, out CheckingId checkingId) is false)
            {
                _logger.LogCritical(
                    "Checking task for dump = {DumpTaskId} not found",
                    submission.DumpTaskId.Value);

                continue;
            }

            yield return new SubmissionData(
                submission.SubmissionId,
                submission.UserId,
                submission.AssignmentId,
                checkingId,
                submission.FileLink);
        }
    }
}