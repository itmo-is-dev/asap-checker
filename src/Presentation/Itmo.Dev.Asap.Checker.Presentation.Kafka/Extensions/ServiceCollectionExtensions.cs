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

        string host = configuration.GetSection("Presentation:Kafka:Host").Get<string>() ?? string.Empty;
        string group = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;

        collection.AddKafkaConsumer<SubmissionDataKey, SubmissionDataValue>(selector => selector
            .HandleWith<SubmissionDataHandler>()
            .DeserializeKeyWithProto()
            .DeserializeValueWithProto()
            .UseNamedOptionsConfiguration(
                "SubmissionData",
                configuration.GetSection($"{consumerKey}:SubmissionData"),
                c => c.WithHost(host).WithGroup(group)));

        collection.AddKafkaConsumer<BanMachineAnalysisKey, BanMachineAnalysisValue>(selector => selector
            .HandleWith<BanMachineAnalysisHandler>()
            .DeserializeKeyWithProto()
            .DeserializeValueWithProto()
            .UseNamedOptionsConfiguration(
                "BanMachineAnalysis",
                configuration.GetSection($"{consumerKey}:BanMachineAnalysis"),
                c => c.WithHost(host).WithGroup(group)));

        return collection;
    }
}