namespace Domain.Models;

/// <summary>
///     Модель валюты для отображения в UI
/// </summary>
public class Currency
{
    /// <summary>
    ///     Буквенный код валюты (например, USD)
    /// </summary>
    public string CharCode { get; set; } = string.Empty;

    /// <summary>
    ///     Номинал валюты
    /// </summary>
    public int Nominal { get; set; }

    /// <summary>
    ///     Название валюты
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Курс валюты
    /// </summary>
    public decimal Value { get; set; }
}