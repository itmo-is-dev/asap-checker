namespace Itmo.Dev.Asap.Checker.Application.Models.SubjectCourses;

public record SubjectCourseCheckingTask(CheckingId Id, DateTimeOffset CreatedAt, bool IsCompleted);