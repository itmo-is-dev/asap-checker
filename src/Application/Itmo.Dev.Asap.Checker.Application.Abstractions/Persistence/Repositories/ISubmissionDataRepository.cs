using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.Checker.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;

public interface ISubmissionDataRepository
{
    IAsyncEnumerable<SubmissionData> QueryAsync(SubmissionDataQuery query, CancellationToken cancellationToken);

    Task AddRangeAsync(IReadOnlyCollection<SubmissionData> submissionData, CancellationToken cancellationToken);
}