using Itmo.Dev.Platform.Events;

namespace Itmo.Dev.Asap.Checker.Application.Contracts.Checking.Notifications;

public record SubjectCourseDumpFinishedEvent(long DumpTaskId) : IEvent;