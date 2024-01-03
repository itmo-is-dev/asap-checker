namespace Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;

public record SubmissionPairCheckingResult(
    Guid AssignmentId,
    Guid FirstSubmissionId,
    Guid FirstSubmissionGroupId,
    Guid SecondSubmissionId,
    Guid SecondSubmissionGroupId,
    double SimilarityScore);