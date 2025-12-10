using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Repositories;
using UI.Models;

namespace UI.ViewModels;

public partial class CurrencyConverterViewModel(ICurrencyRepository repository) : ObservableObject
{
    [ObservableProperty] private ObservableCollection<CurrencyItem> _currencies = [];

    [ObservableProperty] private string _fromAmount = "1";

    [ObservableProperty] private CurrencyItem? _fromCurrency;

    [ObservableProperty] private bool _isLoading;

    private bool _isUpdating;

    [ObservableProperty] private DateTime _maxDate = DateTime.Today;

    [ObservableProperty] private string _rateInfoText = string.Empty;

    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty] private string _toAmount = string.Empty;

    [ObservableProperty] private CurrencyItem? _toCurrency;

    partial void OnSelectedDateChanged(DateTime value)
    {
        Debug.WriteLine($"[VM] Date changed to: {value:yyyy-MM-dd}");
        _ = LoadCurrenciesAsync();
    }

    partial void OnFromCurrencyChanged(CurrencyItem? value)
    {
        ConvertFromSource();
    }

    partial void OnToCurrencyChanged(CurrencyItem? value)
    {
        ConvertFromSource();
    }

    partial void OnFromAmountChanged(string value)
    {
        if (!_isUpdating)
            ConvertFromSource();
    }

    partial void OnToAmountChanged(string value)
    {
        if (!_isUpdating)
            ConvertFromTarget();
    }

    [RelayCommand]
    private async Task LoadCurrenciesAsync()
    {
        Debug.WriteLine("[VM] LoadCurrenciesAsync started");
        IsLoading = true;

        try
        {
            var date = DateOnly.FromDateTime(SelectedDate);
            Debug.WriteLine($"[VM] Requesting data for: {date:yyyy-MM-dd}");

            var result = await repository.GetRatesByDateAsync(date);

            Debug.WriteLine($"[VM] Received {result.Currencies.Count()} currencies");
            Debug.WriteLine(
                $"[VM] RequestedDate: {result.RequestedDate:yyyy-MM-dd}, ActualDate: {result.ActualDate:yyyy-MM-dd}");

            RateInfoText = $"Курс на {result.ActualDate:d MMMM yyyy}";

            var previousFromCode = FromCurrency?.CharCode;
            var previousToCode = ToCurrency?.CharCode;

            var items = new List<CurrencyItem>
            {
                new()
                {
                    CharCode = "RUB",
                    Name = "Российский рубль",
                    Nominal = 1,
                    Value = 1
                }
            };

            items.AddRange(result.Currencies.Select(c => new CurrencyItem
            {
                CharCode = c.CharCode,
                Name = c.Name,
                Nominal = c.Nominal,
                Value = c.Value
            }));

            Debug.WriteLine($"[VM] Total items after adding RUB: {items.Count}");

            foreach (var item in items.Take(5))
                Debug.WriteLine($"[VM] Currency: {item.CharCode} - {item.Name} ({item.Nominal} = {item.Value})");

            Currencies = new ObservableCollection<CurrencyItem>(items);

            Debug.WriteLine($"[VM] Currencies collection updated with {Currencies.Count} items");

            FromCurrency = Currencies.FirstOrDefault(c => c.CharCode == previousFromCode)
                           ?? Currencies.FirstOrDefault(c => c.CharCode == "RUB");

            ToCurrency = Currencies.FirstOrDefault(c => c.CharCode == previousToCode)
                         ?? Currencies.FirstOrDefault(c => c.CharCode == "USD");

            Debug.WriteLine($"[VM] FromCurrency: {FromCurrency?.CharCode}, ToCurrency: {ToCurrency?.CharCode}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM] Exception: {ex.Message}");
            Debug.WriteLine($"[VM] StackTrace: {ex.StackTrace}");
            RateInfoText = "Oshibka zagruzki kursov";
        }
        finally
        {
            IsLoading = false;
            Debug.WriteLine("[VM] LoadCurrenciesAsync finished");
        }
    }

    private void ConvertFromSource()
    {
        if (FromCurrency == null || ToCurrency == null)
            return;

        if (!decimal.TryParse(FromAmount, NumberStyles.Any, CultureInfo.CurrentCulture, out var amount))
            return;

        _isUpdating = true;

        try
        {
            var amountInRub = amount * FromCurrency.RatePerUnit;
            var result = amountInRub / ToCurrency.RatePerUnit;

            ToAmount = Math.Round(result, 2).ToString(CultureInfo.CurrentCulture);
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void ConvertFromTarget()
    {
        if (FromCurrency == null || ToCurrency == null)
            return;

        if (!decimal.TryParse(ToAmount, NumberStyles.Any, CultureInfo.CurrentCulture, out var amount))
            return;

        _isUpdating = true;

        try
        {
            var amountInRub = amount * ToCurrency.RatePerUnit;
            var result = amountInRub / FromCurrency.RatePerUnit;

            FromAmount = Math.Round(result, 2).ToString(CultureInfo.CurrentCulture);
        }
        finally
        {
            _isUpdating = false;
        }
    }
}