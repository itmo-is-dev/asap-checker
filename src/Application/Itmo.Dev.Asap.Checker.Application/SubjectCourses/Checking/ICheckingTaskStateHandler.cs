using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking;

public interface ICheckingTaskStateHandler
{
    Task<CheckingTaskStateExecutionResult> HandleAsync(
        CheckingTaskStateHandlerContext context,
        CancellationToken cancellationToken);
}