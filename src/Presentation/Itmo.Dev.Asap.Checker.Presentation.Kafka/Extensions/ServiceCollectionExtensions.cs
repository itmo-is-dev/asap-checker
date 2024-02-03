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

        collection.AddPlatformKafka(builder => builder
            .ConfigureOptions(configuration.GetSection("Presentation:Kafka"))
            .AddConsumer(b => b
                .WithKey<SubmissionDataKey>()
                .WithValue<SubmissionDataValue>()
                .WithConfiguration(
                    configuration.GetSection($"{consumerKey}:SubmissionData"),
                    c => c.WithGroup(group))
                .DeserializeKeyWithProto()
                .DeserializeValueWithProto()
                .HandleWith<SubmissionDataHandler>())
            .AddConsumer(b => b
                .WithKey<BanMachineAnalysisKey>()
                .WithValue<BanMachineAnalysisValue>()
                .WithConfiguration(
                    configuration.GetSection($"{consumerKey}:BanMachineAnalysis"),
                    c => c.WithGroup(group))
                .DeserializeKeyWithProto()
                .DeserializeValueWithProto()
                .HandleWith<BanMachineAnalysisHandler>()));

        return collection;
    }
}