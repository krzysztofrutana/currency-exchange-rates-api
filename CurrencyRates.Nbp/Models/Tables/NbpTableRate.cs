using System.Text.Json.Serialization;

namespace CurrencyRates.Nbp.Models.Tables;

public class NbpTableRate
{
    /// <summary>
    /// Nazwa waluty
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency  { get; set; }
    
    /// <summary>
    /// Kod waluty
    /// </summary>
    [JsonPropertyName("code")]
    public string Code  { get; set; }
    
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