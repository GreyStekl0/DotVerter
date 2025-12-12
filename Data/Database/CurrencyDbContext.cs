using Data.Database.Entities;
using SQLite;

namespace Data.Database;

/// <summary>
///     Контекст базы данных SQLite для хранения курсов валют
/// </summary>
public class CurrencyDbContext
{
    private readonly SQLiteAsyncConnection _database;
    private readonly SemaphoreSlim _initializeLock = new(1, 1);
    private bool _initialized;

    public CurrencyDbContext(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
    }

    /// <summary>
    ///     Инициализация базы данных (создание таблиц)
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        await _initializeLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_initialized)
                return;

            await _database.CreateTableAsync<CurrencyEntity>().ConfigureAwait(false);
            _initialized = true;
        }
        finally
        {
            _initializeLock.Release();
        }
    }

    /// <summary>
    ///     Получить все курсы по запрошенной дате
    /// </summary>
    public async Task<List<CurrencyEntity>> GetCurrenciesByRequestedDateAsync(DateTime requestedDate)
    {
        await InitializeAsync().ConfigureAwait(false);

        var dateOnly = requestedDate.Date;
        return await _database
            .Table<CurrencyEntity>()
            .Where(c => c.RequestedDate == dateOnly)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Проверяет, есть ли данные для запрошенной даты
    /// </summary>
    public async Task<bool> HasDataForRequestedDateAsync(DateTime requestedDate)
    {
        await InitializeAsync().ConfigureAwait(false);

        var dateOnly = requestedDate.Date;
        var count = await _database
            .Table<CurrencyEntity>()
            .Where(c => c.RequestedDate == dateOnly)
            .CountAsync()
            .ConfigureAwait(false);

        return count > 0;
    }

    /// <summary>
    ///     Сохранить курсы валют
    /// </summary>
    public async Task SaveCurrenciesAsync(IEnumerable<CurrencyEntity> currencies)
    {
        await InitializeAsync().ConfigureAwait(false);
        await _database.InsertAllAsync(currencies).ConfigureAwait(false);
    }
}
