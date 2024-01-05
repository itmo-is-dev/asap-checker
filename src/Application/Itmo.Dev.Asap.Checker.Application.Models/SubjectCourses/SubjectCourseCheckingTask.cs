namespace Itmo.Dev.Asap.Checker.Application.Models.SubjectCourses;

public record SubjectCourseCheckingTask(long Id, DateTimeOffset CreatedAt, bool IsCompleted);