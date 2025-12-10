using System.Net.Http.Json;
using Data.Dto;

namespace Data.Services;

/// <summary>
///     Сервис для работы с API ЦБ РФ (cbr-xml-daily.ru)
/// </summary>
internal class CbrApiService(HttpClient httpClient)
{
    private const string BaseUrl = "https://www.cbr-xml-daily.ru/daily_json.js";
    private const string ArchiveUrlTemplate = "https://www.cbr-xml-daily.ru/archive/{0}/{1}/{2}/daily_json.js";
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
        System.Diagnostics.Debug.WriteLine($"[API] GetRatesByDateAsync called for date: {date:yyyy-MM-dd}");
        
        // Пробуем получить курс, если нет — идём назад по датам
        for (var i = 0; i < MaxDaysBack; i++)
        {
            var targetDate = date.AddDays(-i);
            System.Diagnostics.Debug.WriteLine($"[API] Trying date: {targetDate:yyyy-MM-dd} (offset: {i})");
            
            var response = await TryGetRatesForDateAsync(targetDate);

            if (response != null)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Success! Found {response.Valute?.Count ?? 0} currencies for {targetDate:yyyy-MM-dd}");
                return response;
            }
        }

        System.Diagnostics.Debug.WriteLine($"[API] Failed to get rates for {date:yyyy-MM-dd} after {MaxDaysBack} attempts");
        return null;
    }

    /// <summary>
    ///     Попытка получить курс на конкретную дату (без поиска по предыдущим)
    /// </summary>
    private async Task<CbrResponseDto?> TryGetRatesForDateAsync(DateOnly date)
    {
        try
        {
            string url;
            
            // Для сегодняшней даты используем основной URL (может быть быстрее)
            if (date == DateOnly.FromDateTime(DateTime.Today))
            {
                url = BaseUrl;
            }
            else
            {
                var dt = date.ToDateTime(TimeOnly.MinValue);
                url = string.Format(ArchiveUrlTemplate, dt.Year, dt.Month.ToString("D2"), dt.Day.ToString("D2"));
            }

            System.Diagnostics.Debug.WriteLine($"[API] URL: {url}");
            
            var response = await httpClient.GetAsync(url);

            System.Diagnostics.Debug.WriteLine($"[API] HTTP Status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[API] Content length: {content.Length} chars");
            
            var dto = await response.Content.ReadFromJsonAsync<CbrResponseDto>();
            System.Diagnostics.Debug.WriteLine($"[API] Deserialized: {dto?.Valute?.Count ?? 0} currencies");
            
            return dto;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API] Exception: {ex.Message}");
            return null;
        }
    }
}