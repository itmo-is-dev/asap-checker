using Itmo.Dev.Asap.Checker.Application.Abstractions.Core;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Core.Models;
using Itmo.Dev.Asap.Core.Students;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Core.Services;

internal class CoreStudentService : ICoreStudentService
{
    private readonly StudentService.StudentServiceClient _client;

    public CoreStudentService(StudentService.StudentServiceClient client)
    {
        _client = client;
    }

    public async IAsyncEnumerable<Student> GetByIdsAsync(
        IEnumerable<Guid> studentIds,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var request = new QueryStudentRequest { Ids = { studentIds.Select(x => x.ToString()) } };
        QueryStudentResponse response = await _client.QueryAsync(request, cancellationToken: cancellationToken);

        foreach (Asap.Core.Models.Student student in response.Students)
        {
            yield return new Student(
                Id: Guid.Parse(student.User.Id),
                GroupId: Guid.Parse(student.GroupId));
        }
    }
}