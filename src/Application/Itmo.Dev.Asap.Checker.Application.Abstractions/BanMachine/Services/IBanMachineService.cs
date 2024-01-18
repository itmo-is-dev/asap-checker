using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;
using Itmo.Dev.Asap.Checker.Application.Models;
using Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;
using Itmo.Dev.Asap.Checker.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Services;

public interface IBanMachineService
{
    Task<AnalysisId> CreateAnalysisAsync(CancellationToken cancellationToken);

    Task AddAnalysisDataAsync(
        AnalysisId analysisId,
        IReadOnlyCollection<SubmissionData> data,
        CancellationToken cancellationToken);

    Task StartAnalysisAsync(AnalysisId analysisId, CancellationToken cancellationToken);

    IAsyncEnumerable<BanMachinePairAnalysisResult> GetAnalysisResultsDataAsync(
        AnalysisId analysisId,
        CancellationToken cancellationToken);

    IAsyncEnumerable<SimilarCodeBlocks> GetAnalysisResultCodeBlocksAsync(
        CheckingResultCodeBlocksRequest query,
        CancellationToken cancellationToken);
}