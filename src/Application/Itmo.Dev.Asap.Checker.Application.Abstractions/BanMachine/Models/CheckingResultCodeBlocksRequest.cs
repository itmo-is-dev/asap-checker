using Itmo.Dev.Asap.Checker.Application.Models;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;

public record CheckingResultCodeBlocksRequest(
    AnalysisId AnalysisId,
    Guid FirstSubmissionId,
    Guid SecondSubmissionId,
    double MinimumSimilarityScore,
    int Cursor);