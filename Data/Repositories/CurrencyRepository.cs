using System.Diagnostics;
using Data.Database;
using Data.Database.Entities;
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

        // Проверяем наличие данных в БД
        if (await _dbContext.HasDataForRequestedDateAsync(requestedDateTime))
        {
            // Данные есть в БД — возвращаем их
            var entities = await _dbContext.GetCurrenciesByRequestedDateAsync(requestedDateTime);
            var actualDate = entities.FirstOrDefault()?.ActualDate ?? requestedDateTime;

            Debug.WriteLine($"[CACHE] Loaded {entities.Count} currencies from DB for {date}");

            return new CurrencyRatesResult
            {
                RequestedDate = date,
                ActualDate = DateOnly.FromDateTime(actualDate),
                Currencies = entities.ToModels()
            };
        }

        // Данных нет — загружаем из сети и сохраняем в БД
        Debug.WriteLine($"[API] Fetching data for {date}");
        return await FetchAndSaveRatesAsync(date);
    }

    /// <summary>
    ///     Загрузить курсы из сети и сохранить в БД
    /// </summary>
    private async Task<CurrencyRatesResult> FetchAndSaveRatesAsync(DateOnly requestedDate)
    {
        var response = await _apiService.GetRatesByDateAsync(requestedDate);

        Debug.WriteLine($"[API] Response: {response?.Valute.Count ?? 0} currencies");

        if (response?.Valute == null || response.Valute.Count == 0)
        {
            Debug.WriteLine($"[API] No data received for {requestedDate}");
            return new CurrencyRatesResult
            {
                RequestedDate = requestedDate,
                ActualDate = requestedDate,
                Currencies = []
            };
        }

        var dtos = response.Valute.Values;
        var actualDate = response.Date;
        var requestedDateTime = requestedDate.ToDateTime(TimeOnly.MinValue);

        Debug.WriteLine($"[API] Received {dtos.Count} currencies, actual date: {actualDate:yyyy-MM-dd}");

        // Сохраняем в БД с обеими датами
        var entities = dtos.ToEntities(requestedDateTime, actualDate);
        var currencyEntities = entities as CurrencyEntity[] ?? entities.ToArray();
        await _dbContext.SaveCurrenciesAsync(currencyEntities);

        Debug.WriteLine($"[DB] Saved {currencyEntities.Length} currencies to DB");

        // Возвращаем результат с информацией о датах
        return new CurrencyRatesResult
        {
            RequestedDate = requestedDate,
            ActualDate = DateOnly.FromDateTime(actualDate),
            Currencies = dtos.ToModels()
        };
    }
}