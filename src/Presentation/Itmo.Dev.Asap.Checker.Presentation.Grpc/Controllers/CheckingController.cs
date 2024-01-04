using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Itmo.Dev.Asap.Checker.Application.Contracts.Checking;
using Itmo.Dev.Asap.Checker.Models;
using Itmo.Dev.Asap.Checker.Presentation.Grpc.Mapping;
using Newtonsoft.Json;
using CodeBlock = Itmo.Dev.Asap.Checker.Application.Models.CheckingResults.CodeBlock;
using SimilarCodeBlocks = Itmo.Dev.Asap.Checker.Models.SimilarCodeBlocks;
using SubmissionInfo = Itmo.Dev.Asap.Checker.Application.Models.CheckingResults.SubmissionInfo;

namespace Itmo.Dev.Asap.Checker.Presentation.Grpc.Controllers;

public class CheckingController : CheckingService.CheckingServiceBase
{
    private readonly ICheckingService _service;

    public CheckingController(ICheckingService service)
    {
        _service = service;
    }

    public override async Task<StartResponse> Start(StartRequest request, ServerCallContext context)
    {
        var applicationRequest = new StartChecking.Request(request.SubjectCourseId.MapToGuid());

        StartChecking.Response applicationResponse = await _service
            .StartCheckingAsync(applicationRequest, context.CancellationToken);

        return applicationResponse switch
        {
            StartChecking.Response.Success => new StartResponse { Success = new() },
            StartChecking.Response.AlreadyInProgress => new StartResponse { AlreadyInProgress = new() },
            StartChecking.Response.SubjectCourseNotFound => new StartResponse { SubjectCourseNotFound = new() },
            _ => throw new RpcException(new Status(StatusCode.Internal, "Operation yielded unexpected result")),
        };
    }

    public override async Task<GetTasksResponse> GetTasks(GetTasksRequest request, ServerCallContext context)
    {
        GetCheckingTasks.PageToken? pageToken = request.PageToken is not null
            ? JsonConvert.DeserializeObject<GetCheckingTasks.PageToken>(request.PageToken)
            : null;

        var applicationRequest = new GetCheckingTasks.Request(
            request.SubjectCourseId.MapToGuid(),
            request.IsActive,
            request.PageSize,
            pageToken);

        GetCheckingTasks.Response applicationResponse = await _service
            .GetCheckingTasksAsync(applicationRequest, context.CancellationToken);

        IEnumerable<CheckingTask> tasks = applicationResponse.Tasks.Select(task => new CheckingTask
        {
            TaskId = task.Id,
            CreatedAt = Timestamp.FromDateTimeOffset(task.CreatedAt),
        });

        return new GetTasksResponse
        {
            Tasks = { tasks },

            PageToken = applicationResponse.PageToken is not null
                ? JsonConvert.SerializeObject(applicationResponse.PageToken)
                : null,
        };
    }

    public override async Task<GetResultsResponse> GetResults(GetResultsRequest request, ServerCallContext context)
    {
        GetCheckingTaskResults.PageToken? pageToken = request.PageToken is not null
            ? JsonConvert.DeserializeObject<GetCheckingTaskResults.PageToken>(request.PageToken)
            : null;

        var applicationRequest = new GetCheckingTaskResults.Request(
            request.TaskId,
            request.AssignmentIds.Select(x => x.MapToGuid()).ToArray(),
            request.GroupIds.Select(x => x.MapToGuid()).ToArray(),
            request.PageSize,
            pageToken);

        GetCheckingTaskResults.Response applicationResponse = await _service
            .GetCheckingResultsAsync(applicationRequest, context.CancellationToken);

        IEnumerable<CheckingResult> results = applicationResponse.Results.Select(result => new CheckingResult
        {
            FirstSubmission = Map(result.FirstSubmission),
            SecondSubmission = Map(result.SecondSubmission),
            SimilarityScore = result.SimilarityScore,
            AssignmentId = result.AssignmentId.ToString(),
        });

        return new GetResultsResponse
        {
            Results = { results },

            PageToken = applicationResponse.PageToken is not null
                ? JsonConvert.SerializeObject(applicationResponse.PageToken)
                : null,
        };
    }

    public override async Task<GetResultCodeBlocksResponse> GetResultCodeBlocks(
        GetResultCodeBlocksRequest request,
        ServerCallContext context)
    {
        var applicationRequest = new GetCheckingTaskResultCodeBlocks.Request(
            request.TaskId,
            request.FirstSubmissionId.MapToGuid(),
            request.SecondSubmissionId.MapToGuid(),
            request.PageSize,
            request.Cursor);

        GetCheckingTaskResultCodeBlocks.Response applicationResponse = await _service
            .GetCheckingResultCodeBlocksAsync(applicationRequest, context.CancellationToken);

        IEnumerable<SimilarCodeBlocks> codeBlocks = applicationResponse.CodeBlocks.Select(block => new SimilarCodeBlocks
        {
            First = Map(block.First),
            Second = Map(block.Second),
            SimilarityScore = block.SimilarityScore,
        });

        var response = new GetResultCodeBlocksResponse { CodeBlocks = { codeBlocks } };
        response.HasNext = response.CodeBlocks.Count.Equals(request.PageSize);

        return response;
    }

    private static Itmo.Dev.Asap.Checker.Models.CodeBlock Map(CodeBlock codeBlock)
    {
        return new Models.CodeBlock
        {
            FilePath = codeBlock.FilePath,
            LineFrom = codeBlock.LineFrom,
            LineTo = codeBlock.LineTo,
            Content = codeBlock.Content,
        };
    }

    private static Itmo.Dev.Asap.Checker.Models.SubmissionInfo Map(SubmissionInfo submission)
    {
        return new Models.SubmissionInfo
        {
            SubmissionId = submission.Id.ToString(),
            UserId = submission.UserId.ToString(),
            GroupId = submission.GroupId.ToString(),
        };
    }
}