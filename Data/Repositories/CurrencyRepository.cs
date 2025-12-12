using Data.Database;
using Data.Database.Entities;
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
    public Task<CurrencyRatesResult> GetTodayRatesAsync()
    {
        // Используем локальную дату пользователя
        return GetRatesByDateAsync(DateOnly.FromDateTime(DateTime.Today));
    }

    /// <inheritdoc />
    public async Task<CurrencyRatesResult> GetRatesByDateAsync(DateOnly date)
    {
        var requestedDateTime = date.ToDateTime(TimeOnly.MinValue);

        // Проверяем наличие данных в БД
        if (await dbContext.HasDataForRequestedDateAsync(requestedDateTime).ConfigureAwait(false))
        {
            var entities = await dbContext.GetCurrenciesByRequestedDateAsync(requestedDateTime).ConfigureAwait(false);
            var actualDate = entities.Count > 0 ? entities[0].ActualDate : requestedDateTime;

            return new CurrencyRatesResult
            {
                RequestedDate = date,
                ActualDate = DateOnly.FromDateTime(actualDate),
                Currencies = entities.ToModels()
            };
        }

        return await FetchAndSaveRatesAsync(date).ConfigureAwait(false);
    }

    private async Task<CurrencyRatesResult> FetchAndSaveRatesAsync(DateOnly requestedDate)
    {
        var response = await apiService.GetRatesByDateAsync(requestedDate).ConfigureAwait(false);

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

        var dtoValues = response.Valute.Values.ToArray();
        var currencyEntities = new CurrencyEntity[dtoValues.Length];
        var currencies = new Currency[dtoValues.Length];

        for (var i = 0; i < dtoValues.Length; i++)
        {
            var dto = dtoValues[i];
            currencyEntities[i] = dto.ToEntity(requestedDateTime, actualDateTime);
            currencies[i] = dto.ToModel();
        }

        await dbContext.SaveCurrenciesAsync(currencyEntities).ConfigureAwait(false);

        return new CurrencyRatesResult
        {
            RequestedDate = requestedDate,
            ActualDate = actualDate,
            Currencies = currencies
        };
    }
}
