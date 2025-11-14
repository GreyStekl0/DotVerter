using DotVerter.Data.Remote.Cbr.DTO;
using DotVerter.Domain.Models;

namespace DotVerter.Data.Remote.Cbr;

public static class Mapper
{
    public static RatesForDate ToDomain(
        this CbrDailyResponseDto dto,
        DateOnly requestedDate)
    {
        var actualDate = DateOnly.FromDateTime(dto.Date);

        var rates = dto.Valute.Values
            .Select(valuteDto => new CurrencyRate
            {
                Currency = new Currency
                {
                    CharCode = valuteDto.CharCode,
                    Name = valuteDto.Name
                },
                Nominal = valuteDto.Nominal,
                RublesPerNominal = valuteDto.Value
            })
            .ToArray();

        return new RatesForDate
        {
            RequestedDate = requestedDate,
            ActualDate = actualDate,
            Rates = rates
        };
    }
}