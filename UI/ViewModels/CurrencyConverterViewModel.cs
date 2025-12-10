using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Repositories;
using UI.Models;

namespace UI.ViewModels;

public partial class CurrencyConverterViewModel : ObservableObject
{
    private readonly ICurrencyRepository _repository;
    private bool _isUpdating;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _maxDate = DateTime.Today;

    [ObservableProperty]
    private string _rateInfoText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CurrencyItem> _currencies = [];

    [ObservableProperty]
    private CurrencyItem? _fromCurrency;

    [ObservableProperty]
    private CurrencyItem? _toCurrency;

    [ObservableProperty]
    private string _fromAmount = "1";

    [ObservableProperty]
    private string _toAmount = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public CurrencyConverterViewModel(ICurrencyRepository repository)
    {
        _repository = repository;
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        System.Diagnostics.Debug.WriteLine($"[VM] Date changed to: {value:yyyy-MM-dd}");
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
        System.Diagnostics.Debug.WriteLine($"[VM] LoadCurrenciesAsync started");
        IsLoading = true;

        try
        {
            var date = DateOnly.FromDateTime(SelectedDate);
            System.Diagnostics.Debug.WriteLine($"[VM] Requesting data for: {date:yyyy-MM-dd}");

            var result = await _repository.GetRatesByDateAsync(date);

            System.Diagnostics.Debug.WriteLine($"[VM] Received {result.Currencies.Count()} currencies");
            System.Diagnostics.Debug.WriteLine($"[VM] RequestedDate: {result.RequestedDate:yyyy-MM-dd}, ActualDate: {result.ActualDate:yyyy-MM-dd}");

            RateInfoText = $"Kurs na {result.ActualDate:d MMMM yyyy}";

            var previousFromCode = FromCurrency?.CharCode;
            var previousToCode = ToCurrency?.CharCode;

            var items = new List<CurrencyItem>
            {
                new()
                {
                    CharCode = "RUB",
                    Name = "Rossiyskiy rubl",
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

            System.Diagnostics.Debug.WriteLine($"[VM] Total items after adding RUB: {items.Count}");

            foreach (var item in items.Take(5))
            {
                System.Diagnostics.Debug.WriteLine($"[VM] Currency: {item.CharCode} - {item.Name} ({item.Nominal} = {item.Value})");
            }

            Currencies = new ObservableCollection<CurrencyItem>(items);

            System.Diagnostics.Debug.WriteLine($"[VM] Currencies collection updated with {Currencies.Count} items");

            FromCurrency = Currencies.FirstOrDefault(c => c.CharCode == previousFromCode)
                           ?? Currencies.FirstOrDefault(c => c.CharCode == "RUB");

            ToCurrency = Currencies.FirstOrDefault(c => c.CharCode == previousToCode)
                         ?? Currencies.FirstOrDefault(c => c.CharCode == "USD");

            System.Diagnostics.Debug.WriteLine($"[VM] FromCurrency: {FromCurrency?.CharCode}, ToCurrency: {ToCurrency?.CharCode}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VM] Exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[VM] StackTrace: {ex.StackTrace}");
            RateInfoText = "Oshibka zagruzki kursov";
        }
        finally
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine($"[VM] LoadCurrenciesAsync finished");
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
