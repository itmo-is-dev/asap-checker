using Itmo.Dev.Asap.Checker.Presentation.Grpc.Controllers;
using Microsoft.AspNetCore.Builder;

namespace Itmo.Dev.Asap.Checker.Presentation.Grpc.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePresentationGrpc(this IApplicationBuilder builder)
    {
        builder.UseEndpoints(x =>
        {
            x.MapGrpcService<CheckingController>();

            x.MapGrpcReflectionService();
        });

        return builder;
    }
}