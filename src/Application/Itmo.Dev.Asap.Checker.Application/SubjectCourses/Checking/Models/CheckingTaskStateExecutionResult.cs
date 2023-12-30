using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;

public record CheckingTaskStateExecutionResult(
    CheckingTaskState State,
    BackgroundTaskExecutionResult<EmptyExecutionResult, CheckingTaskError> Result);