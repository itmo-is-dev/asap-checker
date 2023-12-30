using Itmo.Dev.Asap.Checker.Application.Models.SubjectCourses;

namespace Itmo.Dev.Asap.Checker.Application.Contracts.Checking;

public static class GetCheckingTasks
{
    public sealed record Request(Guid SubjectCourseId, bool IsActive, int PageSize, PageToken? PageToken);

    public sealed record Response(IReadOnlyCollection<SubjectCourseCheckingTask> Tasks, PageToken? PageToken);

    public sealed record PageToken(DateTimeOffset CreatedAt);
}