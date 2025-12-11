using Data;
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

        // Путь к базе данных
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "currencies.db");

        // Регистрация сервисов Data слоя (включая HttpClient и репозиторий)
        builder.Services.AddDataServices(dbPath);

        // Регистрация сервисов UI слоя
        builder.Services.AddUIServices();

        return builder.Build();
    }
}