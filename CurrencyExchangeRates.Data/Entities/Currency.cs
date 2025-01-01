using System.ComponentModel.DataAnnotations;

namespace CurrencyExchangeRates.Data.Entities;

public class Currency
{
    public int Id { get; set; }
    
    [MaxLength(3)]
    public string Code { get; set; }

    public string AdditionalDataJson { get; set; }
    
    public virtual ICollection<CurrencyExchangeRate> CurrencyExchangeRates { get; set; } = new List<CurrencyExchangeRate>();
}