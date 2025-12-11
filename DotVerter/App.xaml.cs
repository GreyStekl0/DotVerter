using UI.Views;

namespace DotVerter;

public partial class App
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var page = _serviceProvider.GetRequiredService<CurrencyConverterPage>();
        return new Window(new NavigationPage(page));
    }
}