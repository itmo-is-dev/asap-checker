using Itmo.Dev.Asap.Checker.Application.Models.Submissions;
using Itmo.Dev.Platform.Events;

namespace Itmo.Dev.Asap.Checker.Application.Contracts.Submissions;

public record SubmissionDataAddedEvent(IReadOnlyCollection<SubmissionData> SubmissionData) : IEvent;