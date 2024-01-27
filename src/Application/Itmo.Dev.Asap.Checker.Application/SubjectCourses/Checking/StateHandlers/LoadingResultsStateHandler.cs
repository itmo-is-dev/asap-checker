using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;
using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Services;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.Checker.Application.Models;
using Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Services;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Options;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.Postgres.Transactions;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.StateHandlers;

internal class LoadingResultsStateHandler : ICheckingTaskStateHandler<LoadingResultsState>
{
    private readonly IBanMachineService _banMachineService;
    private readonly IPostgresTransactionProvider _transactionProvider;
    private readonly CheckingServiceOptions _options;
    private readonly ICheckingResultRepository _checkingResultRepository;
    private readonly PairCheckingResultEnricher _resultEnricher;

    public LoadingResultsStateHandler(
        IBanMachineService banMachineService,
        IPostgresTransactionProvider transactionProvider,
        IOptions<CheckingServiceOptions> options,
        ICheckingResultRepository checkingResultRepository,
        PairCheckingResultEnricher resultEnricher)
    {
        _banMachineService = banMachineService;
        _transactionProvider = transactionProvider;
        _checkingResultRepository = checkingResultRepository;
        _resultEnricher = resultEnricher;
        _options = options.Value;
    }

    public async ValueTask<CheckingTaskStateExecutionResult> HandleAsync(
        LoadingResultsState state,
        CheckingTaskStateHandlerContext context,
        CancellationToken cancellationToken)
    {
        IAsyncEnumerable<BanMachinePairAnalysisResult> resultData = _banMachineService
            .GetAnalysisResultsDataAsync(state.AnalysisId, cancellationToken);

        await using NpgsqlTransaction transaction = await _transactionProvider
            .CreateTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        await foreach (BanMachinePairAnalysisResult result in resultData)
        {
            if (result.SimilarityScore < _options.MinimumSubmissionSimilarityScore)
                continue;

            await SaveResultAsync(state, context, result, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return new CheckingTaskStateExecutionResult.FinishedWithResult(
            new CompletedState(),
            BackgroundTaskExecutionResult.Success.WithEmptyResult().ForError<CheckingTaskError>());
    }

    private async Task SaveResultAsync(
        LoadingResultsState state,
        CheckingTaskStateHandlerContext context,
        BanMachinePairAnalysisResult result,
        CancellationToken cancellationToken)
    {
        var checkingId = new CheckingId(context.BackgroundTaskId.Value);

        SubmissionPairCheckingResult checkingResult = await _resultEnricher
            .EnrichAsync(checkingId, result, cancellationToken);

        await _checkingResultRepository
            .AddCheckingResultAsync(checkingId, checkingResult, cancellationToken);

        var codeBlocksQuery = new CheckingResultCodeBlocksRequest(
            state.AnalysisId,
            result.FirstSubmissionId,
            result.SecondSubmissionId,
            _options.MinimumCodeBlocksSimilarityScore,
            0);

        SimilarCodeBlocks[] codeBlocks;

        do
        {
            codeBlocks = await _banMachineService
                .GetAnalysisResultCodeBlocksAsync(codeBlocksQuery, cancellationToken)
                .ToArrayAsync(cancellationToken);

            await _checkingResultRepository.AddCheckingResultCodeBlocksAsync(
                checkingId,
                result.FirstSubmissionId,
                result.SecondSubmissionId,
                codeBlocks,
                cancellationToken);

            codeBlocksQuery = codeBlocksQuery with { Cursor = codeBlocksQuery.Cursor + codeBlocks.Length };
        }
        while (codeBlocks is not []);
    }
}