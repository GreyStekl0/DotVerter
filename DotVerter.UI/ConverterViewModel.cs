using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DotVerter.UI;

public partial class ConverterViewModel : ObservableObject
{
    public static DateTime Today => DateTime.Now.Date;
    public DateTime MinDate { get; } = new(1992, 7, 1);
    
    [ObservableProperty]
    private DateTime _selectedDate = Today;
}