using Grpc.Net.ClientFactory;
using Itmo.Dev.Asap.BanMachine;
using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Services;
using Itmo.Dev.Asap.Checker.Infrastructure.BanMachine.Options;
using Itmo.Dev.Asap.Checker.Infrastructure.BanMachine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.Checker.Infrastructure.BanMachine.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureBanMachine(this IServiceCollection collection)
    {
        collection
            .AddOptions<BanMachineClientOptions>()
            .BindConfiguration("Infrastructure:BanMachine");

        collection.AddGrpcClient<AnalysisService.AnalysisServiceClient>(ConfigureAddress);
        collection.AddGrpcClient<AnalysisResultsService.AnalysisResultsServiceClient>(ConfigureAddress);
        collection.AddScoped<IBanMachineService, BanMachineService>();

        return collection;

        static void ConfigureAddress(IServiceProvider sp, GrpcClientFactoryOptions o)
        {
            IOptions<BanMachineClientOptions> options = sp.GetRequiredService<IOptions<BanMachineClientOptions>>();
            o.Address = options.Value.ServiceUri;
        }
    }
}