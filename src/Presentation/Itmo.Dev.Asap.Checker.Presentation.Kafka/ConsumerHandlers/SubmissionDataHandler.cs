using Itmo.Dev.Asap.Checker.Application.Contracts.Checking.Notifications;
using Itmo.Dev.Asap.Checker.Application.Contracts.Submissions;
using Itmo.Dev.Asap.Checker.Application.Models.Submissions;
using Itmo.Dev.Asap.Checker.Presentation.Kafka.Mapping;
using Itmo.Dev.Asap.Kafka;
using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Checker.Presentation.Kafka.ConsumerHandlers;

internal class SubmissionDataHandler : IKafkaMessageHandler<SubmissionDataKey, SubmissionDataValue>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<SubmissionDataHandler> _logger;

    public SubmissionDataHandler(IEventPublisher eventPublisher, ILogger<SubmissionDataHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async ValueTask HandleAsync(
        IEnumerable<ConsumerKafkaMessage<SubmissionDataKey, SubmissionDataValue>> messages,
        CancellationToken cancellationToken)
    {
        var finishedEvents = new List<SubjectCourseDumpFinishedEvent>();

        SubmissionData[] data = FilterAddedData(messages, finishedEvents).ToArray();
        var evt = new SubmissionDataAddedEvent(data);

        await _eventPublisher.PublishAsync(evt, cancellationToken);
        await _eventPublisher.PublishAsync(finishedEvents, cancellationToken);
    }

    private IEnumerable<SubmissionData> FilterAddedData(
        IEnumerable<ConsumerKafkaMessage<SubmissionDataKey, SubmissionDataValue>> messages,
        ICollection<SubjectCourseDumpFinishedEvent> finishedEvents)
    {
#pragma warning disable IDE0008
        foreach (var grouping in messages.GroupBy(x => x.Value.EventCase))
#pragma warning restore IDE0008
        {
            if (grouping.Key is SubmissionDataValue.EventOneofCase.SubmissionDataCollectionFinished)
            {
                foreach (ConsumerKafkaMessage<SubmissionDataKey, SubmissionDataValue> message in grouping)
                {
                    var evt = new SubjectCourseDumpFinishedEvent(message.Key.TaskId);
                    finishedEvents.Add(evt);
                }
            }
            else if (grouping.Key is SubmissionDataValue.EventOneofCase.SubmissionDataAdded)
            {
                foreach (ConsumerKafkaMessage<SubmissionDataKey, SubmissionDataValue> message in grouping)
                {
                    yield return new SubmissionData(
                        message.Value.SubmissionDataAdded.SubmissionId.MapToGuid(),
                        message.Value.SubmissionDataAdded.UserId.MapToGuid(),
                        message.Value.SubmissionDataAdded.AssignmentId.MapToGuid(),
                        message.Key.TaskId,
                        message.Value.SubmissionDataAdded.FileLink);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Unknown submission data event = {EventCase}",
                    grouping.Key);
            }
        }
    }
}