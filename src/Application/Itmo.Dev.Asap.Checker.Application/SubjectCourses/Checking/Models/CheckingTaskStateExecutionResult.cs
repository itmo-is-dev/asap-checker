using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;

public abstract record CheckingTaskStateExecutionResult
{
    public sealed record Finished(CheckingTaskState State) : CheckingTaskStateExecutionResult;

    public sealed record FinishedWithResult(
        CheckingTaskState State,
        BackgroundTaskExecutionResult<EmptyExecutionResult, CheckingTaskError> Result)
        : CheckingTaskStateExecutionResult;
}