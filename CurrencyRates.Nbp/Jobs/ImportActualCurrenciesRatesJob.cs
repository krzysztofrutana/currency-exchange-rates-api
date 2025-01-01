using System.ComponentModel;
using CurrencyExchangeRates.Data.Context;
using CurrencyExchangeRates.Data.Entities;
using CurrencyRates.Common.Hangfire;
using CurrencyRates.Nbp.Extensions;
using CurrencyRates.Nbp.Helpers;
using CurrencyRates.Nbp.HttpClient;
using CurrencyRates.Nbp.Models.AdditionalDatas;
using CurrencyRates.Nbp.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CurrencyRates.Nbp.Jobs;

[Description("Pobieranie najnowszych kursów walut na dany moment")]
public class ImportActualCurrenciesRatesJob : IScheduleJob
{
    private readonly INbpTablesApiClient _nbpTablesApiClient;
    private readonly DatabaseContext _databaseContext;
    private readonly ILogger<ImportActualCurrenciesRatesJob> _logger;

    public ImportActualCurrenciesRatesJob(
        INbpTablesApiClient nbpTablesApiClient,
        DatabaseContext databaseContext,
        ILogger<ImportActualCurrenciesRatesJob> logger)
    {
        _nbpTablesApiClient = nbpTablesApiClient;
        _databaseContext = databaseContext;
        _logger = logger;
    }

    private DateOnly _today => DateOnly.FromDateTime(DateTime.Now);
    public string CronExpression => "*/15 * * * *";

    private List<Currency> _existCurrencies;
    
    public async Task Execute()
    {
        try
        {
            _logger.LogInformation("Importing currencies rates");
            
            string[] tablesToProcess = ["a", "b", "c"];

            await LoadExistingCurrenciesAsync();
        
            foreach (var table in tablesToProcess)
            {
                await ProcessTableAsync(table);
            }

            await _databaseContext.SaveChangesAsync();
            
            _logger.LogInformation("Imported currencies rates");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    private async Task LoadExistingCurrenciesAsync()
    {
        _existCurrencies = await _databaseContext.Currencies
            .AsQueryable()
            .ToListAsync();
    }

    private async Task ProcessTableAsync(string tableType)
    {
        var table = await _nbpTablesApiClient.GetTableAsync(tableType);
        
        if(!table.Any())
            return;
        
        var tableResult = table.First();

        foreach (var currencyRate in tableResult.Rates)
        {
            await ProcessCurrencyRateAsync(tableResult, currencyRate);
        }
    }

    private async Task ProcessCurrencyRateAsync(NbpTable table, NbpTableRate currencyRate)
    {
        var currency = _existCurrencies
            .FirstOrDefault(x => x.Code == currencyRate.Code);

        if (currency is null)
        {
            currency = new Currency()
            {
                Code = currencyRate.Code,
            };
            
            // zapisujemy informację w jakiej tabeli dana waluta ma kurs
            currency.SetAdditionalData(new CurrencyAdditionalData()
            {
                NbpTables = [table.Table]
            });

            // waluta w tabelach może się powtarzać (patrz Tabela typu A i C, jedna zawiera uśredniony kurs, druga kurs kupna/sprzedaży)
            // stad przy następnej tabeli musimy wiedzieć, że już dodaliśmy taką walutę
            _existCurrencies.Add(currency);
            _databaseContext.Add(currency);
        }
        else
        {
            if (!currency.GetAdditionalData().NbpTables.Contains(table.Table))
            {
                var actualAdditionalData = currency.GetAdditionalData();
                actualAdditionalData.NbpTables.Add(table.Table);
                currency.SetAdditionalData(actualAdditionalData);
            }
        }

        CurrencyExchangeRate rate;
        // Waluta już była w bazie, szukamy więc kursu po Id waluty i dacie publikacji
        if (currency.Id > 0)
        {
            rate = await _databaseContext.CurrencyExchangeRates
                .FirstOrDefaultAsync(x => x.CurrencyId == currency.Id && x.ForDate == _today);
        }
        else
        {
            // dla pewności sprawdzamy, czy dla nowej waluty nie dodaliśmy już kursu dla tej daty
            rate = currency.CurrencyExchangeRates.FirstOrDefault(x => x.ForDate == _today);
        }
        
        // jeśli kurs istnieje to aktualizujemy wartości
        // jeśli kurs pochodzi z bazy będzie zaatachowany i się zaktualizuje
        // jeśli to nowy kurs np. z tabeli A i jesteśmy w tabeli C to po prostu dopiszą się tam wartości a końcowo wpis doda się do bazy
        if (rate is not null)
        {
            rate.PurchaseRate ??= currencyRate.PurchaseRate;
            rate.SaleRate ??= currencyRate.SaleRate;
            rate.AvarageRate ??= currencyRate.AvarageRate;
            rate.EffectiveDate = table.EffectiveDate;
                
            return;
        }

        rate = new CurrencyExchangeRate();
        rate.Currency = currency;
        rate.AvarageRate = currencyRate.AvarageRate;
        rate.PurchaseRate = currencyRate.PurchaseRate;
        rate.SaleRate = currencyRate.SaleRate;
        rate.EffectiveDate = table.EffectiveDate;
        rate.Source = NbpConstants.SourceName;
        rate.ForDate = _today;
            
        // waluta jest zaatachowana, dodanie do kolekcji spowoduje dodanie wpisu do bazy
        currency.CurrencyExchangeRates.Add(rate);
    }
}