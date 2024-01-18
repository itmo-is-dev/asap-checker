using Itmo.Dev.Asap.Checker.Application.Abstractions.Core;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Core.Models;
using Itmo.Dev.Asap.Core.Students;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Core.Services;

internal class CoreStudentService : ICoreStudentService
{
    private readonly StudentService.StudentServiceClient _client;
    private readonly ILogger<CoreStudentService> _logger;

    public CoreStudentService(StudentService.StudentServiceClient client, ILogger<CoreStudentService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async IAsyncEnumerable<Student> GetByIdsAsync(
        IEnumerable<Guid> studentIds,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var request = new QueryStudentRequest
        {
            Ids = { studentIds.Select(x => x.ToString()) },
            PageSize = int.MaxValue,
        };

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Querying asap-core for students, ids = {Ids}",
                string.Join(", ", request.Ids));
        }

        QueryStudentResponse response = await _client.QueryAsync(request, cancellationToken: cancellationToken);

        foreach (Asap.Core.Models.Student student in response.Students)
        {
            var model = new Student(
                Id: Guid.Parse(student.User.Id),
                GroupId: Guid.Parse(student.GroupId));

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Received student from asap-core = {Student}", model);
            }

            yield return model;
        }
    }
}