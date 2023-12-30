namespace Itmo.Dev.Asap.Checker.Application.Abstractions.Core;

public interface ICoreSubjectCourseService
{
    Task<bool> SubjectCourseExistsAsync(Guid subjectCourseId, CancellationToken cancellationToken);
}