using Data.Database.Entities;
using SQLite;

namespace Data.Database;

/// <summary>
///     Контекст базы данных SQLite для хранения курсов валют
/// </summary>
public class CurrencyDbContext(string dbPath)
{
    private readonly SQLiteAsyncConnection _database = new(dbPath);
    private readonly SemaphoreSlim _initializeLock = new(1, 1);
    private bool _initialized;

    /// <summary>
    ///     Инициализация базы данных (создание таблиц)
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
            return;

        await _initializeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
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
    public async Task<List<CurrencyEntity>> GetCurrenciesByRequestedDateAsync(DateTime requestedDate,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        var dateOnly = requestedDate.Date;
        return await _database
            .Table<CurrencyEntity>()
            .Where(c => c.RequestedDate == dateOnly)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Сохранить курсы валют для запрошенной даты
    /// </summary>
    public async Task SaveCurrenciesAsync(IEnumerable<CurrencyEntity> currencies,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        await _database.InsertAllAsync(currencies).ConfigureAwait(false);
    }
}
