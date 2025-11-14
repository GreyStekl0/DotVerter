using CommunityToolkit.Mvvm.ComponentModel;
using DotVerter.Domain.Interface;
using DotVerter.Domain.Models;

namespace DotVerter.UI;

public partial class ConverterViewModel : ObservableObject
{
    private readonly IExchangeRateRepository _exchangeRateRepository;

    public static DateTime Today => DateTime.Now.Date;
    public DateTime MinDate { get; } = new(1992, 7, 1);

    [ObservableProperty] private DateTime _selectedDate = Today;

    [ObservableProperty] private RatesForDate? _ratesForDate;

    [ObservableProperty] private CurrencyRate? _currencyRateTo;

    [ObservableProperty] private CurrencyRate? _currencyRateFrom;

    [ObservableProperty] private IReadOnlyCollection<CurrencyRate>? _rates;

    [ObservableProperty] private decimal _amountFrom;

    [ObservableProperty] private decimal _amountTo;
    
    private bool _isUpdating;

    public ConverterViewModel(IExchangeRateRepository exchangeRateRepository)
    {
        _exchangeRateRepository = exchangeRateRepository;
        _ = LoadRatesAsync(SelectedDate);
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        _ = LoadRatesAsync(value);
    }

    partial void OnAmountFromChanged(decimal value)
    {
        if (_isUpdating) return;
        CalculateFromToTo();
    }
    
    partial void OnAmountToChanged(decimal value)
    {
        if (_isUpdating) return;
        CalculateFromToFrom();
    }

    partial void OnCurrencyRateFromChanged(CurrencyRate? value)
    {
        CalculateFromToTo();
    }

    partial void OnCurrencyRateToChanged(CurrencyRate? value)
    {
        CalculateFromToTo();
    }

    private void CalculateFromToTo()
    {
        if (CurrencyRateFrom == null || CurrencyRateTo == null)
        {
            _isUpdating = true;
            AmountTo = 0;
            _isUpdating = false;
            return;
        }

        // Конвертация через рубли: сумма FROM → рубли → TO
        var rublesAmount = AmountFrom * CurrencyRateFrom.RublesPerOne;
        _isUpdating = true;
        AmountTo = Math.Round(rublesAmount / CurrencyRateTo.RublesPerOne, 3);
        _isUpdating = false;
    }
    
    private void CalculateFromToFrom()
    {
        if (CurrencyRateFrom == null || CurrencyRateTo == null)
        {
            _isUpdating = true;
            AmountFrom = 0;
            _isUpdating = false;
            return;
        }

        var rublesAmount = AmountTo * CurrencyRateTo.RublesPerOne;
        _isUpdating = true;
        AmountFrom = Math.Round(rublesAmount / CurrencyRateFrom.RublesPerOne, 3);
        _isUpdating = false;
    }

    private async Task LoadRatesAsync(DateTime date)
    {
        var dateOnly = DateOnly.FromDateTime(date);
        RatesForDate = await _exchangeRateRepository.GetRatesAsync(dateOnly);
        Rates = RatesForDate?.Rates;
    }
}