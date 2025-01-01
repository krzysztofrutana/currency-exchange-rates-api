using System.ComponentModel;
using CurrencyExchangeRates.Data.Entities;

namespace CurrencyRates.Nbp.Models.Responses;

[Description("Kurs waluty")]
public class CurrencyRateResponse
{
    public CurrencyRateResponse(DateOnly forDate, CurrencyExchangeRate databaseValue)
    {
        ForDate = forDate;
        
        if(databaseValue is null)
            return;
        
        RateDate = databaseValue.EffectiveDate;
        PurchaseRate = databaseValue.PurchaseRate;
        SaleRate = databaseValue.SaleRate;
        AvarageRate = databaseValue.AvarageRate;
    }

    [Description("Wskazana data")]
    public DateOnly ForDate { get; set; }
    
    [Description("Data publikacji kursu")]
    public DateOnly? RateDate { get; set; }
    
    [Description("Przeliczony kurs kupna waluty")]
    public decimal? PurchaseRate { get; set; }
    
    [Description("Przeliczony kurs sprzedaży waluty")]
    public decimal? SaleRate { get; set; }
    
    [Description("Przeliczony kurs średni waluty")]
    public decimal? AvarageRate { get; set; }
}