using Domain.Models;

namespace Domain.Repositories;

/// <summary>
///     Репозиторий для работы с курсами валют.
///     Предоставляет данные из БД, при отсутствии загружает из сети и кэширует.
/// </summary>
public interface ICurrencyRepository
{
    /// <summary>
    ///     Получить курсы валют на указанную дату.
    ///     Если на указанную дату курс не установлен, возвращает ближайший предыдущий курс.
    /// </summary>
    /// <param name="date">Дата для получения курсов</param>
    Task<CurrencyRatesResult> GetRatesByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
}
