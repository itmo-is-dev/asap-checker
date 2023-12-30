using Itmo.Dev.Platform.Events;

namespace Itmo.Dev.Asap.Checker.Application.Extensions;

public static class EventsConfigurationBuilderExtensions
{
    public static IEventsConfigurationBuilder AddApplicationEvents(this IEventsConfigurationBuilder builder)
    {
        return builder.AddHandlersFromAssemblyContaining<IAssemblyMarker>();
    }
}