using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;
using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Services;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Services;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Options;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.Postgres.Transactions;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.StateHandlers;

internal class LoadingResultsStateHandler : ICheckingTaskStateHandler
{
    private readonly LoadingResultsState _state;
    private readonly IBanMachineService _banMachineService;
    private readonly IPostgresTransactionProvider _transactionProvider;
    private readonly CheckingServiceOptions _options;
    private readonly ICheckingResultRepository _checkingResultRepository;
    private readonly PairCheckingResultEnricher _resultEnricher;

    public LoadingResultsStateHandler(
        LoadingResultsState state,
        IBanMachineService banMachineService,
        IPostgresTransactionProvider transactionProvider,
        IOptions<CheckingServiceOptions> options,
        ICheckingResultRepository checkingResultRepository,
        PairCheckingResultEnricher resultEnricher)
    {
        _state = state;
        _banMachineService = banMachineService;
        _transactionProvider = transactionProvider;
        _checkingResultRepository = checkingResultRepository;
        _resultEnricher = resultEnricher;
        _options = options.Value;
    }

    public async Task<CheckingTaskStateExecutionResult> HandleAsync(
        CheckingTaskStateHandlerContext context,
        CancellationToken cancellationToken)
    {
        IAsyncEnumerable<BanMachinePairCheckingResult> resultData = _banMachineService
            .GetCheckingResultDataAsync(_state.CheckingId, cancellationToken);

        await using NpgsqlTransaction transaction = await _transactionProvider
            .CreateTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        await foreach (BanMachinePairCheckingResult result in resultData)
        {
            if (result.SimilarityScore < _options.MinimumSubmissionSimilarityScore)
                continue;

            SubmissionPairCheckingResult checkingResult = await _resultEnricher
                .EnrichAsync(result, cancellationToken);

            await _checkingResultRepository
                .AddCheckingResultAsync(context.BackgroundTaskId.Value, checkingResult, cancellationToken);

            var codeBlocksQuery = new CheckingResultCodeBlocksRequest(
                _state.CheckingId,
                result.FirstSubmissionId,
                result.SecondSubmissionId,
                _options.MinimumCodeBlocksSimilarityScore,
                0);

            SimilarCodeBlocks[] codeBlocks;

            do
            {
                codeBlocks = await _banMachineService
                    .GetCheckingResultCodeBlocksAsync(codeBlocksQuery, cancellationToken)
                    .ToArrayAsync(cancellationToken);

                await _checkingResultRepository.AddCheckingResultCodeBlocksAsync(
                    context.BackgroundTaskId.Value,
                    result.FirstSubmissionId,
                    result.SecondSubmissionId,
                    codeBlocks,
                    cancellationToken);

                codeBlocksQuery = codeBlocksQuery with { Cursor = codeBlocksQuery.Cursor + codeBlocks.Length };
            }
            while (codeBlocks is not []);
        }

        await transaction.CommitAsync(cancellationToken);

        var success = new BackgroundTaskExecutionResult<EmptyExecutionResult, CheckingTaskError>.Success(
            EmptyExecutionResult.Value);

        return new CheckingTaskStateExecutionResult(new CompletedState(), success);
    }
}