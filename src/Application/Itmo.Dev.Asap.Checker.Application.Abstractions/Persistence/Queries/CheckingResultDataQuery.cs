using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public partial record CheckingResultDataQuery(
    long TaskId,
    Guid? FirstSubmissionId,
    Guid? SecondSubmissionId,
    int PageSize);