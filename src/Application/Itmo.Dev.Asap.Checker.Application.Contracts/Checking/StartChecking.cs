namespace Itmo.Dev.Asap.Checker.Application.Contracts.Checking;

public static class StartChecking
{
    public sealed record Request(Guid SubjectCourseId);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success : Response;

        public sealed record SubjectCourseNotFound : Response;

        public sealed record AlreadyInProgress : Response;
    }
}