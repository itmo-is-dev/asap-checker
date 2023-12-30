using Itmo.Dev.Platform.BackgroundTasks.Models;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;

public record CheckingTaskStateHandlerContext(BackgroundTaskId BackgroundTaskId, CheckingTaskMetadata Metadata);