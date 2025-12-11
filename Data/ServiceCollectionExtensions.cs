using Data.Database;
using Data.Repositories;
using Data.Services;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Data;

/// <summary>
///     Расширения для регистрации сервисов Data слоя
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Добавляет все сервисы Data слоя в DI контейнер
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="dbPath">Путь к базе данных SQLite</param>
    public static IServiceCollection AddDataServices(this IServiceCollection services, string dbPath)
    {
        // База данных - singleton для переиспользования соединения
        services.AddSingleton(_ => new CurrencyDbContext(dbPath));

        // HTTP клиент через IHttpClientFactory
        services.AddHttpClient<ICbrApiService, CbrApiService>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "DotVerter/1.0");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Репозиторий
        services.AddSingleton<ICurrencyRepository, CurrencyRepository>();

        return services;
    }
}
