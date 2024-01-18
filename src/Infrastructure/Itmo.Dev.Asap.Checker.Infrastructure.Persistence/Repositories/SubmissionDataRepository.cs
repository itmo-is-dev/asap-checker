using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.Checker.Application.Models;
using Itmo.Dev.Asap.Checker.Application.Models.Submissions;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Repositories;

public sealed class SubmissionDataRepository : ISubmissionDataRepository
{
    private readonly IPostgresConnectionProvider _connectionProvider;

    public SubmissionDataRepository(IPostgresConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async IAsyncEnumerable<SubmissionData> QueryAsync(
        SubmissionDataQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        select submission_id, 
               user_id, 
               assignment_id,
               task_id, 
               submission_data_file_link
        from submission_data
        where 
            (cardinality(:task_ids) = 0 or task_id = any (:task_ids))
            and (cardinality(:submission_ids) = 0 or submission_id = any (:submission_ids))
            and (:should_ignore_cursor or submission_id > :submission_id_cursor)
        order by submission_id
        limit :limit;
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("task_ids", query.CheckingIds.Select(x => x.Value).ToArray())
            .AddParameter("submission_ids", query.SubmissionIds)
            .AddParameter("should_ignore_cursor", query.SubmissionIdCursor is null)
            .AddParameter("submission_id_cursor", query.SubmissionIdCursor ?? Guid.Empty)
            .AddParameter("limit", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int submissionId = reader.GetOrdinal("submission_id");
        int userId = reader.GetOrdinal("user_id");
        int assignmentId = reader.GetOrdinal("assignment_id");
        int taskId = reader.GetOrdinal("task_id");
        int fileLink = reader.GetOrdinal("submission_data_file_link");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new SubmissionData(
                SubmissionId: reader.GetGuid(submissionId),
                UserId: reader.GetGuid(userId),
                AssignmentId: reader.GetGuid(assignmentId),
                CheckingId: new CheckingId(reader.GetInt64(taskId)),
                FileLink: reader.GetString(fileLink));
        }
    }

    public async Task AddRangeAsync(
        IReadOnlyCollection<SubmissionData> submissionData,
        CancellationToken cancellationToken)
    {
        const string sql = """
        insert into submission_data(submission_id, user_id, assignment_id, task_id, submission_data_file_link)
        select * from unnest(:submission_ids, :user_ids, :assignment_ids, :task_ids, :file_links)
        on conflict on constraint submission_data_pkey do update set submission_data_file_link = excluded.submission_data_file_link;
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("submission_ids", submissionData.Select(x => x.SubmissionId).ToArray())
            .AddParameter("user_ids", submissionData.Select(x => x.UserId).ToArray())
            .AddParameter("assignment_ids", submissionData.Select(x => x.AssignmentId).ToArray())
            .AddParameter("task_ids", submissionData.Select(x => x.CheckingId.Value).ToArray())
            .AddParameter("file_links", submissionData.Select(x => x.FileLink).ToArray());

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}