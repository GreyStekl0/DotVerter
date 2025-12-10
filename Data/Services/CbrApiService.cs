using System.Net.Http.Json;
using Data.Dto;

namespace Data.Services;

/// <summary>
///     Сервис для работы с API ЦБ РФ (cbr-xml-daily.ru)
/// </summary>
internal class CbrApiService(HttpClient httpClient)
{
    private const string BaseUrl = "https://www.cbr-xml-daily.ru/daily_json.js";
    private const string ArchiveUrlTemplate = "https://www.cbr-xml-daily.ru/archive/{0:yyyy/MM/dd}/daily_json.js";

    public CbrApiService() : this(new HttpClient())
    {
    }

    /// <summary>
    ///     Получить актуальные курсы валют
    /// </summary>
    public async Task<CbrResponseDto?> GetLatestRatesAsync()
    {
        try
        {
            return await httpClient.GetFromJsonAsync<CbrResponseDto>(BaseUrl);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     Получить курсы валют на указанную дату
    /// </summary>
    public async Task<CbrResponseDto?> GetRatesByDateAsync(DateOnly date)
    {
        if (date == DateOnly.FromDateTime(DateTime.Today)) return await GetLatestRatesAsync();

        try
        {
            var url = string.Format(ArchiveUrlTemplate, date.ToDateTime(TimeOnly.MinValue));
            return await httpClient.GetFromJsonAsync<CbrResponseDto>(url);
        }
        catch (Exception)
        {
            return null;
        }
    }
}