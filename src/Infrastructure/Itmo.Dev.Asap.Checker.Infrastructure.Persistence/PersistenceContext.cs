using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Persistence;

public class PersistenceContext : IPersistenceContext
{
    public PersistenceContext(
        ICheckingResultRepository checkingResults,
        ISubjectCourseDumpTaskRepository subjectCourseDumpTasks,
        ISubmissionDataRepository submissionData)
    {
        CheckingResults = checkingResults;
        SubjectCourseDumpTasks = subjectCourseDumpTasks;
        SubmissionData = submissionData;
    }

    public ICheckingResultRepository CheckingResults { get; }

    public ISubjectCourseDumpTaskRepository SubjectCourseDumpTasks { get; }

    public ISubmissionDataRepository SubmissionData { get; }
}