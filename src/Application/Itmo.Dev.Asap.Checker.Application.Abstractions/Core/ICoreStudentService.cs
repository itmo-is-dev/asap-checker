using Itmo.Dev.Asap.Checker.Application.Abstractions.Core.Models;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.Core;

public interface ICoreStudentService
{
    IAsyncEnumerable<Student> GetByIdsAsync(IEnumerable<Guid> studentIds, CancellationToken cancellationToken);
}