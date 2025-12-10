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
    private const int MaxDaysBack = 10; // Максимум дней назад для поиска курса

    public CbrApiService() : this(new HttpClient())
    {
    }

    /// <summary>
    ///     Получить курсы валют на указанную дату.
    ///     Если на дату нет курса, ищет ближайший предыдущий (до MaxDaysBack дней назад).
    /// </summary>
    public async Task<CbrResponseDto?> GetRatesByDateAsync(DateOnly date)
    {
        // Пробуем получить курс, если нет — идём назад по датам
        for (var i = 0; i < MaxDaysBack; i++)
        {
            var targetDate = date.AddDays(-i);
            var response = await TryGetRatesForDateAsync(targetDate);

            if (response != null)
                return response;
        }

        return null;
    }

    /// <summary>
    ///     Попытка получить курс на конкретную дату (без поиска по предыдущим)
    /// </summary>
    private async Task<CbrResponseDto?> TryGetRatesForDateAsync(DateOnly date)
    {
        try
        {
            // Для сегодняшней даты используем основной URL (может быть быстрее)
            var url = date == DateOnly.FromDateTime(DateTime.Today)
                ? BaseUrl
                : string.Format(ArchiveUrlTemplate, date.ToDateTime(TimeOnly.MinValue));

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CbrResponseDto>();
        }
        catch (Exception)
        {
            return null;
        }
    }
}