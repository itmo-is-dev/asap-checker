namespace Itmo.Dev.Asap.Checker.Application.Contracts.Checking;

public interface ICheckingService
{
    Task<StartChecking.Response> StartCheckingAsync(StartChecking.Request request, CancellationToken cancellationToken);

    Task<GetCheckingTasks.Response> GetCheckingTasksAsync(
        GetCheckingTasks.Request request,
        CancellationToken cancellationToken);

    Task<GetCheckingTaskResults.Response> GetCheckingResultsAsync(
        GetCheckingTaskResults.Request request,
        CancellationToken cancellationToken);

    Task<GetCheckingTaskResultCodeBlocks.Response> GetCheckingResultCodeBlocksAsync(
        GetCheckingTaskResultCodeBlocks.Request request,
        CancellationToken cancellationToken);
}