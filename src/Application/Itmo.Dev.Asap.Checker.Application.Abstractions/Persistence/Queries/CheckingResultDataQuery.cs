using Itmo.Dev.Asap.Checker.Application.Models;
using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public partial record CheckingResultDataQuery(
    CheckingId CheckingId,
    Guid[] AssignmentIds,
    Guid[] GroupIds,
    Guid? FirstSubmissionId,
    Guid? SecondSubmissionId,
    int PageSize);