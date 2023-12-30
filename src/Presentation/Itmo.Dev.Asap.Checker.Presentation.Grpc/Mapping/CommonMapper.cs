namespace Itmo.Dev.Asap.Checker.Presentation.Grpc.Mapping;

public static class CommonMapper
{
    public static Guid MapToGuid(this string value)
        => Guid.Parse(value);
}