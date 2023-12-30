using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence;
using Itmo.Dev.Asap.Checker.Application.Contracts.Submissions;
using Itmo.Dev.Platform.Events;

namespace Itmo.Dev.Asap.Checker.Application.Submissions;

public class SubmissionDataAddedHandler : IEventHandler<SubmissionDataAddedEvent>
{
    private readonly IPersistenceContext _context;

    public SubmissionDataAddedHandler(IPersistenceContext context)
    {
        _context = context;
    }

    public async ValueTask HandleAsync(SubmissionDataAddedEvent evt, CancellationToken cancellationToken)
    {
        await _context.SubmissionData.AddRangeAsync(evt.SubmissionData, cancellationToken);
    }
}