using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Migrations;
using Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Repositories;
using Itmo.Dev.Platform.Postgres.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection collection)
    {
        collection.AddPlatformPostgres(builder => builder.BindConfiguration("Infrastructure:Persistence:Postgres"));
        collection.AddPlatformMigrations(typeof(IAssemblyMarker).Assembly);

        collection.AddHostedService<MigrationRunnerService>();

        collection.AddScoped<ICheckingResultRepository, CheckingResultRepository>();
        collection.AddScoped<ISubjectCourseDumpTaskRepository, SubjectCourseDumpTaskRepository>();
        collection.AddScoped<ISubmissionDataRepository, SubmissionDataRepository>();

        collection.AddScoped<IPersistenceContext, PersistenceContext>();

        return collection;
    }
}