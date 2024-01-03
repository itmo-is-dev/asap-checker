namespace Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;

public record CheckingResultCodeBlocksRequest(
    CheckingId CheckingId,
    Guid FistSubmissionId,
    Guid SecondSubmissionId,
    double MinimumSimilarityScore,
    int Cursor);