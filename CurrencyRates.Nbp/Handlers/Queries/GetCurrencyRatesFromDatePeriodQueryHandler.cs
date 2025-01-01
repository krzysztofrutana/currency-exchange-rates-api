using CurrencyExchangeRates.Data.Context;
using CurrencyExchangeRates.Data.Entities;
using CurrencyRates.Common.Models;
using CurrencyRates.Nbp.Helpers;
using CurrencyRates.Nbp.Models.Responses;
using CurrencyRates.Nbp.Queries;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyRates.Nbp.Handlers.Queries;

internal class GetCurrencyRatesFromDatePeriodQueryHandler : 
    GetCurrencyRateBaseHandler, 
    IRequestHandler<GetCurrencyRatesFromDatePeriodQuery, Result<IEnumerable<CurrencyRateResponse>>>
{
    public GetCurrencyRatesFromDatePeriodQueryHandler(
        DatabaseContext databaseContext,
        NbpHelper nbpHelper,
        IMemoryCache memoryCache) : base(nbpHelper, databaseContext, memoryCache)
    {
    }
    
    public async Task<Result<IEnumerable<CurrencyRateResponse>>> Handle(GetCurrencyRatesFromDatePeriodQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetCurrencyRatesFromDatePeriodQuery.GetCurrencyRatesFromDatePeriodQueryValidator();
        
        var validationResults = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResults.IsValid)
        {
            var errors = validationResults
                .Errors.Select(x => x.ErrorMessage);

            return Result<IEnumerable<CurrencyRateResponse>>.Failure(
                string.Join(", ", errors));
        }

        var currency = await TryGetCurrencyFromCache(request.Code);
        if(currency is null)
            return Result<IEnumerable<CurrencyRateResponse>>.Failure("Currency with this code does not exist");

        var result = new List<CurrencyRateResponse>();

        CurrencyExchangeRate lastAdded = null;
        foreach (var date in request.Dates.OrderBy(x => x))
        {
            if(date > DateOnly.FromDateTime(DateTime.Now))
                return Result<IEnumerable<CurrencyRateResponse>>.Failure("Date is greater than the current date");
            
            var databaseValue = await TryGetForDateAsync(currency, date);

            // nie znaleziono rezultaltu na dany dzień i jest to pierwsza data
            if (databaseValue is null && lastAdded is null)
            {
                // próbujemy szukać kursu wstecz
                databaseValue = await TryGetLastAvailableFromDateAsync(currency, date);
                if (databaseValue is null)
                {
                    result.Add(new CurrencyRateResponse(date, null));
                    continue;
                }

                lastAdded = databaseValue;
                result.Add(new CurrencyRateResponse(date, databaseValue));
            }
            // jeśli na dany dzień nie ma kursu, ale mamy kurs z dnia poprzedniego (czyli jest np. sobota)
            else if(databaseValue is null)
                result.Add(new CurrencyRateResponse(date, lastAdded));
            // Mamy kurs na dany dzień, zapamiętujemy ostatnio dodany kurs
            else
            {
                lastAdded = databaseValue;
                result.Add(new CurrencyRateResponse(date, databaseValue));
            }
        }

        return Result<IEnumerable<CurrencyRateResponse>>.Success(result);
    }
}