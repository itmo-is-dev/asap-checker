using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.Checker.Application.Models.SubjectCourses;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Repositories;

public sealed class SubjectCourseDumpTaskRepository : ISubjectCourseDumpTaskRepository
{
    private readonly IPostgresConnectionProvider _connectionProvider;

    public SubjectCourseDumpTaskRepository(IPostgresConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async IAsyncEnumerable<SubjectCourseDumpTask> QueryAsync(
        SubjectCourseDumpTaskQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        select subject_course_id, task_id
        from subject_course_dump_tasks
        where 
            (cardinality(:subject_course_ids) = 0 or subject_course_id = any(:subject_course_ids))
            and (cardinality(:task_ids) = 0 or task_id = any(:task_ids))
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("subject_course_ids", query.SubjectCourseIds)
            .AddParameter("task_ids", query.TaskIds);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new SubjectCourseDumpTask(
                SubjectCourseId: reader.GetGuid(0),
                TaskId: reader.GetInt64(1));
        }
    }

    public async Task AddAsync(SubjectCourseDumpTask task, CancellationToken cancellationToken)
    {
        const string sql = """
        insert into subject_course_dump_tasks(subject_course_id, task_id)
        values (:subject_course_id, :task_id);
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("subject_course_id", task.SubjectCourseId)
            .AddParameter("task_id", task.TaskId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}