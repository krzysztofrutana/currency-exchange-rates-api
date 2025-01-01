using System.Text.Json.Serialization;

namespace CurrencyRates.Nbp.Models.Tables;

public class NbpTable
{
    /// <summary>
    /// Typ tabeli
    /// </summary>
    [JsonPropertyName("table")]
    public string Table { get; set; }
    
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
    /// Data publikacji
    /// </summary>
    [JsonPropertyName("tradingDate")]
    public DateOnly? TradingDate  { get; set; }

    /// <summary>
    /// Informacje o kursach walut w tabeli
    /// </summary>
    [JsonPropertyName("rates")]
    public IEnumerable<NbpTableRate> Rates { get; set; } = [];
}