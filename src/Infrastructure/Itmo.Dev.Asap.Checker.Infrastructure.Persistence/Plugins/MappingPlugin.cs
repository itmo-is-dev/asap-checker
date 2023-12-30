using Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;
using Itmo.Dev.Platform.Postgres.Plugins;
using Npgsql;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Plugins;

public class MappingPlugin : IDataSourcePlugin
{
    public void Configure(NpgsqlDataSourceBuilder builder)
    {
        builder.MapComposite<CodeBlock>();
    }
}