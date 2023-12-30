using FluentMigrator;
using Itmo.Dev.Platform.Postgres.Migrations;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Migrations;

#pragma warning disable SA1649

[Migration(1702144702, "Initial")]
public class Initial : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider) =>
    """
    create type code_block as
    (
        code_block_file_path text ,
        code_block_line_from int ,
        code_block_line_to int ,
        code_block_content text
    );   

    create table subject_course_dump_tasks
    (
        subject_course_id uuid not null ,
        task_id bigint not null primary key
    );

    create index subject_course_id_idx on subject_course_dump_tasks(subject_course_id);

    create table submission_data
    (
        submission_id uuid not null ,
        user_id uuid not null ,
        assignment_id uuid not null ,
        task_id bigint not null ,
        submission_data_file_link text not null ,
        
        primary key (user_id, assignment_id, task_id)
    );

    create index submission_data_task_id_idx on submission_data(task_id);

    create table checking_results
    (
        task_id bigint not null ,
        checking_result_first_submission_id uuid not null ,
        checking_result_second_submission_id uuid not null ,
        checking_result_similarity_score float8 not null ,
        
        primary key (task_id, checking_result_first_submission_id, checking_result_second_submission_id)
    );

    create index on checking_results(task_id, 
                                    checking_result_first_submission_id,
                                    checking_result_second_submission_id,
                                    checking_result_similarity_score desc);

    create table checking_result_code_blocks
    (
        checking_result_code_block_id bigint primary key generated always as identity ,
        task_id bigint not null ,
        checking_result_first_submission_id uuid not null ,
        checking_result_second_submission_id uuid not null , 
        checking_result_code_block_first code_block not null ,
        checking_result_code_block_second code_block not null ,
        checking_result_code_block_similarity_score float8 not null 
    );

    create index checking_result_code_blocks_idx
        on checking_result_code_blocks(task_id, 
                                       checking_result_first_submission_id,
                                       checking_result_second_submission_id,
                                       checking_result_code_block_similarity_score desc);
    """;

    protected override string GetDownSql(IServiceProvider serviceProvider) =>
    """
    drop table subject_course_dump_tasks;
    drop table submission_data;
    """;
}