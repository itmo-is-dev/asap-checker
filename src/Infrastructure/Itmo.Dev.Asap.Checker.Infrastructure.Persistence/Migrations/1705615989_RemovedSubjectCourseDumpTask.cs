using FluentMigrator;
using Itmo.Dev.Platform.Postgres.Migrations;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Migrations;

#pragma warning disable SA1649

[Migration(1705615989, "Removed subject_coruse_dump_task")]
public class RemovedSubjectCourseDumpTask : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider) =>
    """
    drop table subject_course_dump_tasks;
    """;

    protected override string GetDownSql(IServiceProvider serviceProvider) =>
    """
    create table subject_course_dump_tasks
    (
        subject_course_id uuid not null ,
        task_id bigint not null primary key
    );
    """;
}