using Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;

namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Checking.States;

public sealed record LoadingResultsState(CheckingId CheckingId) : CheckingTaskState;