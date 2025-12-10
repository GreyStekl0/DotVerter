using Data.Repositories;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using UI;

namespace DotVerter;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Регистрация сервисов
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "currencies.db");
        builder.Services.AddSingleton<ICurrencyRepository>(new CurrencyRepository(dbPath));

        // UI сервисы
        builder.Services.AddUIServices();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}