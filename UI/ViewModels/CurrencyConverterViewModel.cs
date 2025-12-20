using System.Collections.ObjectModel;
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
    private const int AmountDecimals = 2;

    private bool _isUpdating;
    private bool _hasLoadedCurrencies;
    private bool _reloadPending;

    [ObservableProperty]
    private ObservableCollection<CurrencyItem> currencies = [];

    [ObservableProperty]
    private string fromAmount = "1";

    [ObservableProperty]
    private CurrencyItem? fromCurrency;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private DateTime maxDate = DateTime.Today;

    [ObservableProperty]
    private string rateInfoText = string.Empty;

    [ObservableProperty]
    private DateTime selectedDate = DateTime.Today;

    [ObservableProperty]
    private string toAmount = string.Empty;

    [ObservableProperty]
    private CurrencyItem? toCurrency;

    partial void OnSelectedDateChanged(DateTime value)
    {
        if (_isUpdating)
            return;

        SaveState();
        RequestReload();
    }

    partial void OnFromCurrencyChanged(CurrencyItem? value)
    {
        if (_isUpdating)
            return;

        SaveState();
        ConvertFromSource();
    }

    partial void OnToCurrencyChanged(CurrencyItem? value)
    {
        if (_isUpdating)
            return;

        SaveState();
        ConvertFromSource();
    }

    partial void OnFromAmountChanged(string value)
    {
        if (_isUpdating)
            return;

        SaveState();
        ConvertFromSource();
    }

    partial void OnToAmountChanged(string value)
    {
        if (_isUpdating)
            return;

        ConvertFromTarget();
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        RestoreState();

        try
        {
            await LoadCurrenciesCommand.ExecuteAsync(null);
        }
        catch (OperationCanceledException)
        {
            // Ignore canceled load.
        }
    }

    private void RequestReload()
    {
        if (LoadCurrenciesCommand.IsRunning)
        {
            _reloadPending = true;
            LoadCurrenciesCommand.Cancel();
            return;
        }

        _ = LoadCurrenciesCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private void SwapCurrencies()
    {
        if (FromCurrency == null || ToCurrency == null)
            return;

        _isUpdating = true;
        try
        {
            (FromCurrency, ToCurrency) = (ToCurrency, FromCurrency);
            (FromAmount, ToAmount) = (ToAmount, FromAmount);
        }
        finally
        {
            _isUpdating = false;
        }

        SaveState();
        ConvertFromSource();
    }

    [RelayCommand]
    private async Task LoadCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;

        try
        {
            var date = DateOnly.FromDateTime(SelectedDate);

            var result = await repository.GetRatesByDateAsync(date, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            var currencyList = result.Currencies.ToList();
            RateInfoText = currencyList.Count > 0
                ? $"Курс на {result.ActualDate:d MMMM yyyy}"
                : "Нет данных за выбранную дату";

            var previousFromCode = FromCurrency?.CharCode;
            var previousToCode = ToCurrency?.CharCode;

            if (!_hasLoadedCurrencies)
            {
                previousFromCode = Preferences.Default.Get(PrefFromCurrency, "RUB");
                previousToCode = Preferences.Default.Get(PrefToCurrency, "USD");
            }

            var items = new List<CurrencyItem>(currencyList.Count + 1)
            {
                new()
                {
                    CharCode = "RUB",
                    Name = "Российский рубль",
                    Nominal = 1,
                    Value = 1
                }
            };

            items.AddRange(currencyList.Select(CurrencyItem.FromDomain));

            _isUpdating = true;
            try
            {
                Currencies = new ObservableCollection<CurrencyItem>(items);

                FromCurrency = Currencies.FirstOrDefault(c => c.CharCode == previousFromCode)
                               ?? Currencies.FirstOrDefault(c => c.CharCode == "RUB");

                ToCurrency = Currencies.FirstOrDefault(c => c.CharCode == previousToCode)
                             ?? Currencies.FirstOrDefault(c => c.CharCode == "USD");
            }
            finally
            {
                _isUpdating = false;
            }

            _hasLoadedCurrencies = true;
            ConvertFromSource();
        }
        catch (OperationCanceledException)
        {
            // Загрузка была отменена - игнорируем
        }
        catch (Exception)
        {
            RateInfoText = "Ошибка загрузки данных";
        }
        finally
        {
            IsLoading = false;

            if (_reloadPending)
            {
                _reloadPending = false;
                _ = LoadCurrenciesCommand.ExecuteAsync(null);
            }
        }
    }

    /// <summary>
    /// Загружает сохранённое состояние из Preferences
    /// </summary>
    private void RestoreState()
    {
        _isUpdating = true;
        try
        {
            MaxDate = DateTime.Today;

            var savedDateTicks = Preferences.Default.Get(PrefSelectedDate, DateTime.Today.Ticks);
            var savedDate = new DateTime(savedDateTicks);

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
        if (!_hasLoadedCurrencies)
            return;

        Preferences.Default.Set(PrefSelectedDate, SelectedDate.Ticks);
        Preferences.Default.Set(PrefFromCurrency, FromCurrency?.CharCode ?? "RUB");
        Preferences.Default.Set(PrefToCurrency, ToCurrency?.CharCode ?? "USD");
        Preferences.Default.Set(PrefFromAmount, string.IsNullOrWhiteSpace(FromAmount) ? "1" : FromAmount);
    }

    private void ConvertFromSource()
    {
        if (FromCurrency == null || ToCurrency == null)
            return;

        if (!TryParseAmount(FromAmount, out var amount))
            return;

        _isUpdating = true;

        try
        {
            var amountInRub = amount * FromCurrency.RatePerUnit;
            var result = amountInRub / ToCurrency.RatePerUnit;

            ToAmount = FormatAmount(result);
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

        if (!TryParseAmount(ToAmount, out var amount))
            return;

        _isUpdating = true;

        try
        {
            var amountInRub = amount * ToCurrency.RatePerUnit;
            var result = amountInRub / FromCurrency.RatePerUnit;

            FromAmount = FormatAmount(result);
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private static bool TryParseAmount(string? value, out decimal amount)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out amount);
    }

    private static string FormatAmount(decimal amount)
    {
        return Math.Round(amount, AmountDecimals).ToString(CultureInfo.CurrentCulture);
    }
}
