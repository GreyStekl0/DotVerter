using UI.Views;

namespace DotVerter;

public partial class App
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ServiceProvider = serviceProvider;
    }

    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var page = ServiceProvider.GetRequiredService<CurrencyConverterPage>();
        return new Window(new NavigationPage(page));
    }
}