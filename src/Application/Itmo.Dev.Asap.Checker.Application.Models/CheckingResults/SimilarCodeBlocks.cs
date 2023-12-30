namespace Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;

public record struct SimilarCodeBlocks(CodeBlock First, CodeBlock Second, double SimilarityScore);