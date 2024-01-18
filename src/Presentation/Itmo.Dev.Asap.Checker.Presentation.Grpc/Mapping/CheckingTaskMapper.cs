using Google.Protobuf.WellKnownTypes;
using Itmo.Dev.Asap.Checker.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Checker.Models;

namespace Itmo.Dev.Asap.Checker.Presentation.Grpc.Mapping;

public static class CheckingTaskMapper
{
    public static CheckingTask MapToProtoModel(this SubjectCourseCheckingTask task)
    {
        return new CheckingTask
        {
            TaskId = task.Id.Value,
            CreatedAt = Timestamp.FromDateTimeOffset(task.CreatedAt),
            IsCompleted = task.IsCompleted,
        };
    }
}