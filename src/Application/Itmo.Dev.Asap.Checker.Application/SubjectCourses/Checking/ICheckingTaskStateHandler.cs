using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking;

public interface ICheckingTaskStateHandler<in TState> where TState : CheckingTaskState
{
    ValueTask<CheckingTaskStateExecutionResult> HandleAsync(
        TState state,
        CheckingTaskStateHandlerContext context,
        CancellationToken cancellationToken);
}