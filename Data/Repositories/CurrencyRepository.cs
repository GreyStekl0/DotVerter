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
    public async Task<IEnumerable<Currency>> GetTodayRatesAsync()
    {
        return await GetRatesByDateAsync(DateOnly.FromDateTime(DateTime.Today));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Currency>> GetRatesByDateAsync(DateOnly date)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);

        if (!await _dbContext.HasDataForDateAsync(dateTime)) return await FetchAndSaveRatesAsync(date);
        var entities = await _dbContext.GetCurrenciesByDateAsync(dateTime);
        return entities.ToModels();
    }

    /// <summary>
    ///     Загрузить курсы из сети и сохранить в БД
    /// </summary>
    private async Task<IEnumerable<Currency>> FetchAndSaveRatesAsync(DateOnly date)
    {
        var response = await _apiService.GetRatesByDateAsync(date);

        if (response?.Valute == null || response.Valute.Count == 0)
            return [];

        var dtos = response.Valute.Values;
        var rateDate = response.Date;

        // Сохраняем в БД
        var entities = dtos.ToEntities(rateDate);
        await _dbContext.SaveCurrenciesAsync(entities);

        // Возвращаем Domain модели
        return dtos.ToModels();
    }
}