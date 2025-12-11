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
        if (await dbContext.HasDataForRequestedDateAsync(requestedDateTime))
        {
            var entities = await dbContext.GetCurrenciesByRequestedDateAsync(requestedDateTime);
            var actualDate = entities.FirstOrDefault()?.ActualDate ?? requestedDateTime;

            return new CurrencyRatesResult
            {
                RequestedDate = date,
                ActualDate = DateOnly.FromDateTime(actualDate),
                Currencies = entities.ToModels()
            };
        }

        return await FetchAndSaveRatesAsync(date);
    }

    private async Task<CurrencyRatesResult> FetchAndSaveRatesAsync(DateOnly requestedDate)
    {
        var response = await apiService.GetRatesByDateAsync(requestedDate);

        if (response?.Valute == null || response.Valute.Count == 0)
        {
            return new CurrencyRatesResult
            {
                RequestedDate = requestedDate,
                ActualDate = requestedDate,
                Currencies = []
            };
        }

        var dtos = response.Valute.Values;

        var actualDate = response.RequestedArchiveDate ?? requestedDate;
        var requestedDateTime = requestedDate.ToDateTime(TimeOnly.MinValue);
        var actualDateTime = actualDate.ToDateTime(TimeOnly.MinValue);

        var entities = dtos.ToEntities(requestedDateTime, actualDateTime);
        var currencyEntities = entities as CurrencyEntity[] ?? entities.ToArray();
        await dbContext.SaveCurrenciesAsync(currencyEntities);

        return new CurrencyRatesResult
        {
            RequestedDate = requestedDate,
            ActualDate = actualDate,
            Currencies = dtos.ToModels()
        };
    }
}