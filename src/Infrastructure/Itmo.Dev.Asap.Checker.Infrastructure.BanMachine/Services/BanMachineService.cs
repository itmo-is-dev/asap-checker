using Itmo.Dev.Asap.BanMachine;
using Itmo.Dev.Asap.BanMachine.Models;
using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;
using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Services;
using Itmo.Dev.Asap.Checker.Application.Models;
using Itmo.Dev.Asap.Checker.Infrastructure.BanMachine.Mapping;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using CodeBlock = Itmo.Dev.Asap.Checker.Application.Models.CheckingResults.CodeBlock;
using SimilarCodeBlocks = Itmo.Dev.Asap.Checker.Application.Models.CheckingResults.SimilarCodeBlocks;
using SubmissionData = Itmo.Dev.Asap.Checker.Application.Models.Submissions.SubmissionData;

namespace Itmo.Dev.Asap.Checker.Infrastructure.BanMachine.Services;

internal class BanMachineService : IBanMachineService
{
    private readonly AnalysisService.AnalysisServiceClient _analysisServiceClient;
    private readonly AnalysisResultsService.AnalysisResultsServiceClient _analysisResultsServiceClient;
    private readonly ILogger<BanMachineService> _logger;

    public BanMachineService(
        AnalysisService.AnalysisServiceClient analysisServiceClient,
        AnalysisResultsService.AnalysisResultsServiceClient analysisResultsServiceClient,
        ILogger<BanMachineService> logger)
    {
        _analysisServiceClient = analysisServiceClient;
        _analysisResultsServiceClient = analysisResultsServiceClient;
        _logger = logger;
    }

    public async Task<AnalysisId> CreateAnalysisAsync(CancellationToken cancellationToken)
    {
        CreateAnalysisResponse response = await _analysisServiceClient
            .CreateAsync(new CreateAnalysisRequest(), cancellationToken: cancellationToken);

        return new AnalysisId(response.AnalysisId);
    }

    public async Task AddAnalysisDataAsync(
        AnalysisId analysisId,
        IReadOnlyCollection<SubmissionData> data,
        CancellationToken cancellationToken)
    {
        var request = new AddAnalysisDataRequest
        {
            AnalysisId = analysisId.ToString(),
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

    public async Task StartAnalysisAsync(AnalysisId analysisId, CancellationToken cancellationToken)
    {
        var request = new StartAnalysisRequest { AnalysisId = analysisId.Value };

        StartAnalysisResponse response = await _analysisServiceClient
            .StartAsync(request, cancellationToken: cancellationToken);

        if (response.ResultCase is not StartAnalysisResponse.ResultOneofCase.Success)
        {
            throw new InvalidOperationException(
                $"Failed to start analysis, result = {response.ResultCase}");
        }
    }

    public async IAsyncEnumerable<BanMachinePairAnalysisResult> GetAnalysisResultsDataAsync(
        AnalysisId analysisId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var request = new GetAnalysisResultsDataRequest { AnalysisId = analysisId.ToString() };

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
                _logger.LogTrace(
                    "Received pair checking result, first submission = {FirstSubmissionId}, second submission = {SecondSubmissionId}, score = {Score}",
                    data.FirstSubmissionId,
                    data.SecondSubmissionId,
                    data.SimilarityScore);

                yield return new BanMachinePairAnalysisResult(
                    data.FirstSubmissionId.MapToGuid(),
                    data.SecondSubmissionId.MapToGuid(),
                    SimilarityScore: Math.Round(data.SimilarityScore, 2));
            }
        }
        while (request.PageToken is not null);
    }

    public async IAsyncEnumerable<SimilarCodeBlocks> GetAnalysisResultCodeBlocksAsync(
        CheckingResultCodeBlocksRequest query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var request = new GetAnalysisResultCodeBlocksRequest
        {
            AnalysisId = query.AnalysisId.ToString(),
            FirstSubmissionId = query.FirstSubmissionId.ToString(),
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

        _logger.LogTrace(
            "Received code blocks, count = {Count}",
            response.Success.CodeBlocks.Count);

        foreach (Asap.BanMachine.Models.SimilarCodeBlocks codeBlocks in response.Success.CodeBlocks)
        {
            yield return new SimilarCodeBlocks(
                codeBlocks.First.Select(MapToCodeBlock).ToArray(),
                codeBlocks.Second.Select(MapToCodeBlock).ToArray(),
                SimilarityScore: Math.Round(codeBlocks.SimilarityScore, 2));
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

    private static CodeBlock MapToCodeBlock(Itmo.Dev.Asap.BanMachine.Models.CodeBlock codeBlock)
    {
        return new CodeBlock(
            codeBlock.FilePath,
            codeBlock.LineFrom,
            codeBlock.LineTo,
            codeBlock.Content);
    }
}