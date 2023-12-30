using Itmo.Dev.Asap.Checker.Application.Abstractions.Github.Results;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.Github;

public interface IGithubSubjectCourseService
{
    Task<StartContentDumpResult> StartContentDumpAsync(Guid subjectCourseId, CancellationToken cancellationToken);
}