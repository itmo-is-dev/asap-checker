using FluentSerialization;
using FluentSerialization.Extensions.NewtonsoftJson;
using Itmo.Dev.Asap.Checker.Application.Contracts.Checking;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses;
using Itmo.Dev.Asap.Checker.Application.Tools;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Itmo.Dev.Asap.Checker.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        collection.AddScoped<ICheckingService, CheckingService>();

        collection.Configure<JsonSerializerSettings>(o => ConfigurationBuilder
            .Build(new SerializationConfiguration())
            .ApplyToSerializationSettings(o));

        return collection;
    }
}