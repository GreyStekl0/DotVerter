using Domain.Models;

namespace UI.Models;

/// <summary>
///     Модель валюты для отображения в UI
/// </summary>
public sealed record CurrencyItem
{
    public string CharCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Nominal { get; init; }
    public decimal Value { get; init; }

    /// <summary>
    ///     Отображаемое значение в Picker
    /// </summary>
    public string DisplayName => $"{Name} ({CharCode})";

    /// <summary>
    ///     Курс за 1 единицу валюты
    /// </summary>
    public decimal RatePerUnit => Nominal == 0 ? 0 : Value / Nominal;

    public static CurrencyItem FromDomain(Currency currency)
    {
        return new CurrencyItem
        {
            CharCode = currency.CharCode,
            Name = currency.Name,
            Nominal = currency.Nominal,
            Value = currency.Value
        };
    }
}
