using System.Text.Json.Serialization;

namespace CurrencyRates.Nbp.Models.Rates;

public class NbpCurrency
{
    /// <summary>
    /// Typ tabeli
    /// </summary>
    [JsonPropertyName("table")]
    public string Table { get; set; }

    /// <summary>
    /// Nazwa kraju
    /// </summary>
    [JsonPropertyName("country")]
    public string Country { get; set; }

    /// <summary>
    /// Symbol waluty (numeryczny, dotyczy kursów archiwalnych)
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }
    
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
    /// Lista kursów poszczególnych walut w tabeli
    /// </summary>
    [JsonPropertyName("rates")]
    public IEnumerable<NbpCurrencyRate> Rates { get; set; }
}