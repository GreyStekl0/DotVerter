using UI.ViewModels;

namespace UI.Views;

public partial class CurrencyConverterPage : ContentPage
{
    public CurrencyConverterPage(CurrencyConverterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CurrencyConverterViewModel vm)
        {
            vm.RestoreState();
            await vm.LoadCurrenciesCommand.ExecuteAsync(null);
        }
    }
}
