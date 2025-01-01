using System.ComponentModel.DataAnnotations;

namespace CurrencyExchangeRates.Data.Entities;

public class CurrencyExchangeRate
{
    public int Id { get; set; }
    public int CurrencyId { get; set; }
    public virtual Currency Currency { get; set; }
    
    public DateOnly ForDate { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public decimal? PurchaseRate { get; set; }
    public decimal? SaleRate { get; set; }
    public decimal? AvarageRate { get; set; }

    [MaxLength(50)]
    public string Source { get; set; }
    
}