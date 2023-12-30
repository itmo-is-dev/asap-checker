namespace Itmo.Dev.Asap.Checker.Infrastructure.BanMachine.Mapping;

public static class CommonMapper
{
    public static Guid MapToGuid(this string value)
        => Guid.Parse(value);
}