using Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;

namespace Itmo.Dev.Asap.Checker.Application.Contracts.Checking;

public static class GetCheckingTaskResultCodeBlocks
{
    public sealed record Request(
        long TaskId,
        Guid FirstSubmissionId,
        Guid SecondSubmissionId,
        int PageSize,
        int Cursor);

    public sealed record Response(IReadOnlyCollection<SimilarCodeBlocks> CodeBlocks);
}