namespace UI.Models;

/// <summary>
///     Модель валюты для отображения в UI
/// </summary>
public class CurrencyItem
{
    public string CharCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Nominal { get; set; }
    public decimal Value { get; set; }

    /// <summary>
    ///     Отображаемое значение в Picker
    /// </summary>
    public string DisplayName => $"{Name} ({CharCode})";

    /// <summary>
    ///     Курс за 1 единицу валюты
    /// </summary>
    public decimal RatePerUnit => Value / Nominal;
}