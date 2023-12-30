namespace Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;

public record struct CodeBlock(string FilePath, int LineFrom, int LineTo, string Content);