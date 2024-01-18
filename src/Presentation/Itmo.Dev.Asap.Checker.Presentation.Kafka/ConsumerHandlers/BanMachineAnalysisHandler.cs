using Itmo.Dev.Asap.Checker.Application.Contracts.Checking.Notifications;
using Itmo.Dev.Asap.Checker.Application.Models;
using Itmo.Dev.Asap.Kafka;
using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Consumer.Models;

namespace Itmo.Dev.Asap.Checker.Presentation.Kafka.ConsumerHandlers;

public class BanMachineAnalysisHandler : IKafkaMessageHandler<BanMachineAnalysisKey, BanMachineAnalysisValue>
{
    private readonly IEventPublisher _eventPublisher;

    public BanMachineAnalysisHandler(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async ValueTask HandleAsync(
        IEnumerable<ConsumerKafkaMessage<BanMachineAnalysisKey, BanMachineAnalysisValue>> messages,
        CancellationToken cancellationToken)
    {
        foreach (ConsumerKafkaMessage<BanMachineAnalysisKey, BanMachineAnalysisValue> message in messages)
        {
            if (message.Value.EventCase is not BanMachineAnalysisValue.EventOneofCase.AnalysisCompleted)
                continue;

            var evt = new AnalysisCompletedEvent(new AnalysisId(message.Key.AnalysisKey));
            await _eventPublisher.PublishAsync(evt, cancellationToken);
        }
    }
}