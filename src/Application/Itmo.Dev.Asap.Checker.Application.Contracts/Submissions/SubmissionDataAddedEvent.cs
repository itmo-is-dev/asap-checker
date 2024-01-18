using Itmo.Dev.Asap.Checker.Application.Models;
using Itmo.Dev.Platform.Events;

namespace Itmo.Dev.Asap.Checker.Application.Contracts.Submissions;

public record SubmissionDataAddedEvent(IReadOnlyCollection<SubmissionDataAddedEvent.Data> SubmissionData) : IEvent
{
#pragma warning disable CA1724
    public record struct Data(
        Guid SubmissionId,
        Guid UserId,
        Guid AssignmentId,
        DumpTaskId DumpTaskId,
        string FileLink);
}