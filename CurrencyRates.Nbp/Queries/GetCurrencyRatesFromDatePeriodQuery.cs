using System.Data;
using CurrencyRates.Common.Models;
using CurrencyRates.Nbp.Models.Responses;
using FluentValidation;
using MediatR;

namespace CurrencyRates.Nbp.Queries;

public class GetCurrencyRatesFromDatePeriodQuery : IRequest<Result<IEnumerable<CurrencyRateResponse>>>
{
    public GetCurrencyRatesFromDatePeriodQuery(string code, int limit)
    {
        Limit = limit;
        Code = code.ToUpper();

        var dates = new List<DateOnly>();
        for (int i = 0; i < limit; i++)
        {
            dates.Add(DateOnly.FromDateTime(DateTime.Now.AddDays(-i)));
        }

        Dates = dates;
    }
    
    public GetCurrencyRatesFromDatePeriodQuery(string code, DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
        Code = code.ToUpper();
        
        var dates = new List<DateOnly>();
        dates.Add(endDate);

        var tempEndDate = endDate;
        while (tempEndDate > startDate)
        {
            tempEndDate = tempEndDate.AddDays(-1);
            dates.Add(tempEndDate);
        }

        Dates = dates;
    }
    
    public string Code { get; }
    public DateOnly? StartDate { get; }
    public DateOnly? EndDate { get; }
    public int? Limit { get; }
    
    public IEnumerable<DateOnly> Dates { get; } 

    public class GetCurrencyRatesFromDatePeriodQueryValidator : AbstractValidator<GetCurrencyRatesFromDatePeriodQuery>
    {
        public GetCurrencyRatesFromDatePeriodQueryValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code cannot be empty")
                .MinimumLength(3).WithMessage("Code must be at least 3 characters long")
                .MaximumLength(3).WithMessage("Code must be no more than 3 characters long");

            When(x => x.Limit.HasValue, () =>
            {
                RuleFor(x => x.Limit)
                    .GreaterThan(0).WithMessage("Limit must be greater than 0")
                    .LessThan(101).WithMessage("Limit cannot be greater than 100");
            });

            When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
            {
                RuleFor(x => x.StartDate)
                    .GreaterThan(DateOnly.MinValue).WithMessage("Date must be greater than minimum date value")
                    .GreaterThan(new DateOnly(2002, 1, 1)).WithMessage("Archival rate are available from 2002-01-02");
                
                RuleFor(x => x.EndDate)
                    .GreaterThan(DateOnly.MinValue).WithMessage("Date must be greater than minimum date value")
                    .LessThan(DateOnly.FromDateTime(DateTime.Now.AddDays(1))).WithMessage("Date cannot be greater than today date");
                
                RuleFor(x => new { StartDate = x.StartDate.Value, EndDate = x.EndDate.Value })
                    .Must(arg =>
                    {
                        var difference = arg.EndDate.ToDateTime(TimeOnly.MinValue) - arg.StartDate.ToDateTime(TimeOnly.MinValue);
                        return difference.Days <= 100;
                    }).WithMessage("Date difference cannot be greater than 100 days");
            });
        }
    }
}