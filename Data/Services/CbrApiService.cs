using System.Net.Http.Json;
using Data.Models;

namespace Data.Services;

/// <summary>
///     Сервис для работы с API ЦБ РФ (cbr-xml-daily.ru)
/// </summary>
public class CbrApiService(HttpClient httpClient) : ICbrApiService
{
    private const string BaseUrl = "https://www.cbr-xml-daily.ru/daily_json.js";

    public CbrApiService() : this(new HttpClient())
    {
    }

    /// <inheritdoc />
    public async Task<CbrResponse?> GetLatestRatesAsync()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<CbrResponse>(BaseUrl);
            return response;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<Currency?> GetCurrencyByCodeAsync(string charCode)
    {
        var response = await GetLatestRatesAsync();

        return response?.Valute.Values
            .FirstOrDefault(c => c.CharCode.Equals(charCode, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Currency>> GetAllCurrenciesAsync()
    {
        var response = await GetLatestRatesAsync();

        if (response?.Valute == null)
            return [];

        return response.Valute.Values;
    }
}