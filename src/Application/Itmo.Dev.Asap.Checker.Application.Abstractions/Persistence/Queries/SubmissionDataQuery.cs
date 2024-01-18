using Itmo.Dev.Asap.Checker.Application.Models;
using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public partial record SubmissionDataQuery(
    CheckingId[] CheckingIds,
    Guid[] SubmissionIds,
    Guid? SubmissionIdCursor,
    int PageSize);