using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking;

public record CheckingTaskMetadata(Guid SubjectCourseId) : IBackgroundTaskMetadata;