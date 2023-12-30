namespace Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;

public record CheckingResultCodeBlocksQuery(
    CheckingId CheckingId,
    Guid FistSubmissionId,
    Guid SecondSubmissionId,
    double MinimumSimilarityScore,
    int Cursor);