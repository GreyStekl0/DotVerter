using DotVerter.Data.Remote.Cbr.DTO;
using DotVerter.Domain.Models;

namespace DotVerter.Data.Remote.Cbr;

using System.Net;
using System.Net.Http.Json;

public sealed class CbrClient(HttpClient httpClient) : IClient
{
    public async Task<RatesForDate> GetRatesAsync(
        DateOnly requestedDate,
        CancellationToken cancellationToken = default)
    {
        const int maxDaysBack = 30;

        for (var offset = 0; offset <= maxDaysBack; offset++)
        {
            var currentDate = requestedDate.AddDays(-offset);

            var url = $"archive/{currentDate:yyyy/MM/dd}/daily_json.js";

            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                continue;
            }

            response.EnsureSuccessStatusCode();

            var dto = await response.Content
                .ReadFromJsonAsync<CbrDailyResponseDto>(cancellationToken: cancellationToken);

            return dto is null ? throw new InvalidOperationException("CBR response is empty.") : dto.ToDomain(requestedDate);
        }

        throw new InvalidOperationException(
            $"Не удалось найти курсы ЦБ для даты {requestedDate} и предыдущих {maxDaysBack} дней.");
    }
}