namespace Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;

public record BanMachinePairCheckingResult(
    Guid FirstSubmissionId,
    Guid SecondSubmissionId,
    double SimilarityScore);