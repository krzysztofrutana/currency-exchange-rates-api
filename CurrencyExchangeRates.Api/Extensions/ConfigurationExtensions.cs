using System.IO;
using dotenv.net;
using Microsoft.Extensions.Configuration;

namespace CurrencyExchangeRates.Api.Extensions;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddConfiguration
        (this IConfigurationBuilder builder)
    {
        var basePath = Path.GetDirectoryName(typeof(ConfigurationExtensions).Assembly.Location)!;
        
        DotEnv.Load(new DotEnvOptions(
            trimValues:true,
            envFilePaths:[".env", Path.Combine(basePath, ".env")]));
        
        builder.Sources.Clear();
        
        builder.SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        return builder;
    }
}