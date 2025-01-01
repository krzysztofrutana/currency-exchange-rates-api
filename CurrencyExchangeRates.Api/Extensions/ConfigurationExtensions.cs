using System.IO;
using dotenv.net;
using Microsoft.Extensions.Configuration;

namespace CurrencyExchangeRates.Api.Extensions;

public static class ConfigurationExtensions
{
    public static void AddConfiguration
        (this IConfigurationBuilder builder)
    {
        var basePath = Path.GetDirectoryName(typeof(ConfigurationExtensions).Assembly.Location)!;
        
        // Ładowanie zmiennych środowiskowych z lokalnego pliku .env
        DotEnv.Load(new DotEnvOptions(
            trimValues:true,
            envFilePaths:[".env", Path.Combine(basePath, ".env")]));
        
        // Czyszczenie aktualnych źródeł
        builder.Sources.Clear();
        
        // Ładowanie konfiguracji tylko z appsettings i zmiennych środowiskowych załadowanych wyżej
        builder.SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
    }
}