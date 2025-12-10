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

    extension(CurrencyDto dto)
    {
        /// <summary>
        ///     DTO -> Entity
        /// </summary>
        public CurrencyEntity ToEntity(DateTime rateDate)
        {
            return new CurrencyEntity
            {
                CharCode = dto.CharCode,
                Nominal = dto.Nominal,
                Name = dto.Name,
                Value = dto.Value,
                RateDate = rateDate.Date
            };
        }

        /// <summary>
        ///     DTO -> Domain Model
        /// </summary>
        public Currency ToModel()
        {
            return new Currency
            {
                CharCode = dto.CharCode,
                Nominal = dto.Nominal,
                Name = dto.Name,
                Value = dto.Value
            };
        }
    }

    /// <summary>
    ///     Коллекция Entity -> Domain Models
    /// </summary>
    public static IEnumerable<Currency> ToModels(this IEnumerable<CurrencyEntity> entities)
    {
        return entities.Select(e => e.ToModel());
    }

    extension(IEnumerable<CurrencyDto> dtos)
    {
        /// <summary>
        ///     Коллекция DTO -> Entities
        /// </summary>
        public IEnumerable<CurrencyEntity> ToEntities(DateTime rateDate)
        {
            return dtos.Select(d => d.ToEntity(rateDate));
        }

        /// <summary>
        ///     Коллекция DTO -> Domain Models
        /// </summary>
        public IEnumerable<Currency> ToModels()
        {
            return dtos.Select(d => d.ToModel());
        }
    }
}