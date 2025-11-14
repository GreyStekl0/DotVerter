using DotVerter.Data.Remote;
using DotVerter.Domain.Interface;
using DotVerter.Domain.Models;

namespace DotVerter.Data.Repositories;

public class ExchangeRateRepository(IClient client) : IExchangeRateRepository
{
    public async Task<RatesForDate> GetRatesAsync(
        DateOnly requestedDate,
        CancellationToken cancellationToken = default)
    {
        return await client.GetRatesAsync(requestedDate, cancellationToken);
    }
}