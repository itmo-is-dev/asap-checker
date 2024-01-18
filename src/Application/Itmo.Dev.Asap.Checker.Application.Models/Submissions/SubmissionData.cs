namespace Itmo.Dev.Asap.Checker.Application.Models.Submissions;

public record SubmissionData(
    Guid SubmissionId,
    Guid UserId,
    Guid AssignmentId,
    CheckingId CheckingId,
    string FileLink);