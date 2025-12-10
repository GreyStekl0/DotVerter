namespace Data.Models;

/// <summary>
///     Ответ от API ЦБ РФ с курсами валют
/// </summary>
public class CbrResponse
{
    /// <summary>
    ///     Дата актуальности курсов
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    ///     Дата предыдущих курсов
    /// </summary>
    public DateTime PreviousDate { get; set; }

    /// <summary>
    ///     URL для предыдущих курсов
    /// </summary>
    public string PreviousURL { get; set; } = string.Empty;

    /// <summary>
    ///     Текущая дата и время обновления
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    ///     Словарь валют (ключ - ID валюты)
    /// </summary>
    public Dictionary<string, Currency> Valute { get; set; } = new();
}