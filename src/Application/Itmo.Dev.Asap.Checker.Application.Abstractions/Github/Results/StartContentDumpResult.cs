namespace Itmo.Dev.Asap.Checker.Application.Abstractions.Github.Results;

public abstract record StartContentDumpResult
{
    private StartContentDumpResult() { }

    public sealed record Success(long TaskId) : StartContentDumpResult;

    public sealed record AlreadyInProgress : StartContentDumpResult;

    public sealed record SubjectCourseNotFound : StartContentDumpResult;
}