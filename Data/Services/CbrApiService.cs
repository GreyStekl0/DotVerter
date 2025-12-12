using System.Diagnostics;
using System.Text.Json;
using Data.Dto;

namespace Data.Services;

/// <summary>
///     Сервис для работы с API ЦБ РФ (cbr-xml-daily.ru)
/// </summary>
public class CbrApiService(HttpClient httpClient) : ICbrApiService
{
    private const string ArchiveUrlTemplate = "https://www.cbr-xml-daily.ru/archive/{0}/{1}/{2}/daily_json.js";
    private const int MaxDaysBack = 10;

    /// <inheritdoc />
    public async Task<CbrResponseDto?> GetRatesByDateAsync(DateOnly date)
    {
        Debug.WriteLine($"[API] GetRatesByDateAsync called for date: {date:yyyy-MM-dd}");

        // Пробуем несколько дат назад, если на запрошенную нет данных (выходные/праздники)
        for (var i = 0; i < MaxDaysBack; i++)
        {
            var targetDate = date.AddDays(-i);
            Debug.WriteLine($"[API] Trying date: {targetDate:yyyy-MM-dd} (offset: {i})");

            var response = await TryGetRatesForDateAsync(targetDate).ConfigureAwait(false);

            if (response == null) continue;

            response.RequestedArchiveDate = targetDate;

            Debug.WriteLine(
                $"[API] Success! Found {response.Valute.Count} currencies for {targetDate:yyyy-MM-dd}");
            return response;
        }

        Debug.WriteLine($"[API] Failed to get rates for {date:yyyy-MM-dd} after {MaxDaysBack} attempts");
        return null;
    }

    private async Task<CbrResponseDto?> TryGetRatesForDateAsync(DateOnly date)
    {
        try
        {
            var url = string.Format(ArchiveUrlTemplate, date.Year, date.Month.ToString("D2"), date.Day.ToString("D2"));

            Debug.WriteLine($"[API] URL: {url}");

            using var response = await httpClient
                .GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            Debug.WriteLine($"[API] HTTP Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var dto = await JsonSerializer
                .DeserializeAsync(stream, CbrJsonContext.Default.CbrResponseDto)
                .ConfigureAwait(false);
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
