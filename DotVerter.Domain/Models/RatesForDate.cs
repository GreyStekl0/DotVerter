namespace DotVerter.Domain.Models;

public sealed class RatesForDate
{
    public required DateOnly RequestedDate { get; init; }
    
    public required DateOnly ActualDate { get; init; }

    public required IReadOnlyCollection<CurrencyRate> Rates { get; init; }
}