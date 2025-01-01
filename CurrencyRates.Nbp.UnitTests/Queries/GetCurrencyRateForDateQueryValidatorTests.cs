using System;
using CurrencyRates.Nbp.Queries;

namespace CurrencyRates.Nbp.UnitTests.Queries;

public class GetCurrencyRateForDateQueryValidatorTests
{
    [Fact]
    public void GetCurrencyRateForDateQuery_Code_Empty_ShouldReturnError()
    {
        var query = new GetCurrencyRateForDateQuery("", new DateOnly(2024, 12, 30),true);
        var validator = new GetCurrencyRateForDateQuery.GetCurrencyRateQueryValidator();
        var result = validator.Validate(query);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Code");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Code cannot be empty");
    }
    
    [Fact]
    public void GetCurrencyRateForDateQuery_Code_ToLong_ShouldReturnError()
    {
        var query = new GetCurrencyRateForDateQuery("AB", new DateOnly(2024, 12, 30),true);
        var validator = new GetCurrencyRateForDateQuery.GetCurrencyRateQueryValidator();
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
    public void GetCurrencyRateForDateQuery_Date_MinValue_ShouldReturnError()
    {
        var query = new GetCurrencyRateForDateQuery("USD", DateOnly.MinValue,true);
        var validator = new GetCurrencyRateForDateQuery.GetCurrencyRateQueryValidator();
        var result = validator.Validate(query);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Date");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Date must be greater than minimum date value");
    }
    
    [Fact]
    public void GetCurrencyRateForDateQuery_Date_SmallerThan2002_1_2_ShouldReturnError()
    {
        var query = new GetCurrencyRateForDateQuery("USD", new DateOnly(2002, 1, 1),true);
        var validator = new GetCurrencyRateForDateQuery.GetCurrencyRateQueryValidator();
        var result = validator.Validate(query);
        
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Date");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Archival rate are available from 2002-01-02");
    }
    
    [Fact]
    public void GetCurrencyRateForDateQuery_ShouldPass()
    {
        var query = new GetCurrencyRateForDateQuery("USD", new DateOnly(2002, 1, 2),true);
        var validator = new GetCurrencyRateForDateQuery.GetCurrencyRateQueryValidator();
        var result = validator.Validate(query);
        
        Assert.True(result.IsValid);
    }
}