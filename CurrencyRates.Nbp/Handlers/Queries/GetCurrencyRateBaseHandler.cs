using CurrencyExchangeRates.Data.Context;
using CurrencyExchangeRates.Data.Entities;
using CurrencyRates.Nbp.Extensions;
using CurrencyRates.Nbp.Helpers;
using CurrencyRates.Nbp.Models.AdditionalDatas;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRates.Nbp.Handlers.Queries;

public abstract class GetCurrencyRateBaseHandler
{
    private const int MaxDaysToCheck = 14;

    private readonly NbpHelper _nbpHelper;
    private readonly DatabaseContext _databaseContext;

    public GetCurrencyRateBaseHandler(NbpHelper nbpHelper, DatabaseContext databaseContext)
    {
        _nbpHelper = nbpHelper;
        _databaseContext = databaseContext;
    }
    
    protected async Task<Currency> GetCurrencyAsync(string currencyCode)
    {
        var databaseValue = await _databaseContext.Currencies
            .FirstOrDefaultAsync(x => x.Code == currencyCode);

        if (databaseValue is not null)
            return databaseValue;

        var nbpResult = await _nbpHelper.SearchCurrencyInNpbAsync(currencyCode);

        if (nbpResult is null)
            return null;

        var additionalData = new CurrencyAdditionalData()
        {
            NbpTables = [nbpResult.Table]
        };
        
        var currency = new Currency()
        {
            Code = currencyCode,
        };

        currency.SetAdditionalData(additionalData);

        var lastRate = nbpResult.Rates.First(x => x.Code == currencyCode);
        
        currency.CurrencyExchangeRates.Add(new CurrencyExchangeRate()
        {
            EffectiveDate = nbpResult.EffectiveDate,
            PurchaseRate = lastRate.PurchaseRate,
            SaleRate = lastRate.SaleRate,
            AvarageRate = lastRate.AvarageRate,
            ForDate = DateOnly.FromDateTime(DateTime.Now)
        });

        _databaseContext.Add(currency);
        await _databaseContext.SaveChangesAsync();

        return currency;
    }
    
    protected async Task<CurrencyExchangeRate> TryGetForDateAsync(Currency currency, DateOnly date)
    {
        var result =  await _databaseContext.CurrencyExchangeRates
            .Where(x => x.CurrencyId == currency.Id && x.ForDate == date)
            .FirstOrDefaultAsync();

        if (result is null)
        {
            result = await TryGetRateFromNbp(currency, date);
        }

        return result;
    }

    protected async Task<CurrencyExchangeRate> TryGetRateFromNbp(Currency currency, DateOnly forDate)
    {
        var nbpRateResult = await _nbpHelper.GetRateForCurrencyAsync(currency, forDate);
        if (nbpRateResult is not null)
        {
            var newRate = new CurrencyExchangeRate();
            newRate.Currency = currency;
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

    private async Task UpdateDaysWithoutRateAsync(Currency currency, List<DateOnly> emptyDays, CurrencyExchangeRate newestAvailableRate)
    {
        foreach (var dayWithoutRate in emptyDays)
        {
            var newRate = new CurrencyExchangeRate();
            newRate.Currency = currency;
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

    private async Task<bool> EnsureIfRateNotExist(Currency currency, DateOnly dayWithoutRate)
    {
        return await _databaseContext.CurrencyExchangeRates
            .AnyAsync(s => s.CurrencyId == currency.Id && s.ForDate == dayWithoutRate);
    }
}