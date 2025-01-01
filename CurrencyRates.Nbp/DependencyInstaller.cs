using CurrencyRates.Nbp.Helpers;
using CurrencyRates.Nbp.HttpClient;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace CurrencyRates.Nbp;

public static class DependencyInstaller
{
    public static void RegisterNbpCurrencyExchangeRates(this IServiceCollection serviceCollection)
    {
        serviceCollection.RegisterRefit();

        serviceCollection.AddScoped<NbpHelper>();
    }

    private static void RegisterRefit(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddRefitClient<INbpTablesApiClient>(new RefitSettings()
        {
            UrlParameterFormatter = new CustomDateUrlParameterFormatter(),
        })
        .ConfigureHttpClient(cfg =>
        {
            cfg.BaseAddress = new Uri("https://api.nbp.pl/api/exchangerates/tables");
            cfg.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        
        serviceCollection.AddRefitClient<INbpRatesApiClient>(new RefitSettings()
            {
                UrlParameterFormatter = new CustomDateUrlParameterFormatter()
            })
            .ConfigureHttpClient(cfg =>
            {
                cfg.BaseAddress = new Uri("https://api.nbp.pl/api/exchangerates/rates");
                cfg.DefaultRequestHeaders.Add("Accept", "application/json");
            });
    }
}