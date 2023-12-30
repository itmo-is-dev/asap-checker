namespace Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;

public record SubmissionPairCheckingResult(
    Guid FirstSubmissionId,
    Guid SecondSubmissionId,
    double SimilarityScore);