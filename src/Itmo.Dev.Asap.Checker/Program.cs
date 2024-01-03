using Itmo.Dev.Asap.Checker.Application.Extensions;
using Itmo.Dev.Asap.Checker.Infrastructure.BanMachine.Extensions;
using Itmo.Dev.Asap.Checker.Infrastructure.Core.Extensions;
using Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Extensions;
using Itmo.Dev.Asap.Checker.Presentation.Grpc.Extensions;
using Itmo.Dev.Asap.Checker.Presentation.Kafka.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.YandexCloud.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();
await builder.AddYandexCloudConfigurationAsync();

builder.Services.AddOptions<JsonSerializerSettings>();
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<JsonSerializerSettings>>().Value);

builder.Services
    .AddApplication()
    .AddInfrastructureBanMachine()
    .AddInfrastructureCore()
    .AddInfrastructurePersistence()
    .AddPresentationGrpc()
    .AddPresentationKafka(builder.Configuration);

builder.Services.AddPlatformBackgroundTasks(configurator => configurator
    .ConfigurePersistence(builder.Configuration.GetSection("Infrastructure:BackgroundTasks:Persistence"))
    .ConfigureScheduling(builder.Configuration.GetSection("Infrastructure:BackgroundTasks:Scheduling"))
    .ConfigureExecution(builder.Configuration.GetSection("Infrastructure:BackgroundTasks:Execution"))
    .AddApplicationBackgroundTasks());

builder.Services.AddPlatformEvents(b => b
    .AddApplicationEvents());

WebApplication app = builder.Build();

app.UsePresentationGrpc();

await app.RunAsync();