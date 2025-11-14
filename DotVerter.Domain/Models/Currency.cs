namespace DotVerter.Domain.Models;

public sealed class Currency
{
    public required string CharCode { get; init; }
    public required string Name { get; init; }
}