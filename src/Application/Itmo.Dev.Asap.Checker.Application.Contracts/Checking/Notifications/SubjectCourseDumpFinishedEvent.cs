using Itmo.Dev.Asap.Checker.Application.Models;
using Itmo.Dev.Platform.Events;

namespace Itmo.Dev.Asap.Checker.Application.Contracts.Checking.Notifications;

public record SubjectCourseDumpFinishedEvent(DumpTaskId DumpTaskId) : IEvent;