using System.Text.Json.Serialization;

namespace CurrencyRates.Nbp.Models.Rates;

public class NbpCurrencyRate
{
    /// <summary>
    /// Numer tabeli
    /// </summary>
    [JsonPropertyName("no")]
    public string Number { get; set; }
    
    /// <summary>
    /// Data publikacji
    /// </summary>
    [JsonPropertyName("effectiveDate")]
    public DateOnly EffectiveDate  { get; set; }
    
    /// <summary>
    /// Przeliczony kurs kupna waluty (dotyczy tabeli C)
    /// </summary>
    [JsonPropertyName("bid")]
    public decimal? PurchaseRate { get; set; }
    
    /// <summary>
    /// Przeliczony kurs sprzedaży waluty (dotyczy tabeli C)
    /// </summary>
    [JsonPropertyName("ask")]
    public decimal? SaleRate { get; set; }
    
    /// <summary>
    /// Przeliczony kurs średni waluty (dotyczy tabel A oraz B)
    /// </summary>
    [JsonPropertyName("mid")]
    public decimal? AvarageRate { get; set; }
}