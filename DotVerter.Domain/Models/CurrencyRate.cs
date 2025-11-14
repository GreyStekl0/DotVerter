namespace DotVerter.Domain.Models;

public sealed class CurrencyRate
{
    public required Currency Currency { get; init; }
    
    public required int Nominal { get; init; }
    
    public required decimal RublesPerNominal { get; init; }

    public decimal RublesPerOne => RublesPerNominal / Nominal;
}