using CurrencyExchangeRates.Data.Context;
using CurrencyExchangeRates.Data.Entities;
using CurrencyRates.Common.Cache;
using CurrencyRates.Nbp.Extensions;
using CurrencyRates.Nbp.Helpers;
using CurrencyRates.Nbp.Models.AdditionalDatas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyRates.Nbp.Handlers.Queries;

public abstract class GetCurrencyRateBaseHandler
{
    private const int MaxDaysToCheck = 14;

    private readonly NbpHelper _nbpHelper;
    private readonly DatabaseContext _databaseContext;
    private readonly IMemoryCache _memoryCache;

    public GetCurrencyRateBaseHandler(NbpHelper nbpHelper, DatabaseContext databaseContext, IMemoryCache memoryCache)
    {
        _nbpHelper = nbpHelper;
        _databaseContext = databaseContext;
        _memoryCache = memoryCache;
    }

    protected async Task<Currency> TryGetCurrencyFromCache(string currencyCode)
    {
        return await _memoryCache.GetOrCreateAsync(CacheHelper.GetCurrencyKey(currencyCode), async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            
            return await GetCurrencyAsync(currencyCode);
        });
    }

    /// <summary>
    /// Pobiera walutę z bazy, jeśli jej nie ma to szuka ją w NBP wraz z aktualnym kursem
    /// </summary>
    /// <param name="currencyCode">Kod waluty</param>
    /// <returns>Zapisany model bazodanowy waluty</returns>
    private async Task<Currency> GetCurrencyAsync(string currencyCode)
    {
        var databaseValue = await _databaseContext.Currencies
            .FirstOrDefaultAsync(x => x.Code == currencyCode);

        if (databaseValue is not null)
            return databaseValue;

        var nbpResult = await _nbpHelper.SearchCurrencyInNpbAsync(currencyCode);

        if (nbpResult is null)
            return null;

        var checkIfIsInTableC = await _nbpHelper.SearchCurrencyInNbpTableCAsync(currencyCode);

        // Zapisujemy informację w jakiej tabeli występuje waluta
        var additionalData = new CurrencyAdditionalData()
        {
            NbpTables = [nbpResult.Table]
        };
        
        if(checkIfIsInTableC is not null)
            additionalData.NbpTables.Add(checkIfIsInTableC.Table);
        
        var currency = new Currency()
        {
            Code = currencyCode,
        };

        currency.SetAdditionalData(additionalData);

        var lastRate = nbpResult.Rates.First(x => x.Code == currencyCode);
        var lastCRate = checkIfIsInTableC?.Rates.First(x => x.Code == currencyCode);
        
        // Dodajemy aktualny kurs
        currency.CurrencyExchangeRates.Add(new CurrencyExchangeRate()
        {
            EffectiveDate = nbpResult.EffectiveDate,
            PurchaseRate = lastCRate?.PurchaseRate,
            SaleRate = lastCRate?.SaleRate,
            AvarageRate = lastRate.AvarageRate,
            ForDate = DateOnly.FromDateTime(DateTime.Now),
            Source = NbpConstants.SourceName
        });

        _databaseContext.Add(currency);
        await _databaseContext.SaveChangesAsync();

        return currency;
    }
    
    /// <summary>
    /// Probuje pobrać kurs dla podanej daty z bazy, a jeśli brak to z NBP
    /// </summary>
    /// <param name="currency">Waluta</param>
    /// <param name="date">Data kursu</param>
    /// <returns>Kurs dla podanej daty, null nie znaleziono</returns>
    protected async Task<CurrencyExchangeRate> TryGetForDateAsync(Currency currency, DateOnly date)
    {
        var result = await TryGetRateForDateFromDatabase(currency, date);

        if (result is null)
            result = await TryGetRateFromNbp(currency, date);

        if (result is null)
            return null;

        if (RateShouldBeUpdated(currency, result))
            await UpdateRateFromNbpAsync(currency, result);

        return result;
    }

    private static bool RateShouldBeUpdated(Currency currency, CurrencyExchangeRate result)
    {
        return result.PurchaseRate is null
               && result.SaleRate is null
               && currency.GetAdditionalData() is not null
               && currency.GetAdditionalData().NbpTables.Contains("c", StringComparer.InvariantCultureIgnoreCase);
    }

    private async Task<CurrencyExchangeRate> TryGetRateForDateFromDatabase(Currency currency, DateOnly date)
    {
        return await _memoryCache.GetOrCreateAsync(CacheHelper.GetKeyForDatabaseRate(currency.Code, date), async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            
            return await _databaseContext.CurrencyExchangeRates
                .Where(x => x.CurrencyId == currency.Id && x.ForDate == date)
                .FirstOrDefaultAsync();
        });
    }

    /// <summary>
    /// Próboje pobrać kurs z NBP dla podanej waluty i daty
    /// </summary>
    /// <param name="currency">Waluta</param>
    /// <param name="forDate">Data</param>
    /// <returns>Model bazodanowy kursu lub null</returns>
    private async Task<CurrencyExchangeRate> TryGetRateFromNbp(Currency currency, DateOnly forDate)
    {
        var nbpRateResult = await _nbpHelper.GetRateForCurrencyAsync(currency, forDate);
        if (nbpRateResult is not null)
        {
            var newRate = new CurrencyExchangeRate();
            newRate.CurrencyId = currency.Id;
            newRate.AvarageRate = nbpRateResult.AvarageRate;
            newRate.PurchaseRate = nbpRateResult.PurchaseRate;
            newRate.SaleRate = nbpRateResult.SaleRate;
            newRate.EffectiveDate = nbpRateResult.EffectiveDate;
            newRate.Source = NbpConstants.SourceName;
            newRate.ForDate = forDate;

            _databaseContext.Add(newRate);
            await _databaseContext.SaveChangesAsync();
            
            return newRate;
        }

        return null;
    }
    
    private async Task<CurrencyExchangeRate> UpdateRateFromNbpAsync(Currency currency, CurrencyExchangeRate rate)
    {
        var nbpRateResult = await _nbpHelper.GetRateForCurrencyAsync(currency, rate.ForDate);
        if (nbpRateResult is not null)
        {
            rate.SaleRate = nbpRateResult.SaleRate;
            rate.AvarageRate = nbpRateResult.AvarageRate;
            rate.PurchaseRate = nbpRateResult.PurchaseRate;

            _databaseContext.Update(rate);
            await _databaseContext.SaveChangesAsync();
        }

        return rate;
    }
    
    /// <summary>
    /// Jeśli kursu na wskazaną datę nie ma to szukamy wstecz pierwszego dostępnego kursu. W przypadku np. weekendów i soboty
    /// (dla niej będzie brak) najbliższym kursem będzie ten z piątku (ostatni poprzedzający dzień roboczy).
    /// Dla pewności szukamy 14 dni wstecz (raczej nad wyraz) do momentu pojawienia się kursu.
    /// Dla dni bez kursu w bazie uzupełniamy je wartością znalezionego kursu
    /// </summary>
    /// <param name="currency">Waluta</param>
    /// <param name="startDate">Data początkowa</param>
    /// <returns>Ostatni (czyli aktualny) obowiązujący kurs</returns>
    protected async Task<CurrencyExchangeRate> TryGetLastAvailableFromDateAsync(Currency currency, DateOnly startDate)
    {
        var emptyDays = new List<DateOnly>();
        
        for (int i = 1; i <= MaxDaysToCheck; i++)
        {
            var date = startDate.AddDays(-i);
            
            var result = await TryGetForDateAsync(currency, date);
            if (result is not null)
            {
                await UpdateDaysWithoutRateAsync(currency, emptyDays, result);

                return result;
            }
            
            emptyDays.Add(date);
        }

        return null;
    }

    /// <summary>
    /// Aktualizacja dni bez kursu ostatnim znalezionym kursem (by nie odpytywać NBP z każdym requestem)
    /// </summary>
    /// <param name="currency">Waluta</param>
    /// <param name="emptyDays">Dni bez kursu</param>
    /// <param name="newestAvailableRate">Ostani znaleziony kurs</param>
    private async Task UpdateDaysWithoutRateAsync(Currency currency, List<DateOnly> emptyDays, CurrencyExchangeRate newestAvailableRate)
    {
        foreach (var dayWithoutRate in emptyDays)
        {
            var newRate = new CurrencyExchangeRate();
            newRate.CurrencyId = currency.Id;
            newRate.AvarageRate = newestAvailableRate.AvarageRate;
            newRate.PurchaseRate = newestAvailableRate.PurchaseRate;
            newRate.SaleRate = newestAvailableRate.SaleRate;
            newRate.EffectiveDate = newestAvailableRate.EffectiveDate;
            newRate.Source = NbpConstants.SourceName;
            newRate.ForDate = dayWithoutRate;

            if(await EnsureIfRateNotExist(currency, dayWithoutRate))
                _databaseContext.Add(newRate);
        }

        await _databaseContext.SaveChangesAsync();
    }

    /// <summary>
    /// Upewniamy się czy kursu na pewno nie ma (bo np. dodał go job w tle)
    /// </summary>
    /// <param name="currency">Waluta</param>
    /// <param name="dayWithoutRate">Data kursu</param>
    /// <returns>True jeśli znaleziono, False jeśli nie</returns>
    private async Task<bool> EnsureIfRateNotExist(Currency currency, DateOnly dayWithoutRate)
    {
        return !await _databaseContext.CurrencyExchangeRates
            .AnyAsync(s => s.CurrencyId == currency.Id && s.ForDate == dayWithoutRate);
    }
}