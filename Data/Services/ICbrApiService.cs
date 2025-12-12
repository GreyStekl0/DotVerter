using Data.Dto;

namespace Data.Services;

/// <summary>
///     Интерфейс для работы с API ЦБ РФ
/// </summary>
public interface ICbrApiService
{
    /// <summary>
    ///     Получить курсы валют на указанную дату
    /// </summary>
    Task<CbrResponseDto?> GetRatesByDateAsync(DateOnly date);
}
