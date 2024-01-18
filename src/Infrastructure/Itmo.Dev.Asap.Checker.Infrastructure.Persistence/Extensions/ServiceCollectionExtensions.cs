using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Migrations;
using Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Plugins;
using Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Repositories;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection collection)
    {
        collection.AddPlatformPostgres(builder => builder.BindConfiguration("Infrastructure:Persistence:Postgres"));
        collection.AddPlatformMigrations(typeof(IAssemblyMarker).Assembly);

        collection.AddHostedService<MigrationRunnerService>();
        collection.AddSingleton<IDataSourcePlugin, MappingPlugin>();

        collection.AddScoped<ICheckingResultRepository, CheckingResultRepository>();
        collection.AddScoped<ISubmissionDataRepository, SubmissionDataRepository>();

        collection.AddScoped<IPersistenceContext, PersistenceContext>();

        return collection;
    }
}