using Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;

namespace Itmo.Dev.Asap.Checker.Application.Contracts.Checking;

public static class GetCheckingTaskResults
{
    public sealed record Request(
        long TaskId,
        Guid[] AssignmentIds,
        Guid[] GroupIds,
        int PageSize,
        PageToken? PageToken);

    public sealed record Response(IReadOnlyCollection<SubmissionPairCheckingResult> Results, PageToken? PageToken);

    public sealed record PageToken(Guid FirstSubmissionId, Guid SecondSubmissionId);
}