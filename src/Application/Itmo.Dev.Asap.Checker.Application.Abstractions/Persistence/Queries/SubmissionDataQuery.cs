using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public partial record SubmissionDataQuery(
    long[] TaskIds,
    Guid[] SubmissionIds,
    Guid? SubmissionIdCursor,
    int PageSize);