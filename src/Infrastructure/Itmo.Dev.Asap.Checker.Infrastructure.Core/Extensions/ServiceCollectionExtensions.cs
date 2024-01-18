using Grpc.Net.ClientFactory;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Core;
using Itmo.Dev.Asap.Checker.Infrastructure.Core.Services;
using Itmo.Dev.Asap.Core.Students;
using Itmo.Dev.Asap.Core.SubjectCourses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureCore(this IServiceCollection collection)
    {
        collection
            .AddOptions<CoreOptions>()
            .BindConfiguration("Infrastructure:Core");

        collection.AddGrpcClient<StudentService.StudentServiceClient>(ConfigureAddress);
        collection.AddScoped<ICoreStudentService, CoreStudentService>();

        collection.AddGrpcClient<SubjectCourseService.SubjectCourseServiceClient>(ConfigureAddress);
        collection.AddScoped<ICoreSubjectCourseService, CoreSubjectCourseService>();

        return collection;

        static void ConfigureAddress(IServiceProvider sp, GrpcClientFactoryOptions o)
        {
            IOptions<CoreOptions> options = sp.GetRequiredService<IOptions<CoreOptions>>();
            o.Address = options.Value.ServiceUri;
        }
    }
}