using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;
using Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;
using Itmo.Dev.Asap.Checker.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Services;

public interface IBanMachineService
{
    Task<CheckingId> CreateCheckingAsync(CancellationToken cancellationToken);

    Task AddCheckingDataAsync(
        CheckingId checkingId,
        IReadOnlyCollection<SubmissionData> data,
        CancellationToken cancellationToken);

    IAsyncEnumerable<SubmissionPairCheckingResult> GetCheckingResultDataAsync(
        CheckingId checkingId,
        CancellationToken cancellationToken);

    IAsyncEnumerable<SimilarCodeBlocks> GetCheckingResultCodeBlocksAsync(
        CheckingResultCodeBlocksQuery query,
        CancellationToken cancellationToken);
}