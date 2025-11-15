using System.IO;
using DotVerter.Data.Local;
using DotVerter.Data.Remote;
using DotVerter.Data.Remote.Cbr;
using DotVerter.Data.Repositories;
using DotVerter.Domain.Interface;
using DotVerter.UI;
using MauiIcons.Material;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using SQLite;

namespace DotVerter;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMaterialMauiIcons()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Регистрация подключения к SQLite
        builder.Services.AddSingleton(_ =>
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "dotverter.db3");
            var flags = SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache;
            return new SQLiteAsyncConnection(databasePath, flags);
        });

        builder.Services.AddSingleton<IExchangeRateLocalStore, ExchangeRateLocalStore>();

        // Регистрация HttpClient для CbrClient
        builder.Services.AddHttpClient<IClient, CbrClient>(client =>
        {
            client.BaseAddress = new Uri("https://www.cbr-xml-daily.ru/");
        });

        // Регистрация репозитория
        builder.Services.AddSingleton<IExchangeRateRepository, ExchangeRateRepository>();

        // Регистрация ViewModel
        builder.Services.AddTransient<ConverterViewModel>();

        // Регистрация страницы
        builder.Services.AddTransient<Converter>();

        return builder.Build();
    }
}
