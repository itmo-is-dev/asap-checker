using Itmo.Dev.Asap.BanMachine;
using Itmo.Dev.Asap.BanMachine.Models;
using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;
using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Services;
using Itmo.Dev.Asap.Checker.Infrastructure.BanMachine.Mapping;
using System.Runtime.CompilerServices;
using CodeBlock = Itmo.Dev.Asap.Checker.Application.Models.CheckingResults.CodeBlock;
using SimilarCodeBlocks = Itmo.Dev.Asap.Checker.Application.Models.CheckingResults.SimilarCodeBlocks;
using SubmissionData = Itmo.Dev.Asap.Checker.Application.Models.Submissions.SubmissionData;

namespace Itmo.Dev.Asap.Checker.Infrastructure.BanMachine.Services;

internal class BanMachineService : IBanMachineService
{
    private readonly AnalysisService.AnalysisServiceClient _analysisServiceClient;
    private readonly AnalysisResultsService.AnalysisResultsServiceClient _analysisResultsServiceClient;

    public BanMachineService(
        AnalysisService.AnalysisServiceClient analysisServiceClient,
        AnalysisResultsService.AnalysisResultsServiceClient analysisResultsServiceClient)
    {
        _analysisServiceClient = analysisServiceClient;
        _analysisResultsServiceClient = analysisResultsServiceClient;
    }

    public async Task<CheckingId> CreateCheckingAsync(CancellationToken cancellationToken)
    {
        CreateAnalysisResponse response = await _analysisServiceClient
            .CreateAsync(new CreateAnalysisRequest(), cancellationToken: cancellationToken);

        return new CheckingId(response.AnalysisId);
    }

    public async Task AddCheckingDataAsync(
        CheckingId checkingId,
        IReadOnlyCollection<SubmissionData> data,
        CancellationToken cancellationToken)
    {
        var request = new AddAnalysisDataRequest
        {
            AnalysisId = checkingId.ToString(),
            SubmissionData = { data.Select(MapToSubmissionData) },
        };

        AddAnalysisDataResponse response = await _analysisServiceClient
            .AddDataAsync(request, cancellationToken: cancellationToken);

        if (response.ResultCase is not AddAnalysisDataResponse.ResultOneofCase.Success)
        {
            string message = $"Failed to add checking data, ban machine returned {response.ResultCase} response";
            throw new InvalidOperationException(message);
        }
    }

    public async Task StartCheckingAsync(CheckingId checkingId, CancellationToken cancellationToken)
    {
        var request = new StartAnalysisRequest { AnalysisId = checkingId.Value };

        StartAnalysisResponse response = await _analysisServiceClient
            .StartAsync(request, cancellationToken: cancellationToken);

        if (response.ResultCase is not StartAnalysisResponse.ResultOneofCase.Success)
        {
            throw new InvalidOperationException(
                $"Failed to start analysis, result = {response.ResultCase}");
        }
    }

    public async IAsyncEnumerable<BanMachinePairCheckingResult> GetCheckingResultDataAsync(
        CheckingId checkingId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var request = new GetAnalysisResultsDataRequest { AnalysisId = checkingId.ToString() };

        do
        {
            GetAnalysisResultsDataResponse response = await _analysisResultsServiceClient
                .GetAnalysisResultsDataAsync(request, cancellationToken: cancellationToken);

            if (response.ResultCase is not GetAnalysisResultsDataResponse.ResultOneofCase.Success)
            {
                string message =
                    $"Failed to load analysis result data, ban machine service returned {response.ResultCase} response";

                throw new InvalidOperationException(message);
            }

            request.PageToken = response.Success.PageToken;

            foreach (SubmissionPairAnalysisResultData data in response.Success.Data)
            {
                yield return new BanMachinePairCheckingResult(
                    data.FirstSubmissionId.MapToGuid(),
                    data.SecondSubmissionId.MapToGuid(),
                    data.SimilarityScore);
            }
        }
        while (request.PageToken is not null);
    }

    public async IAsyncEnumerable<SimilarCodeBlocks> GetCheckingResultCodeBlocksAsync(
        CheckingResultCodeBlocksRequest query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var request = new GetAnalysisResultCodeBlocksRequest
        {
            AnalysisId = query.CheckingId.ToString(),
            FirstSubmissionId = query.FistSubmissionId.ToString(),
            SecondSubmissionId = query.SecondSubmissionId.ToString(),
            MinimumSimilarityScore = query.MinimumSimilarityScore,
            Cursor = query.Cursor,
        };

        GetAnalysisResultCodeBlocksResponse response = await _analysisResultsServiceClient
            .GetAnalysisResultCodeBlocksAsync(request, cancellationToken: cancellationToken);

        if (response.ResultCase is not GetAnalysisResultCodeBlocksResponse.ResultOneofCase.Success)
        {
            string message =
                $"Failed to load analysis result code blocks, ban machine service returned {response.ResultCase} response";

            throw new InvalidOperationException(message);
        }

        foreach (Asap.BanMachine.Models.SimilarCodeBlocks codeBlocks in response.Success.CodeBlocks)
        {
            var first = new CodeBlock(
                codeBlocks.First.FilePath,
                codeBlocks.First.LineFrom,
                codeBlocks.First.LineTo,
                codeBlocks.First.Content);

            var second = new CodeBlock(
                codeBlocks.Second.FilePath,
                codeBlocks.Second.LineFrom,
                codeBlocks.Second.LineTo,
                codeBlocks.Second.Content);

            yield return new SimilarCodeBlocks(first, second, codeBlocks.SimilarityScore);
        }
    }

    private static Asap.BanMachine.Models.SubmissionData MapToSubmissionData(SubmissionData submissionData)
    {
        return new Asap.BanMachine.Models.SubmissionData
        {
            SubmissionId = submissionData.SubmissionId.ToString(),
            UserId = submissionData.UserId.ToString(),
            AssignmentId = submissionData.AssignmentId.ToString(),
            FileLink = submissionData.FileLink,
        };
    }
}