using System.Globalization;
using DotVerter.Data.Local.Entities;
using DotVerter.Domain.Models;
using SQLite;

namespace DotVerter.Data.Local;

public sealed class ExchangeRateLocalStore(SQLiteAsyncConnection connection) : IExchangeRateLocalStore
{
    private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);
    private bool _isInitialized;

    public async Task<RatesForDate?> TryGetRatesAsync(
        DateOnly requestedDate,
        CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var requestedKey = ToKey(requestedDate);

        var header = await connection.Table<RatesForDateEntity>()
            .Where(row => row.RequestedDateKey == requestedKey)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (header is null)
        {
            return null;
        }

        var rates = await connection.Table<CurrencyRateEntity>()
            .Where(row => row.RequestedDateKey == requestedKey)
            .ToListAsync()
            .ConfigureAwait(false);

        if (rates.Count == 0)
        {
            return null;
        }

        return new RatesForDate
        {
            RequestedDate = FromKey(header.RequestedDateKey),
            ActualDate = FromKey(header.ActualDateKey),
            Rates = rates.Select(ToDomain).ToArray()
        };
    }

    public async Task SaveRatesAsync(
        RatesForDate rates,
        CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var requestedKey = ToKey(rates.RequestedDate);

        var header = new RatesForDateEntity
        {
            RequestedDateKey = requestedKey,
            ActualDateKey = ToKey(rates.ActualDate)
        };

        var rateEntities = rates.Rates.Select(rate => new CurrencyRateEntity
        {
            RequestedDateKey = requestedKey,
            CharCode = rate.Currency.CharCode,
            Name = rate.Currency.Name,
            Nominal = rate.Nominal,
            RublesPerNominal = rate.RublesPerNominal
        }).ToList();

        await connection.RunInTransactionAsync(db =>
        {
            var staleRates = db.Table<CurrencyRateEntity>()
                .Where(row => row.RequestedDateKey == requestedKey)
                .ToList();

            foreach (var staleRate in staleRates)
            {
                db.Delete(staleRate);
            }

            db.InsertOrReplace(header);

            foreach (var entity in rateEntities)
            {
                db.Insert(entity);
            }
        }).ConfigureAwait(false);
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized)
        {
            return;
        }

        await _initializationSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_isInitialized)
            {
                return;
            }

            await connection.CreateTableAsync<RatesForDateEntity>().ConfigureAwait(false);
            await connection.CreateTableAsync<CurrencyRateEntity>().ConfigureAwait(false);

            _isInitialized = true;
        }
        finally
        {
            _initializationSemaphore.Release();
        }
    }

    private static CurrencyRate ToDomain(CurrencyRateEntity entity) =>
        new()
        {
            Currency = new Currency
            {
                CharCode = entity.CharCode,
                Name = entity.Name
            },
            Nominal = entity.Nominal,
            RublesPerNominal = entity.RublesPerNominal
        };

    private static string ToKey(DateOnly date) =>
        date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

    private static DateOnly FromKey(string value) =>
        DateOnly.ParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture);
}
