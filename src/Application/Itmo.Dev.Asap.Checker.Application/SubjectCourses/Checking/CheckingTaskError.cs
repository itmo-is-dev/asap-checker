using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking;

public record CheckingTaskError(string Message) : IBackgroundTaskError;