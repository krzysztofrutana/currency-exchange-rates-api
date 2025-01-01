namespace CurrencyRates.Common.Cache;

public class CacheHelper
{
    private const string DatabaseRateCacheKey = "Currency_{0}_date_{1}";
    public static string GetKeyForDatabaseRate(string currencyCode, DateOnly date) => string.Format(DatabaseRateCacheKey, currencyCode, date.ToString());
    
    
    private const string DatabaseCurrencyCacheKey = "Currency_{0}";
    public static string GetCurrencyKey(string currencyCode) => string.Format(DatabaseCurrencyCacheKey, currencyCode);

}