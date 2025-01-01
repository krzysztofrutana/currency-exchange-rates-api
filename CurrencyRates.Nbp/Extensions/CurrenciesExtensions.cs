using CurrencyExchangeRates.Data.Entities;
using CurrencyRates.Nbp.Models.AdditionalDatas;
using Newtonsoft.Json;

namespace CurrencyRates.Nbp.Extensions;

public static class CurrenciesExtensions
{
    public static CurrencyAdditionalData GetAdditionalData(this Currency currency)
    {
        if(string.IsNullOrEmpty(currency.AdditionalDataJson))
            return new CurrencyAdditionalData();
        
        return JsonConvert.DeserializeObject<CurrencyAdditionalData>(currency.AdditionalDataJson);
    }
    
    public static void SetAdditionalData(this Currency currency, CurrencyAdditionalData additionalData)
    {
        currency.AdditionalDataJson = JsonConvert.SerializeObject(additionalData);
    }
}