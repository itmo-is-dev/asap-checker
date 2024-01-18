using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.StateHandlers;

internal sealed class StartingStateHandler : ICheckingTaskStateHandler<StartingState>
{
    public ValueTask<CheckingTaskStateExecutionResult> HandleAsync(
        StartingState state,
        CheckingTaskStateHandlerContext context,
        CancellationToken cancellationToken)
    {
        var result = new CheckingTaskStateExecutionResult.Finished(new DumpingContentState());
        return ValueTask.FromResult<CheckingTaskStateExecutionResult>(result);
    }
}