using Grpc.Net.ClientFactory;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Github;
using Itmo.Dev.Asap.Checker.Infrastructure.Github.Services;
using Itmo.Dev.Asap.Github.SubjectCourses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Github.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureGithub(this IServiceCollection collection)
    {
        collection
            .AddOptions<GithubOptions>()
            .BindConfiguration("Infrastructure:Github");

        collection.AddGrpcClient<GithubSubjectCourseService.GithubSubjectCourseServiceClient>(ConfigureAddress);
        collection.AddScoped<IGithubSubjectCourseService, SubjectCourseService>();

        return collection;

        static void ConfigureAddress(IServiceProvider sp, GrpcClientFactoryOptions o)
        {
            IOptions<GithubOptions> options = sp.GetRequiredService<IOptions<GithubOptions>>();
            o.Address = options.Value.ServiceUri;
        }
    }
}