using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Persistence;

public class PersistenceContext : IPersistenceContext
{
    public PersistenceContext(
        ICheckingResultRepository checkingResults,
        ISubmissionDataRepository submissionData)
    {
        CheckingResults = checkingResults;
        SubmissionData = submissionData;
    }

    public ICheckingResultRepository CheckingResults { get; }

    public ISubmissionDataRepository SubmissionData { get; }
}