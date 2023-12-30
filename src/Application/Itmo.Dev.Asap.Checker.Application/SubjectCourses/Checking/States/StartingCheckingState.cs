namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;

public sealed record StartingCheckingState(long DumpTaskId) : CheckingTaskState;