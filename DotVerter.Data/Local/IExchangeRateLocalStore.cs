using DotVerter.Domain.Models;

namespace DotVerter.Data.Local;

public interface IExchangeRateLocalStore
{
    Task<RatesForDate?> TryGetRatesAsync(
        DateOnly requestedDate,
        CancellationToken cancellationToken = default);

    Task SaveRatesAsync(
        RatesForDate rates,
        CancellationToken cancellationToken = default);
}
