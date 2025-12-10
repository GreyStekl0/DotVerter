using Data.Database;
using Data.Mappers;
using Data.Services;
using Domain.Models;
using Domain.Repositories;

namespace Data.Repositories;

/// <summary>
///     Репозиторий для работы с курсами валют.
///     Кэширует данные в локальной БД, при отсутствии загружает из сети.
/// </summary>
public class CurrencyRepository(string dbPath) : ICurrencyRepository
{
    private readonly CbrApiService _apiService = new();
    private readonly CurrencyDbContext _dbContext = new(dbPath);

    /// <inheritdoc />
    public async Task<CurrencyRatesResult> GetTodayRatesAsync()
    {
        return await GetRatesByDateAsync(DateOnly.FromDateTime(DateTime.Today));
    }

    /// <inheritdoc />
    public async Task<CurrencyRatesResult> GetRatesByDateAsync(DateOnly date)
    {
        var requestedDateTime = date.ToDateTime(TimeOnly.MinValue);

        if (!await _dbContext.HasDataForRequestedDateAsync(requestedDateTime))
            return await FetchAndSaveRatesAsync(date);
        var entities = await _dbContext.GetCurrenciesByRequestedDateAsync(requestedDateTime);
        var actualDate = entities.FirstOrDefault()?.ActualDate ?? requestedDateTime;

        return new CurrencyRatesResult
        {
            RequestedDate = date,
            ActualDate = DateOnly.FromDateTime(actualDate),
            Currencies = entities.ToModels()
        };

        // Загружаем из сети и сохраняем в БД
    }

    /// <summary>
    ///     Загрузить курсы из сети и сохранить в БД
    /// </summary>
    private async Task<CurrencyRatesResult> FetchAndSaveRatesAsync(DateOnly requestedDate)
    {
        var response = await _apiService.GetRatesByDateAsync(requestedDate);

        if (response?.Valute == null || response.Valute.Count == 0)
            return new CurrencyRatesResult
            {
                RequestedDate = requestedDate,
                ActualDate = requestedDate,
                Currencies = []
            };

        var dtos = response.Valute.Values;
        var actualDate = response.Date;
        var requestedDateTime = requestedDate.ToDateTime(TimeOnly.MinValue);

        // Сохраняем в БД с обеими датами
        var entities = dtos.ToEntities(requestedDateTime, actualDate);
        await _dbContext.SaveCurrenciesAsync(entities);

        // Возвращаем результат с информацией о датах
        return new CurrencyRatesResult
        {
            RequestedDate = requestedDate,
            ActualDate = DateOnly.FromDateTime(actualDate),
            Currencies = dtos.ToModels()
        };
    }
}