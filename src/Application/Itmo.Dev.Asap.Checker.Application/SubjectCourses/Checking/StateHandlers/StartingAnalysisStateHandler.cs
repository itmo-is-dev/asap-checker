using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Services;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.Checker.Application.Models;
using Itmo.Dev.Asap.Checker.Application.Models.Submissions;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Models;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;
using Itmo.Dev.Asap.Checker.Application.SubjectCourses.Options;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.StateHandlers;

internal class StartingAnalysisStateHandler : ICheckingTaskStateHandler<StartingAnalysisState>
{
    private readonly CheckingServiceOptions _options;
    private readonly IBanMachineService _banMachineService;
    private readonly ISubmissionDataRepository _submissionDataRepository;

    public StartingAnalysisStateHandler(
        IOptions<CheckingServiceOptions> options,
        IBanMachineService banMachineService,
        ISubmissionDataRepository submissionDataRepository)
    {
        _banMachineService = banMachineService;
        _submissionDataRepository = submissionDataRepository;
        _options = options.Value;
    }

    public async ValueTask<CheckingTaskStateExecutionResult> HandleAsync(
        StartingAnalysisState state,
        CheckingTaskStateHandlerContext context,
        CancellationToken cancellationToken)
    {
        AnalysisId analysisId = await _banMachineService.CreateAnalysisAsync(cancellationToken);

        var query = SubmissionDataQuery.Build(builder => builder
            .WithCheckingId(new CheckingId(context.BackgroundTaskId.Value))
            .WithPageSize(_options.SubmissionDataPageSize));

        SubmissionData[] data;

        do
        {
            data = await _submissionDataRepository
                .QueryAsync(query, cancellationToken)
                .ToArrayAsync(cancellationToken);

            await _banMachineService.AddAnalysisDataAsync(analysisId, data, cancellationToken);

            query = query with
            {
                SubmissionIdCursor = data is [] ? null : data[^1].SubmissionId,
            };
        }
        while (data.Length == _options.SubmissionDataPageSize);

        await _banMachineService.StartAnalysisAsync(analysisId, cancellationToken);

        return new CheckingTaskStateExecutionResult.FinishedWithResult(
            new WaitingAnalysisState(analysisId),
            BackgroundTaskExecutionResult.Suspended.ForEmptyResult().ForError<CheckingTaskError>());
    }
}