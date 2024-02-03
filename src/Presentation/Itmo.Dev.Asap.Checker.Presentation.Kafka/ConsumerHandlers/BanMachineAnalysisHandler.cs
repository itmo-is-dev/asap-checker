using Itmo.Dev.Asap.Checker.Application.Contracts.Checking.Notifications;
using Itmo.Dev.Asap.Checker.Application.Models;
using Itmo.Dev.Asap.Kafka;
using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Kafka.Consumer;

namespace Itmo.Dev.Asap.Checker.Presentation.Kafka.ConsumerHandlers;

public class BanMachineAnalysisHandler : IKafkaConsumerHandler<BanMachineAnalysisKey, BanMachineAnalysisValue>
{
    private readonly IEventPublisher _eventPublisher;

    public BanMachineAnalysisHandler(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async ValueTask HandleAsync(
        IEnumerable<IKafkaConsumerMessage<BanMachineAnalysisKey, BanMachineAnalysisValue>> messages,
        CancellationToken cancellationToken)
    {
        foreach (IKafkaConsumerMessage<BanMachineAnalysisKey, BanMachineAnalysisValue> message in messages)
        {
            if (message.Value.EventCase is not BanMachineAnalysisValue.EventOneofCase.AnalysisCompleted)
                continue;

            var evt = new AnalysisCompletedEvent(new AnalysisId(message.Key.AnalysisKey));
            await _eventPublisher.PublishAsync(evt, cancellationToken);
        }
    }
}