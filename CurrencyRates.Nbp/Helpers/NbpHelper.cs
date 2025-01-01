using CurrencyExchangeRates.Data.Entities;
using CurrencyRates.Nbp.Extensions;
using CurrencyRates.Nbp.HttpClient;
using CurrencyRates.Nbp.Models.Rates;
using CurrencyRates.Nbp.Models.Tables;
using Refit;

namespace CurrencyRates.Nbp.Helpers;

public class NbpHelper
{
    private readonly INbpTablesApiClient _nbpTablesApiClient;
    private readonly INbpRatesApiClient _nbpRatesApiClient;

    public NbpHelper(
        INbpTablesApiClient nbpTablesApiClient,
        INbpRatesApiClient nbpRatesApiClient)
    {
        _nbpTablesApiClient = nbpTablesApiClient;
        _nbpRatesApiClient = nbpRatesApiClient;
    }
    
    /// <summary>
    /// Szuka waluty o podanym kodzie w tabelach typu A i B
    /// </summary>
    /// <param name="currencyCode">Kod waluty</param>
    /// <returns>Tabelę dla podanej waluty, null jeśli nie znaleziono</returns>
    public async Task<NbpTable> SearchCurrencyInNpbAsync(string currencyCode)
    {
        var tableAResult = await _nbpTablesApiClient.GetTableAsync("a");
        if(tableAResult.Any(s => s.Rates.Any(z => z.Code == currencyCode)))
            return tableAResult.First(s => s.Rates.Any(z => z.Code == currencyCode));
        
        var tableBResult = await _nbpTablesApiClient.GetTableAsync("b");
        if(tableBResult.Any(s => s.Rates.Any(z => z.Code == currencyCode)))
            return tableBResult.First(s => s.Rates.Any(z => z.Code == currencyCode));

        return null;
    }
    
    /// <summary>
    /// Szukamy waluty w tabeli C
    /// </summary>
    /// <param name="currencyCode"></param>
    /// <returns></returns>
    public async Task<NbpTable> SearchCurrencyInNbpTableCAsync(string currencyCode)
    {
        var tableCResult = await _nbpTablesApiClient.GetTableAsync("c");
        if(tableCResult.Any(s => s.Rates.Any(z => z.Code == currencyCode)))
            return tableCResult.First(s => s.Rates.Any(z => z.Code == currencyCode));
        
        return null;
    }

    /// <summary>
    /// Sprawdza kurs dla waluty i podanego dnia
    /// </summary>
    /// <param name="currency">Waluta</param>
    /// <param name="date">Data</param>
    /// <returns>Kurs NBP lub null jeśli brak kursu na podany dzień</returns>
    public async Task<NbpCurrencyRate> GetRateForCurrencyAsync(Currency currency, DateOnly date)
    {
        var currencyAdditionalData = currency.GetAdditionalData();

        NbpCurrencyRate rate = null;
        foreach (var tablesType in currencyAdditionalData.NbpTables)
        {
            try
            {
                var tableResult = await _nbpRatesApiClient.GetCurrencyRateAsync(tablesType, currency.Code, date);

                if(rate is null)
                    rate = tableResult.Rates.First();
                else
                {
                    var rateResult = tableResult.Rates.First();
                    
                    rate.AvarageRate ??= rateResult.AvarageRate;
                    rate.PurchaseRate ??= rateResult.PurchaseRate;
                    rate.SaleRate ??= rateResult.SaleRate;
                }
            }
            catch
            {
                // ignored
            }
        }

        return rate;
    }
}