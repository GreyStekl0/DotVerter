using Data.Database.Entities;
using SQLite;

namespace Data.Database;

/// <summary>
///      онтекст базы данных SQLite дл€ хранени€ курсов валют
/// </summary>
internal class CurrencyDbContext(string dbPath)
{
    private readonly SQLiteAsyncConnection _database = new(dbPath);
    private bool _initialized;

    /// <summary>
    ///     »нициализаци€ базы данных (создание таблиц)
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        await _database.CreateTableAsync<CurrencyEntity>();
        _initialized = true;
    }

    /// <summary>
    ///     ѕолучить все курсы на указанную дату
    /// </summary>
    public async Task<List<CurrencyEntity>> GetCurrenciesByDateAsync(DateTime date)
    {
        await InitializeAsync();

        var dateOnly = date.Date;
        return await _database.Table<CurrencyEntity>()
            .Where(c => c.RateDate == dateOnly)
            .ToListAsync();
    }

    /// <summary>
    ///     ѕроверить, есть ли данные на указанную дату
    /// </summary>
    public async Task<bool> HasDataForDateAsync(DateTime date)
    {
        await InitializeAsync();

        var dateOnly = date.Date;
        var count = await _database.Table<CurrencyEntity>()
            .Where(c => c.RateDate == dateOnly)
            .CountAsync();

        return count > 0;
    }

    /// <summary>
    ///     —охранить курсы валют
    /// </summary>
    public async Task SaveCurrenciesAsync(IEnumerable<CurrencyEntity> currencies)
    {
        await InitializeAsync();
        await _database.InsertAllAsync(currencies);
    }
}