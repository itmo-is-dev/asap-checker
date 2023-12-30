namespace Itmo.Dev.Asap.Checker.Application.SubjectCourses.Options;

public class CheckingServiceOptions
{
    public int SubmissionDataPageSize { get; set; }

    public double MinimumSubmissionSimilarityScore { get; set; }

    public double MinimumCodeBlocksSimilarityScore { get; set; }
}