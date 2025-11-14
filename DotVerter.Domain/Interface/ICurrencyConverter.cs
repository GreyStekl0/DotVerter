using DotVerter.Domain.Models;

namespace DotVerter.Domain.Interface;

public interface ICurrencyConverter
{
    Money Convert(
        Money from,
        Currency to,
        RatesForDate rates);
}