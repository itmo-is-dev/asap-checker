using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Core;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Core.Models;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence;
using Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;
using Itmo.Dev.Asap.Checker.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.Services;

internal class PairCheckingResultEnricher
{
    private readonly ICoreStudentService _studentService;
    private readonly IPersistenceContext _context;

    public PairCheckingResultEnricher(ICoreStudentService studentService, IPersistenceContext context)
    {
        _studentService = studentService;
        _context = context;
    }

    public async Task<SubmissionPairCheckingResult> EnrichAsync(
        BanMachinePairCheckingResult result,
        CancellationToken cancellationToken)
    {
        var query = SubmissionDataQuery.Build(builder => builder
            .WithSubmissionId(result.FirstSubmissionId)
            .WithSubmissionId(result.SecondSubmissionId)
            .WithPageSize(2));

        SubmissionData[] data = await _context.SubmissionData
            .QueryAsync(query, cancellationToken)
            .ToArrayAsync(cancellationToken);

        Dictionary<Guid, Student> students = await _studentService
            .GetByIdsAsync(data.Select(x => x.UserId), cancellationToken)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return new SubmissionPairCheckingResult(
            AssignmentId: data[0].AssignmentId,
            FirstSubmissionId: data[0].SubmissionId,
            FirstSubmissionGroupId: students[data[0].UserId].GroupId,
            SecondSubmissionId: data[1].SubmissionId,
            SecondSubmissionGroupId: students[data[1].UserId].GroupId,
            SimilarityScore: result.SimilarityScore);
    }
}