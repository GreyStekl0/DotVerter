using Data.Database.Entities;
using SQLite;

namespace Data.Database;

/// <summary>
///      онтекст базы данных SQLite дл€ хранени€ курсов валют
/// </summary>
public class CurrencyDbContext
{
    private readonly SQLiteAsyncConnection _database;
    private bool _initialized;

    public CurrencyDbContext(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
    }

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
    ///     ѕолучить все курсы по запрошенной дате
    /// </summary>
    public async Task<List<CurrencyEntity>> GetCurrenciesByRequestedDateAsync(DateTime requestedDate)
    {
        await InitializeAsync();

        var dateOnly = requestedDate.Date;
        return await _database.Table<CurrencyEntity>()
            .Where(c => c.RequestedDate == dateOnly)
            .ToListAsync();
    }

    /// <summary>
    ///     ѕровер€ет, есть ли данные дл€ запрошенной даты
    /// </summary>
    public async Task<bool> HasDataForRequestedDateAsync(DateTime requestedDate)
    {
        await InitializeAsync();

        var dateOnly = requestedDate.Date;
        var count = await _database.Table<CurrencyEntity>()
            .Where(c => c.RequestedDate == dateOnly)
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