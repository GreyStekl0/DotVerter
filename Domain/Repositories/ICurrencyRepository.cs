using Domain.Models;

namespace Domain.Repositories;

/// <summary>
///     Репозиторий для работы с курсами валют.
///     Предоставляет данные из БД, при отсутствии загружает из сети и кэширует.
/// </summary>
public interface ICurrencyRepository
{
    /// <summary>
    ///     Получить актуальные курсы валют на сегодня
    /// </summary>
    Task<IEnumerable<Currency>> GetTodayRatesAsync();

    /// <summary>
    ///     Получить курсы валют на указанную дату
    /// </summary>
    /// <param name="date">Дата для получения курсов</param>
    Task<IEnumerable<Currency>> GetRatesByDateAsync(DateOnly date);
}