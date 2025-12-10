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
    private const string PrefSelectedDate = "SelectedDate";
    private const string PrefFromCurrency = "FromCurrency";
    private const string PrefToCurrency = "ToCurrency";
    private const string PrefFromAmount = "FromAmount";

    [ObservableProperty] private ObservableCollection<CurrencyItem> _currencies = [];

    [ObservableProperty] private string _fromAmount = "1";

    [ObservableProperty] private CurrencyItem? _fromCurrency;

    [ObservableProperty] private bool _isLoading;

    private bool _isUpdating;
    private bool _isInitialized;

    [ObservableProperty] private DateTime _maxDate = DateTime.Today;

    [ObservableProperty] private string _rateInfoText = string.Empty;

    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty] private string _toAmount = string.Empty;

    [ObservableProperty] private CurrencyItem? _toCurrency;

    partial void OnSelectedDateChanged(DateTime value)
    {
        Debug.WriteLine($"[VM] Date changed to: {value:yyyy-MM-dd}");
        SaveState();
        _ = LoadCurrenciesAsync();
    }

    partial void OnFromCurrencyChanged(CurrencyItem? value)
    {
        SaveState();
        ConvertFromSource();
    }

    partial void OnToCurrencyChanged(CurrencyItem? value)
    {
        SaveState();
        ConvertFromSource();
    }

    partial void OnFromAmountChanged(string value)
    {
        if (!_isUpdating)
        {
            SaveState();
            ConvertFromSource();
        }
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

            // При первой загрузке используем сохранённые значения
            if (!_isInitialized)
            {
                previousFromCode = Preferences.Default.Get(PrefFromCurrency, "RUB");
                previousToCode = Preferences.Default.Get(PrefToCurrency, "USD");
            }

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

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM] Exception: {ex.Message}");
            Debug.WriteLine($"[VM] StackTrace: {ex.StackTrace}");
            RateInfoText = "Ошибка загрузки курсов";
        }
        finally
        {
            IsLoading = false;
            Debug.WriteLine("[VM] LoadCurrenciesAsync finished");
        }
    }

    /// <summary>
    /// Загружает сохранённое состояние из Preferences
    /// </summary>
    public void RestoreState()
    {
        _isUpdating = true;
        try
        {
            var savedDateTicks = Preferences.Default.Get(PrefSelectedDate, DateTime.Today.Ticks);
            var savedDate = new DateTime(savedDateTicks);
            
            // Убеждаемся, что дата не больше максимальной
            SelectedDate = savedDate <= MaxDate ? savedDate : MaxDate;
            
            FromAmount = Preferences.Default.Get(PrefFromAmount, "1");
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void SaveState()
    {
        if (!_isInitialized)
            return;

        Preferences.Default.Set(PrefSelectedDate, SelectedDate.Ticks);
        Preferences.Default.Set(PrefFromCurrency, FromCurrency?.CharCode ?? "RUB");
        Preferences.Default.Set(PrefToCurrency, ToCurrency?.CharCode ?? "USD");
        Preferences.Default.Set(PrefFromAmount, FromAmount ?? "1");
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