using System.Diagnostics;
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

    // Московский часовой пояс (UTC+3)
    private static readonly TimeZoneInfo MoscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");

    public CbrApiService() : this(new HttpClient())
    {
    }

    /// <summary>
    ///     Получить текущую дату по московскому времени
    /// </summary>
    public static DateOnly GetMoscowToday()
    {
        var moscowNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, MoscowTimeZone);
        return DateOnly.FromDateTime(moscowNow);
    }

    /// <summary>
    ///     Получить курсы валют на указанную дату.
    ///     Если на дату нет курса, ищет ближайший предыдущий (до MaxDaysBack дней назад).
    /// </summary>
    public async Task<CbrResponseDto?> GetRatesByDateAsync(DateOnly date)
    {
        Debug.WriteLine($"[API] GetRatesByDateAsync called for date: {date:yyyy-MM-dd}");
        Debug.WriteLine($"[API] Moscow today: {GetMoscowToday():yyyy-MM-dd}");

        // Пробуем получить курс, если нет — идём назад по датам
        for (var i = 0; i < MaxDaysBack; i++)
        {
            var targetDate = date.AddDays(-i);
            Debug.WriteLine($"[API] Trying date: {targetDate:yyyy-MM-dd} (offset: {i})");

            var response = await TryGetRatesForDateAsync(targetDate);

            if (response == null) continue;
            
            // Устанавливаем дату из URL запроса, а не из ответа API
            // чтобы избежать проблем с часовыми поясами
            response.RequestedArchiveDate = targetDate;
            
            Debug.WriteLine(
                $"[API] Success! Found {response.Valute.Count} currencies for {targetDate:yyyy-MM-dd}");
            return response;
        }

        Debug.WriteLine($"[API] Failed to get rates for {date:yyyy-MM-dd} after {MaxDaysBack} attempts");
        return null;
    }

    /// <summary>
    ///     Попытка получить курс на конкретную дату (без поиска по предыдущим)
    /// </summary>
    private async Task<CbrResponseDto?> TryGetRatesForDateAsync(DateOnly date)
    {
        try
        {
            // Всегда используем архивный URL для получения курсов на конкретную дату
            // Основной URL (BaseUrl) возвращает курсы на следующий день
            var dt = date.ToDateTime(TimeOnly.MinValue);
            var url = string.Format(ArchiveUrlTemplate, dt.Year, dt.Month.ToString("D2"), dt.Day.ToString("D2"));

            Debug.WriteLine($"[API] URL: {url}");

            var response = await httpClient.GetAsync(url);

            Debug.WriteLine($"[API] HTTP Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[API] Content length: {content.Length} chars");

            var dto = await response.Content.ReadFromJsonAsync<CbrResponseDto>();
            Debug.WriteLine($"[API] Deserialized: {dto?.Valute.Count ?? 0} currencies");

            return dto;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[API] Exception: {ex.Message}");
            return null;
        }
    }
}