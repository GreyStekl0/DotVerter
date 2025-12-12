namespace Domain.Models;

/// <summary>
///     Результат запроса курсов валют с информацией о реальной дате курса
/// </summary>
public class CurrencyRatesResult
{
    /// <summary>
    ///     Запрошенная дата
    /// </summary>
    public DateOnly RequestedDate { get; set; }

    /// <summary>
    ///     Реальная дата курса (может отличаться от запрошенной, если на запрошенную дату курс не установлен)
    /// </summary>
    public DateOnly ActualDate { get; set; }

    /// <summary>
    ///     Список курсов валют
    /// </summary>
    public IEnumerable<Currency> Currencies { get; set; } = [];
}