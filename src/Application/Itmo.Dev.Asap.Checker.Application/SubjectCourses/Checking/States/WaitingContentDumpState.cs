using Itmo.Dev.Asap.Checker.Application.Models;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;

public sealed record WaitingContentDumpState(DumpTaskId DumpTaskId) : CheckingTaskState;