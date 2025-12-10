namespace Data.Models;

/// <summary>
///     Модель валюты из API ЦБ РФ
/// </summary>
public class Currency
{
    /// <summary>
    ///     Идентификатор валюты (например, R01235)
    /// </summary>
    public string ID { get; set; } = string.Empty;

    /// <summary>
    ///     Цифровой код валюты (например, 840)
    /// </summary>
    public string NumCode { get; set; } = string.Empty;

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

    /// <summary>
    ///     Предыдущий курс валюты
    /// </summary>
    public decimal Previous { get; set; }
}