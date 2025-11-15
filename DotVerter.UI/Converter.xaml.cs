namespace DotVerter.UI;

public partial class Converter : ContentPage
{
    public Converter(ConverterViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}