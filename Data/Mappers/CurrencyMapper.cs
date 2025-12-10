using Data.Database.Entities;
using Data.Dto;
using Domain.Models;

namespace Data.Mappers;

/// <summary>
///     Маппер между DTO, Entity и Domain моделями
/// </summary>
internal static class CurrencyMapper
{
    /// <summary>
    ///     Entity -> Domain Model
    /// </summary>
    public static Currency ToModel(this CurrencyEntity entity)
    {
        return new Currency
        {
            CharCode = entity.CharCode,
            Nominal = entity.Nominal,
            Name = entity.Name,
            Value = entity.Value
        };
    }

    /// <summary>
    ///     DTO -> Entity
    /// </summary>
    public static CurrencyEntity ToEntity(this CurrencyDto dto, DateTime requestedDate, DateTime actualDate)
    {
        return new CurrencyEntity
        {
            CharCode = dto.CharCode,
            Nominal = dto.Nominal,
            Name = dto.Name,
            Value = dto.Value,
            RequestedDate = requestedDate.Date,
            ActualDate = actualDate.Date
        };
    }

    /// <summary>
    ///     DTO -> Domain Model
    /// </summary>
    public static Currency ToModel(this CurrencyDto dto)
    {
        return new Currency
        {
            CharCode = dto.CharCode,
            Nominal = dto.Nominal,
            Name = dto.Name,
            Value = dto.Value
        };
    }

    /// <summary>
    ///     Коллекция Entity -> Domain Models
    /// </summary>
    public static IEnumerable<Currency> ToModels(this IEnumerable<CurrencyEntity> entities)
    {
        return entities.Select(e => e.ToModel());
    }

    /// <summary>
    ///     Коллекция DTO -> Entities
    /// </summary>
    public static IEnumerable<CurrencyEntity> ToEntities(this IEnumerable<CurrencyDto> dtos, DateTime requestedDate, DateTime actualDate)
    {
        return dtos.Select(d => d.ToEntity(requestedDate, actualDate));
    }

    /// <summary>
    ///     Коллекция DTO -> Domain Models
    /// </summary>
    public static IEnumerable<Currency> ToModels(this IEnumerable<CurrencyDto> dtos)
    {
        return dtos.Select(d => d.ToModel());
    }
}