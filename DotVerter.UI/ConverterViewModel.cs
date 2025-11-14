using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using DotVerter.Domain.Interface;

namespace DotVerter.UI;

public partial class ConverterViewModel(IExchangeRateRepository exchangeRateRepository) : ObservableObject
{
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;

    public static DateTime Today => DateTime.Now.Date;
    public DateTime MinDate { get; } = new(1992, 7, 1);

    [ObservableProperty] private DateTime _selectedDate = Today;
    
    
}