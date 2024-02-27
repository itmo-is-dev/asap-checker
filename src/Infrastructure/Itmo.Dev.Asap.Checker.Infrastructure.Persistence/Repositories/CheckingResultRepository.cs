using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.Checker.Application.Models;
using Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Newtonsoft.Json;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Repositories;

public class CheckingResultRepository : ICheckingResultRepository
{
    private readonly IPostgresConnectionProvider _connectionProvider;
    private readonly JsonSerializerSettings _serializerSettings;

    public CheckingResultRepository(
        IPostgresConnectionProvider connectionProvider,
        JsonSerializerSettings serializerSettings)
    {
        _connectionProvider = connectionProvider;
        _serializerSettings = serializerSettings;
    }

    public async IAsyncEnumerable<SubmissionPairCheckingResult> QueryDataAsync(
        CheckingResultDataQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        select checking_result_assignment_id,
               checking_result_first_submission_id,
               checking_result_first_user_id,
               checking_result_first_group_id,
               checking_result_second_submission_id, 
               checking_result_second_user_id, 
               checking_result_second_group_id, 
               checking_result_similarity_score
        from checking_results
        where 
            task_id = :task_id 
            and (cardinality(:assignment_ids) = 0 or checking_result_assignment_id = any (:assignment_ids))
            and (cardinality(:group_ids) = 0 or checking_result_first_group_id = any (:group_ids) or checking_result_second_group_id = any (:group_ids))
            and (:should_ignore_first_filter or checking_result_first_submission_id >= :first_submission_id)
            and (:should_ignore_first_filter 
                 or :should_ignore_second_filter 
                 or  checking_result_first_submission_id != :first_submission_id
                 or checking_result_second_submission_id > :second_submission_id)
        order by checking_result_similarity_score desc, checking_result_first_submission_id, checking_result_second_submission_id
        limit :page_size
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("task_id", query.CheckingId.Value)
            .AddParameter("assignment_ids", query.AssignmentIds)
            .AddParameter("group_ids", query.GroupIds)
            .AddParameter("should_ignore_first_filter", query.FirstSubmissionId is null)
            .AddParameter("should_ignore_second_filter", query.SecondSubmissionId is null)
            .AddParameter("first_submission_id", query.FirstSubmissionId ?? Guid.Empty)
            .AddParameter("second_submission_id", query.SecondSubmissionId ?? Guid.Empty)
            .AddParameter("page_size", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int assignmentId = reader.GetOrdinal("checking_result_assignment_id");
        int firstSubmissionId = reader.GetOrdinal("checking_result_first_submission_id");
        int firstUserId = reader.GetOrdinal("checking_result_first_user_id");
        int firstGroupId = reader.GetOrdinal("checking_result_first_group_id");
        int secondSubmissionId = reader.GetOrdinal("checking_result_second_submission_id");
        int secondUserId = reader.GetOrdinal("checking_result_second_user_id");
        int secondGroupId = reader.GetOrdinal("checking_result_second_group_id");
        int similarityScore = reader.GetOrdinal("checking_result_similarity_score");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new SubmissionPairCheckingResult(
                AssignmentId: reader.GetGuid(assignmentId),
                FirstSubmission: new SubmissionInfo(
                    Id: reader.GetGuid(firstSubmissionId),
                    UserId: reader.GetGuid(firstUserId),
                    GroupId: reader.GetGuid(firstGroupId)),
                SecondSubmission: new SubmissionInfo(
                    Id: reader.GetGuid(secondSubmissionId),
                    UserId: reader.GetGuid(secondUserId),
                    GroupId: reader.GetGuid(secondGroupId)),
                SimilarityScore: reader.GetDouble(similarityScore));
        }
    }

    public async IAsyncEnumerable<SimilarCodeBlocks> QueryCodeBlocksAsync(
        CheckingResultCodeBlocksQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        select checking_result_code_block_first::code_block,
               checking_result_code_block_second::code_block, 
               checking_result_code_block_similarity_score
        from checking_result_code_blocks
        where 
            task_id = :task_id
            and checking_result_first_submission_id = :first_submission_id
            and checking_result_second_submission_id = :second_submission_id
        order by checking_result_code_block_similarity_score desc
        offset :cursor
        limit :page_size;
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("task_id", query.CheckingId.Value)
            .AddParameter("first_submission_id", query.FirstSubmissionId)
            .AddParameter("second_submission_id", query.SecondSubmissionId)
            .AddParameter("cursor", query.Cursor)
            .AddParameter("page_size", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new SimilarCodeBlocks(
                First: reader.GetFieldValue<CodeBlock[]>(0),
                Second: reader.GetFieldValue<CodeBlock[]>(1),
                SimilarityScore: reader.GetDouble(2));
        }
    }

    public async Task AddCheckingResultAsync(
        CheckingId checkingId,
        SubmissionPairCheckingResult result,
        CancellationToken cancellationToken)
    {
        const string sql = """
        insert into checking_results(task_id, 
                                     checking_result_assignment_id,
                                     checking_result_first_submission_id,
                                     checking_result_first_user_id,
                                     checking_result_first_group_id,
                                     checking_result_second_submission_id,
                                     checking_result_second_user_id,
                                     checking_result_second_group_id,
                                     checking_result_similarity_score) 
        values (:task_id, 
                :assignment_id,
                :first_submission_id, 
                :first_user_id, 
                :first_group_id,
                :second_submission_id,
                :second_user_id,
                :second_group_id,
                :similarity_score);
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("task_id", checkingId.Value)
            .AddParameter("assignment_id", result.AssignmentId)
            .AddParameter("first_submission_id", result.FirstSubmission.Id)
            .AddParameter("first_user_id", result.FirstSubmission.UserId)
            .AddParameter("first_group_id", result.FirstSubmission.GroupId)
            .AddParameter("second_submission_id", result.SecondSubmission.Id)
            .AddParameter("second_user_id", result.SecondSubmission.UserId)
            .AddParameter("second_group_id", result.SecondSubmission.GroupId)
            .AddParameter("similarity_score", result.SimilarityScore);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AddCheckingResultCodeBlocksAsync(
        CheckingId checkingId,
        Guid firstSubmissionId,
        Guid secondSubmissionId,
        IReadOnlyCollection<SimilarCodeBlocks> codeBlocks,
        CancellationToken cancellationToken)
    {
        const string sql = """
        insert into checking_result_code_blocks(task_id, 
                                                checking_result_first_submission_id,
                                                checking_result_second_submission_id,
                                                checking_result_code_block_first,
                                                checking_result_code_block_second,
                                                checking_result_code_block_similarity_score)
        select  :task_id, 
                :fist_submission_id, 
                :second_submission_id,
                json_populate_recordset(null::code_block, s.first_code_blocks),
                json_populate_recordset(null::code_block, s.second_code_blocks),
                s.similarity_scores 
        from unnest(:first_code_blocks, :second_code_blocks, :similarity_scores) as s(first_code_blocks, second_code_blocks, similarity_scores);
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("task_id", checkingId.Value)
            .AddParameter("fist_submission_id", firstSubmissionId)
            .AddParameter("second_submission_id", secondSubmissionId)
            .AddJsonArrayParameter(
                "first_code_blocks",
                codeBlocks.Select(x => x.First),
                _serializerSettings)
            .AddJsonArrayParameter(
                "second_code_blocks",
                codeBlocks.Select(x => x.Second),
                _serializerSettings)
            .AddParameter("similarity_scores", codeBlocks.Select(x => x.SimilarityScore).ToArray());
    }
}