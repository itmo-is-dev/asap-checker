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
        long taskId,
        BanMachinePairCheckingResult result,
        CancellationToken cancellationToken)
    {
        var query = SubmissionDataQuery.Build(builder => builder
            .WithTaskId(taskId)
            .WithSubmissionId(result.FirstSubmissionId)
            .WithSubmissionId(result.SecondSubmissionId)
            .WithPageSize(2));

        Dictionary<Guid, SubmissionData> data = await _context.SubmissionData
            .QueryAsync(query, cancellationToken)
            .ToDictionaryAsync(x => x.SubmissionId, cancellationToken);

        Dictionary<Guid, Student> students = await _studentService
            .GetByIdsAsync(data.Select(x => x.Value.UserId), cancellationToken)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return new SubmissionPairCheckingResult(
            AssignmentId: data[result.FirstSubmissionId].AssignmentId,
            FirstSubmission: Map(data[result.FirstSubmissionId], students[data[result.FirstSubmissionId].UserId]),
            SecondSubmission: Map(data[result.SecondSubmissionId], students[data[result.SecondSubmissionId].UserId]),
            SimilarityScore: result.SimilarityScore);
    }

    private static SubmissionInfo Map(SubmissionData submission, Student student)
    {
        return new SubmissionInfo(submission.SubmissionId, student.Id, student.GroupId);
    }
}