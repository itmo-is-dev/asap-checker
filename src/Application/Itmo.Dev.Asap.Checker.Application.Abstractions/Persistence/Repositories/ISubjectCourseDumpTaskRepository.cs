using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.Checker.Application.Models.SubjectCourses;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;

public interface ISubjectCourseDumpTaskRepository
{
    IAsyncEnumerable<SubjectCourseDumpTask> QueryAsync(
        SubjectCourseDumpTaskQuery query,
        CancellationToken cancellationToken);

    Task AddAsync(SubjectCourseDumpTask task, CancellationToken cancellationToken);
}