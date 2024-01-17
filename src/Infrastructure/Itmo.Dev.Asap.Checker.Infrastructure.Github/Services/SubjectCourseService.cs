using Itmo.Dev.Asap.Checker.Application.Abstractions.Github;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Github.Results;
using Itmo.Dev.Asap.Github.SubjectCourses;
using static Itmo.Dev.Asap.Github.SubjectCourses.StartContentDumpResponse;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Github.Services;

public class SubjectCourseService : IGithubSubjectCourseService
{
    private readonly GithubSubjectCourseService.GithubSubjectCourseServiceClient _client;

    public SubjectCourseService(
        GithubSubjectCourseService.GithubSubjectCourseServiceClient client)
    {
        _client = client;
    }

    public async Task<StartContentDumpResult> StartContentDumpAsync(
        Guid subjectCourseId,
        CancellationToken cancellationToken)
    {
        var request = new StartContentDumpRequest { SubjectCourseId = subjectCourseId.ToString() };

        StartContentDumpResponse response = await _client
            .StartContentDumpAsync(request, cancellationToken: cancellationToken);

        return response.ResultCase switch
        {
            ResultOneofCase.Success
                => new StartContentDumpResult.Success(response.Success.TaskId),

            ResultOneofCase.AlreadyRunning => new StartContentDumpResult.AlreadyInProgress(),

            ResultOneofCase.SubjectCourseNotFound
                => new StartContentDumpResult.SubjectCourseNotFound(),

            _ or ResultOneofCase.None => throw new ArgumentOutOfRangeException(),
        };
    }
}