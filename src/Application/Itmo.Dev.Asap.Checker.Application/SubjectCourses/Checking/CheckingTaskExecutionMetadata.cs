using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking;

public class CheckingTaskExecutionMetadata : IBackgroundTaskExecutionMetadata
{
    public CheckingTaskState? PreviousState { get; set; }

    public CheckingTaskState? State { get; set; }
}