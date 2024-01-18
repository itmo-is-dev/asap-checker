using Itmo.Dev.Asap.Checker.Application.Models;
using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public partial record CheckingResultCodeBlocksQuery(
    CheckingId CheckingId,
    Guid FirstSubmissionId,
    Guid SecondSubmissionId,
    int Cursor,
    int PageSize);