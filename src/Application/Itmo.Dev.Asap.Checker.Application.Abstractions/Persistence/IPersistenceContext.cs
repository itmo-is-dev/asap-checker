using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence;

public interface IPersistenceContext
{
    ICheckingResultRepository CheckingResults { get; }

    ISubjectCourseDumpTaskRepository SubjectCourseDumpTasks { get; }

    ISubmissionDataRepository SubmissionData { get; }
}