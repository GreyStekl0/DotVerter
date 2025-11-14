using DotVerter.Domain.Models;

namespace DotVerter.Domain.Interface;

public interface IExchangeRateRepository
{
    Task<RatesForDate> GetRatesAsync(
        DateOnly requestedDate,
        CancellationToken cancellationToken = default);
}