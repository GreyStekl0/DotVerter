using DotVerter.Data.Local;
using DotVerter.Data.Remote;
using DotVerter.Domain.Interface;
using DotVerter.Domain.Models;
using Microsoft.Extensions.Logging;

namespace DotVerter.Data.Repositories;

public sealed class ExchangeRateRepository(
    IClient client,
    IExchangeRateLocalStore localStore,
    ILogger<ExchangeRateRepository> logger) : IExchangeRateRepository
{
    public async Task<RatesForDate> GetRatesAsync(
        DateOnly requestedDate,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Loading rates for {RequestedDate} from local database", requestedDate);

        var cached = await localStore.TryGetRatesAsync(requestedDate, cancellationToken);

        if (cached is not null)
        {
            logger.LogInformation(
                "Found rates for {RequestedDate} in local database (actual date {ActualDate})",
                requestedDate,
                cached.ActualDate);

            return cached;
        }

        logger.LogInformation("No cached rates for {RequestedDate}. Fetching from remote API", requestedDate);

        var fresh = await client.GetRatesAsync(requestedDate, cancellationToken);

        logger.LogInformation(
            "Fetched rates for {RequestedDate} (actual {ActualDate}) from remote API. Saving into local database",
            requestedDate,
            fresh.ActualDate);

        await localStore.SaveRatesAsync(fresh, cancellationToken);

        return fresh;
    }
}
