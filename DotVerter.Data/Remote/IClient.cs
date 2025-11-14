using DotVerter.Domain.Models;

namespace DotVerter.Data.Remote;

public interface IClient
{
    Task<RatesForDate> GetRatesAsync(
        DateOnly requestedDate,
        CancellationToken cancellationToken = default);
}
