using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.StateHandlers;

public class WaitingAnalysisStateHandler : ICheckingTaskStateHandler<WaitingAnalysisState>
{
    public ValueTask<CheckingTaskStateExecutionResult> HandleAsync(
        WaitingAnalysisState state,
        CheckingTaskStateHandlerContext context,
        CancellationToken cancellationToken)
    {
        var result = new CheckingTaskStateExecutionResult.Finished(new LoadingResultsState(state.AnalysisId));
        return ValueTask.FromResult<CheckingTaskStateExecutionResult>(result);
    }
}