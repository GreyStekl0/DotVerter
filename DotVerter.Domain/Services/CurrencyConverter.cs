using DotVerter.Domain.Interface;
using DotVerter.Domain.Models;

namespace DotVerter.Domain.Services;

public sealed class CurrencyConverter : ICurrencyConverter
{
    public Money Convert(
        Money from,
        Currency to,
        RatesForDate rates)
    {
        if (from.Currency.CharCode == to.CharCode)
            return from;

        var moneyInRub = ToRub(from, rates);
        return FromRub(moneyInRub, to, rates);
    }

    private static Money ToRub(Money money, RatesForDate rates)
    {
        if (money.Currency.CharCode == "RUB")
            return money;

        var rate = rates.Rates.Single(r => r.Currency.CharCode == money.Currency.CharCode);
        var rubPerOne = rate.RublesPerOne;
        var rubAmount = money.Amount * rubPerOne;

        return new Money(rubAmount, new Currency { CharCode = "RUB", Name = "Российский рубль" });
    }

    private static Money FromRub(Money rubMoney, Currency to, RatesForDate rates)
    {
        if (to.CharCode == "RUB")
            return rubMoney;

        var rate = rates.Rates.Single(r => r.Currency.CharCode == to.CharCode);
        var rubPerOne = rate.RublesPerOne;
        var targetAmount = rubMoney.Amount / rubPerOne;

        return new Money(targetAmount, to);
    }
}