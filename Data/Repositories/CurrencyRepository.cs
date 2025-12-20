using Data.Database;
using Data.Mappers;
using Data.Services;
using Domain.Models;
using Domain.Repositories;

namespace Data.Repositories;

/// <summary>
///     Репозиторий для работы с курсами валют.
///     Кэширует данные в локальной БД, при необходимости загружает из сети.
/// </summary>
public class CurrencyRepository(ICbrApiService apiService, CurrencyDbContext dbContext) : ICurrencyRepository
{
    /// <inheritdoc />
    public async Task<CurrencyRatesResult> GetRatesByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var requestedDateTime = date.ToDateTime(TimeOnly.MinValue);

        // Проверяем наличие данных в БД
        var entities = await dbContext.GetCurrenciesByRequestedDateAsync(requestedDateTime, cancellationToken)
            .ConfigureAwait(false);

        if (entities.Count <= 0) return await FetchAndSaveRatesAsync(date, cancellationToken).ConfigureAwait(false);
        var actualDate = entities[0].ActualDate;

        return new CurrencyRatesResult
        {
            RequestedDate = date,
            ActualDate = DateOnly.FromDateTime(actualDate),
            Currencies = entities.ToModels()
        };

    }

    private async Task<CurrencyRatesResult> FetchAndSaveRatesAsync(DateOnly requestedDate, CancellationToken cancellationToken)
    {
        var response = await apiService.GetRatesByDateAsync(requestedDate, cancellationToken).ConfigureAwait(false);

        if (response?.Valute == null || response.Valute.Count == 0)
        {
            return new CurrencyRatesResult
            {
                RequestedDate = requestedDate,
                ActualDate = requestedDate,
                Currencies = []
            };
        }

        var actualDate = response.RequestedArchiveDate ?? requestedDate;
        var requestedDateTime = requestedDate.ToDateTime(TimeOnly.MinValue);
        var actualDateTime = actualDate.ToDateTime(TimeOnly.MinValue);

        var dtoValues = response.Valute.Values;
        var currencyEntities = dtoValues.ToEntities(requestedDateTime, actualDateTime).ToArray();
        var currencies = dtoValues.ToModels().ToArray();

        await dbContext.SaveCurrenciesAsync(currencyEntities, cancellationToken)
            .ConfigureAwait(false);

        return new CurrencyRatesResult
        {
            RequestedDate = requestedDate,
            ActualDate = actualDate,
            Currencies = currencies
        };
    }
}
