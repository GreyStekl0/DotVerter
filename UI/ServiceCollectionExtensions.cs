using UI.ViewModels;
using UI.Views;

namespace UI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUIServices(this IServiceCollection services)
    {
        // ViewModels
        services.AddTransient<CurrencyConverterViewModel>();

        // Views
        services.AddTransient<CurrencyConverterPage>();

        return services;
    }
}
