using UI.ViewModels;

namespace UI.Views;

public partial class CurrencyConverterPage
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
            await vm.InitializeCommand.ExecuteAsync(null);
    }
}
