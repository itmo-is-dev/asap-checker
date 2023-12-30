using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Checker.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public partial record SubjectCourseDumpTaskQuery(Guid[] SubjectCourseIds, long[] TaskIds);