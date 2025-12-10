using Data.Models;

namespace Data.Services;

/// <summary>
///     Интерфейс для работы с API ЦБ РФ
/// </summary>
public interface ICbrApiService
{
    /// <summary>
    ///     Получить актуальные курсы валют
    /// </summary>
    Task<CbrResponse?> GetLatestRatesAsync();

    /// <summary>
    ///     Получить курс конкретной валюты по коду (например, USD, EUR)
    /// </summary>
    Task<Currency?> GetCurrencyByCodeAsync(string charCode);

    /// <summary>
    ///     Получить список всех валют
    /// </summary>
    Task<IEnumerable<Currency>> GetAllCurrenciesAsync();
}