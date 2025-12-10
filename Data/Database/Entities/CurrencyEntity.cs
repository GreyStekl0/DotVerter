using SQLite;

namespace Data.Database.Entities;

/// <summary>
///     Сущность валюты для хранения в SQLite
/// </summary>
[Table("Currencies")]
internal class CurrencyEntity
{
    /// <summary>
    ///     Первичный ключ (автоинкремент)
    /// </summary>
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    ///     Буквенный код валюты (например, USD)
    /// </summary>
    [Indexed]
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
    ///     Дата курса
    /// </summary>
    [Indexed]
    public DateTime RateDate { get; set; }
}