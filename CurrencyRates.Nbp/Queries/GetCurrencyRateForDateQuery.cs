using CurrencyRates.Common.Models;
using CurrencyRates.Nbp.Models.Responses;
using FluentValidation;
using MediatR;

namespace CurrencyRates.Nbp.Queries;

public class GetCurrencyRateForDateQuery : IRequest<Result<CurrencyRateResponse>>
{
    public GetCurrencyRateForDateQuery(string code, DateOnly date, bool getLastAvailableIfCurrentNotExist)
    {
        Code = code.ToUpper();
        Date = date;
        GetLastAvailableIfCurrentNotExist = getLastAvailableIfCurrentNotExist;
    }
    
    public string Code { get; }
    public DateOnly Date { get; }
    public bool GetLastAvailableIfCurrentNotExist { get; }

    public class GetCurrencyRateQueryValidator : AbstractValidator<GetCurrencyRateForDateQuery>
    {
        public GetCurrencyRateQueryValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code cannot be empty")
                .MinimumLength(3).WithMessage("Code must be at least 3 characters long")
                .MaximumLength(3).WithMessage("Code must be no more than 3 characters long");

            RuleFor(x => x.Date)
                .GreaterThan(DateOnly.MinValue).WithMessage("Date must be greater than minimum date value")
                .GreaterThan(new DateOnly(2002, 1, 1)).WithMessage("Archival rate are available from 2002-01-02");
        }
    }
}