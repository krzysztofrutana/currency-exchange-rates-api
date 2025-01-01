using System;
using CurrencyRates.Nbp.Queries;

namespace CurrencyRates.Nbp.UnitTests.Queries;

public class GetCurrencyRatesFromDatePeriodQueryValidatorTests
{
    [Fact]
    public void GetCurrencyRateForDateQuery_Code_Empty_ShouldReturnError()
    {
        var query = new GetCurrencyRatesFromDatePeriodQuery("", 0);
        var validator = new GetCurrencyRatesFromDatePeriodQuery.GetCurrencyRatesFromDatePeriodQueryValidator();
        var result = validator.Validate(query);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Code");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Code cannot be empty");
    }
    
    [Fact]
    public void GetCurrencyRateForDateQuery_Code_ToLong_ShouldReturnError()
    {
        var query = new GetCurrencyRatesFromDatePeriodQuery("A", 0);
        var validator = new GetCurrencyRatesFromDatePeriodQuery.GetCurrencyRatesFromDatePeriodQueryValidator();
        var result = validator.Validate(query);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Code");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Code must be at least 3 characters long");
    }
    
    [Fact]
    public void GetCurrencyRateForDateQuery_Code_ToShort_ShouldReturnError()
    {
        var query = new GetCurrencyRateForDateQuery("ABCD", new DateOnly(2024, 12, 30),true);
        var validator = new GetCurrencyRateForDateQuery.GetCurrencyRateQueryValidator();
        var result = validator.Validate(query);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Code");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Code must be no more than 3 characters long");
    }
    
    [Fact]
    public void GetCurrencyRateForDateQuery_Limit_Zero_ShouldReturnError()
    {
        var query = new GetCurrencyRatesFromDatePeriodQuery("USD", 0);
        var validator = new GetCurrencyRatesFromDatePeriodQuery.GetCurrencyRatesFromDatePeriodQueryValidator();
        var result = validator.Validate(query);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Limit");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Limit must be greater than 0");
    }
    
    [Fact]
    public void GetCurrencyRateForDateQuery_Limit_MoreThan100_ShouldReturnError()
    {
        var query = new GetCurrencyRatesFromDatePeriodQuery("USD", 101);
        var validator = new GetCurrencyRatesFromDatePeriodQuery.GetCurrencyRatesFromDatePeriodQueryValidator();
        var result = validator.Validate(query);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Limit");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Limit cannot be greater than 100");
    }
    
    [Fact]
    public void GetCurrencyRateForDateQuery_StartDateAndEndDate_ToSmall_ShouldReturnError()
    {
        var query = new GetCurrencyRatesFromDatePeriodQuery("USD", DateOnly.MinValue, DateOnly.MinValue);
        var validator = new GetCurrencyRatesFromDatePeriodQuery.GetCurrencyRatesFromDatePeriodQueryValidator();
        var result = validator.Validate(query);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "StartDate");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Date must be greater than minimum date value");
        
        Assert.Contains(result.Errors, x => x.PropertyName == "EndDate");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Date must be greater than minimum date value");
    }
    
    [Fact]
    public void GetCurrencyRateForDateQuery_StartDate_SmallerThan2002_1_2_ShouldReturnError()
    {
        var query = new GetCurrencyRatesFromDatePeriodQuery("USD", new DateOnly(2002, 1, 1), DateOnly.MinValue);
        var validator = new GetCurrencyRatesFromDatePeriodQuery.GetCurrencyRatesFromDatePeriodQueryValidator();
        var result = validator.Validate(query);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "StartDate");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Date must be greater than minimum date value");
    }
    
    [Fact]
    public void GetCurrencyRateForDateQuery_EndDate_GreaterThanToday_ShouldReturnError()
    {
        var query = new GetCurrencyRatesFromDatePeriodQuery("USD", DateOnly.MinValue, DateOnly.FromDateTime(DateTime.Now.AddDays(1)));
        var validator = new GetCurrencyRatesFromDatePeriodQuery.GetCurrencyRatesFromDatePeriodQueryValidator();
        var result = validator.Validate(query);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "EndDate");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Date cannot be greater than today date");
    }
    
    [Fact]
    public void GetCurrencyRateForDateQuery_StartDateAndEndDate_ToBigDifference_ShouldReturnError()
    {
        var query = new GetCurrencyRatesFromDatePeriodQuery("USD", DateOnly.FromDateTime(DateTime.Now.AddDays(-101)), DateOnly.FromDateTime(DateTime.Now));
        var validator = new GetCurrencyRatesFromDatePeriodQuery.GetCurrencyRatesFromDatePeriodQueryValidator();
        var result = validator.Validate(query);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Date difference cannot be greater than 100 days");
    }
    
    [Fact]
    public void GetCurrencyRateForDateQuery_WithLimit_ShouldPass()
    {
        var query = new GetCurrencyRatesFromDatePeriodQuery("USD", 90);
        var validator = new GetCurrencyRatesFromDatePeriodQuery.GetCurrencyRatesFromDatePeriodQueryValidator();
        var result = validator.Validate(query);
        
        Assert.True(result.IsValid);
    }
    
    [Fact]
    public void GetCurrencyRateForDateQuery_WithDates_ShouldPass()
    {
        var query = new GetCurrencyRatesFromDatePeriodQuery("USD", DateOnly.FromDateTime(DateTime.Now.AddDays(-90)), DateOnly.FromDateTime(DateTime.Now));
        var validator = new GetCurrencyRatesFromDatePeriodQuery.GetCurrencyRatesFromDatePeriodQueryValidator();
        var result = validator.Validate(query);
        
        Assert.True(result.IsValid);
    }
}