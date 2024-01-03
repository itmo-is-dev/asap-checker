using Itmo.Dev.Asap.Checker.Presentation.Kafka.ConsumerHandlers;
using Itmo.Dev.Asap.Kafka;
using Itmo.Dev.Platform.Kafka.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Itmo.Dev.Asap.Checker.Presentation.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationKafka(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        const string consumerKey = "Presentation:Kafka:Consumers";

        string group = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;

        collection.AddKafka(builder => builder
            .ConfigureOptions(b => b.BindConfiguration("Presentation:Kafka"))
            .AddConsumer<SubmissionDataKey, SubmissionDataValue>(selector => selector
                .HandleWith<SubmissionDataHandler>()
                .DeserializeKeyWithProto()
                .DeserializeValueWithProto()
                .UseNamedOptionsConfiguration(
                    "SubmissionData",
                    configuration.GetSection($"{consumerKey}:SubmissionData"),
                    c => c.WithGroup(group)))
            .AddConsumer<BanMachineAnalysisKey, BanMachineAnalysisValue>(selector => selector
                .HandleWith<BanMachineAnalysisHandler>()
                .DeserializeKeyWithProto()
                .DeserializeValueWithProto()
                .UseNamedOptionsConfiguration(
                    "BanMachineAnalysis",
                    configuration.GetSection($"{consumerKey}:BanMachineAnalysis"),
                    c => c.WithGroup(group))));

        return collection;
    }
}