using Itmo.Dev.Platform.Events;

namespace Itmo.Dev.Asap.Checker.Application.Contracts.Checking.Notifications;

public record CheckingCompletedEvent(string CheckingId) : IEvent;