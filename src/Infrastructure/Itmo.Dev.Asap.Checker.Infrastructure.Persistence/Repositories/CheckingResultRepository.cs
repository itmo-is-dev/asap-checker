using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Repositories;

public class CheckingResultRepository : ICheckingResultRepository
{
    private readonly IPostgresConnectionProvider _connectionProvider;

    public CheckingResultRepository(IPostgresConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async IAsyncEnumerable<SubmissionPairCheckingResult> QueryDataAsync(
        CheckingResultDataQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        select checking_result_first_submission_id,
               checking_result_second_submission_id, 
               checking_result_similarity_score
        from checking_results
        where 
            task_id = :task_id 
            and (:should_ignore_first_filter or checking_result_first_submission_id >= :first_submission_id)
            and (:should_ignore_first_filter 
                 or :should_ignore_second_filter 
                 or  checking_result_first_submission_id != :first_submission_id
                 or checking_result_second_submission_id > :second_submission_id)
        order by checking_result_first_submission_id, checking_result_second_submission_id, checking_result_similarity_score desc
        limit :page_size
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("task_id", query.TaskId)
            .AddParameter("should_ignore_first_filter", query.FirstSubmissionId is null)
            .AddParameter("should_ignore_second_filter", query.SecondSubmissionId is null)
            .AddParameter("first_submission_id", query.FirstSubmissionId ?? Guid.Empty)
            .AddParameter("second_submission_id", query.SecondSubmissionId ?? Guid.Empty)
            .AddParameter("page_size", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new SubmissionPairCheckingResult(
                FirstSubmissionId: reader.GetGuid(0),
                SecondSubmissionId: reader.GetGuid(1),
                SimilarityScore: reader.GetDouble(2));
        }
    }

    public async IAsyncEnumerable<SimilarCodeBlocks> QueryCodeBlocksAsync(
        CheckingResultCodeBlocksQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        select checking_result_code_block_first,
               checking_result_code_block_second, 
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
            .AddParameter("task_id", query.TaskId)
            .AddParameter("first_submission_id", query.FirstSubmissionId)
            .AddParameter("second_submission_id", query.SecondSubmissionId)
            .AddParameter("cursor", query.Cursor)
            .AddParameter("page_size", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new SimilarCodeBlocks(
                First: reader.GetFieldValue<CodeBlock>(0),
                Second: reader.GetFieldValue<CodeBlock>(1),
                SimilarityScore: reader.GetDouble(2));
        }
    }

    public async Task AddCheckingResultAsync(
        long taskId,
        SubmissionPairCheckingResult result,
        CancellationToken cancellationToken)
    {
        const string sql = """
        insert into checking_results
        (task_id, checking_result_first_submission_id, checking_result_second_submission_id, checking_result_similarity_score) 
        values (:task_id, :first_submission_id, :second_submission_id, :similarity_score);
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("task_id", taskId)
            .AddParameter("first_submission_id", result.FirstSubmissionId)
            .AddParameter("second_submission_id", result.SecondSubmissionId)
            .AddParameter("similarity_score", result.SimilarityScore);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AddCheckingResultCodeBlocksAsync(
        long taskId,
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
        select :task_id, :fist_submission_id, :second_submission_id, * 
        from unnest(:first_code_blocks, :second_code_blocks, :similarity_scores);
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("task_id", taskId)
            .AddParameter("fist_submission_id", firstSubmissionId)
            .AddParameter("second_submission_id", secondSubmissionId)
            .AddParameter("first_code_blocks", codeBlocks.Select(x => x.First).ToArray())
            .AddParameter("second_code_blocks", codeBlocks.Select(x => x.Second).ToArray())
            .AddParameter("similarity_scores", codeBlocks.Select(x => x.SimilarityScore).ToArray());
    }
}