using Itmo.Dev.Asap.Checker.Application.Abstractions.Core;
using Itmo.Dev.Asap.Core.SubjectCourses;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Core.Services;

public class CoreSubjectCourseService : ICoreSubjectCourseService
{
    private readonly SubjectCourseService.SubjectCourseServiceClient _client;

    public CoreSubjectCourseService(SubjectCourseService.SubjectCourseServiceClient client)
    {
        _client = client;
    }

    public async Task<bool> SubjectCourseExistsAsync(Guid subjectCourseId, CancellationToken cancellationToken)
    {
        var request = new QueryRequest { SubjectCourseIds = { subjectCourseId.ToString() } };
        QueryResponse response = await _client.QueryAsync(request, cancellationToken: cancellationToken);

        return response.SubjectCourses is not [];
    }
}