namespace Itmo.Dev.Asap.Checker.Application.Models;

public readonly record struct AnalysisId(string Value)
{
    public override string ToString()
        => Value;
}