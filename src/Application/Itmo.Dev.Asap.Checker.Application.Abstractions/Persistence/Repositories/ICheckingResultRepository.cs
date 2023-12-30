using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;

public interface ICheckingResultRepository
{
    IAsyncEnumerable<SubmissionPairCheckingResult> QueryDataAsync(
        CheckingResultDataQuery query,
        CancellationToken cancellationToken);

    IAsyncEnumerable<SimilarCodeBlocks> QueryCodeBlocksAsync(
        CheckingResultCodeBlocksQuery query,
        CancellationToken cancellationToken);

    Task AddCheckingResultAsync(long taskId, SubmissionPairCheckingResult result, CancellationToken cancellationToken);

    Task AddCheckingResultCodeBlocksAsync(
        long taskId,
        Guid firstSubmissionId,
        Guid secondSubmissionId,
        IReadOnlyCollection<SimilarCodeBlocks> codeBlocks,
        CancellationToken cancellationToken);
}