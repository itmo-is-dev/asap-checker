using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;
using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Services;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.Checker.Application.Models.Submissions;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Options;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.StateHandlers;

internal class StartingCheckingStateHandler : ICheckingTaskStateHandler
{
    private readonly StartingCheckingState _state;
    private readonly CheckingServiceOptions _options;
    private readonly IBanMachineService _banMachineService;
    private readonly ISubmissionDataRepository _submissionDataRepository;

    public StartingCheckingStateHandler(
        StartingCheckingState state,
        IOptions<CheckingServiceOptions> options,
        IBanMachineService banMachineService,
        ISubmissionDataRepository submissionDataRepository)
    {
        _state = state;
        _banMachineService = banMachineService;
        _submissionDataRepository = submissionDataRepository;
        _options = options.Value;
    }

    public async Task<CheckingTaskStateExecutionResult> HandleAsync(
        CheckingTaskStateHandlerContext context,
        CancellationToken cancellationToken)
    {
        CheckingId checkingId = await _banMachineService.CreateCheckingAsync(cancellationToken);

        var query = SubmissionDataQuery.Build(x
            => x.WithTaskId(_state.DumpTaskId).WithPageSize(_options.SubmissionDataPageSize));

        SubmissionData[] data;

        do
        {
            data = await _submissionDataRepository
                .QueryAsync(query, cancellationToken)
                .ToArrayAsync(cancellationToken);

            await _banMachineService.AddCheckingDataAsync(checkingId, data, cancellationToken);

            query = query with
            {
                SubmissionIdCursor = data is [] ? null : data[^1].SubmissionId,
            };
        }
        while (data.Length == _options.SubmissionDataPageSize);

        return new CheckingTaskStateExecutionResult(
            new LoadingResultsState(checkingId),
            new BackgroundTaskExecutionResult<EmptyExecutionResult, CheckingTaskError>.Suspended());
    }
}