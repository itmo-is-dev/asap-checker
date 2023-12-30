namespace Itmo.Dev.Asap.Checker.Application.Abstractions.BanMachine.Models;

public readonly record struct CheckingId(string Value)
{
    public override string ToString()
        => Value;
}