using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.StateHandlers;

public class WaitingContentDumpStateHandler : ICheckingTaskStateHandler<WaitingContentDumpState>
{
    public ValueTask<CheckingTaskStateExecutionResult> HandleAsync(
        WaitingContentDumpState state,
        CheckingTaskStateHandlerContext context,
        CancellationToken cancellationToken)
    {
        var result = new CheckingTaskStateExecutionResult.Finished(new StartingAnalysisState());
        return ValueTask.FromResult<CheckingTaskStateExecutionResult>(result);
    }
}