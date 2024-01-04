namespace Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;

public record SubmissionPairCheckingResult(
    Guid AssignmentId,
    SubmissionInfo FirstSubmission,
    SubmissionInfo SecondSubmission,
    double SimilarityScore);