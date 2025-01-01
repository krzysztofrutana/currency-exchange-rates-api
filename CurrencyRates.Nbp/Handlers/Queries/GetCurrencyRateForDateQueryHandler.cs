using CurrencyExchangeRates.Data.Context;
using CurrencyExchangeRates.Data.Entities;
using CurrencyRates.Common.Models;
using CurrencyRates.Nbp.Extensions;
using CurrencyRates.Nbp.Helpers;
using CurrencyRates.Nbp.Models.AdditionalDatas;
using CurrencyRates.Nbp.Models.Responses;
using CurrencyRates.Nbp.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRates.Nbp.Handlers.Queries;

internal class GetCurrencyRateForDateQueryHandler : GetCurrencyRateBaseHandler, IRequestHandler<GetCurrencyRateForDateQuery, Result<CurrencyRateResponse>>
{
    public GetCurrencyRateForDateQueryHandler(
        DatabaseContext databaseContext,
        NbpHelper nbpHelper) : base(nbpHelper, databaseContext)
    {
    }
    
    public async Task<Result<CurrencyRateResponse>> Handle(GetCurrencyRateForDateQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetCurrencyRateForDateQuery.GetCurrencyRateQueryValidator();
        
        var validationResults = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResults.IsValid)
        {
            var errors = validationResults
                .Errors.Select(x => x.ErrorMessage);

            return Result<CurrencyRateResponse>.Failure(
                string.Join(", ", errors));
        }

        var currency = await GetCurrencyAsync(request.Code);
        if(currency is null)
            return Result<CurrencyRateResponse>.Failure("Currency with this code does not exist");

        // Próba pobrania aktualnego kursu 
        var databaseValue = await TryGetForDateAsync(currency, request.Date);
        if (databaseValue is not null)
            return Result<CurrencyRateResponse>.Success(new CurrencyRateResponse(request.Date, databaseValue));

        if (request.GetLastAvailableIfCurrentNotExist)
        {
            // Próba pobrania pierwszego kursu z dni poprzedzających
            databaseValue = await TryGetLastAvailableFromDateAsync(currency, request.Date);
        }
        
        if (databaseValue is null)
            return Result<CurrencyRateResponse>.Failure("Cannot find any available rate for currency");
        
        return Result<CurrencyRateResponse>.Success(new CurrencyRateResponse(request.Date, databaseValue));
    }
}