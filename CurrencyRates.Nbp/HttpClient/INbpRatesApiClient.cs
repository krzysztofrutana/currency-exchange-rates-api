using CurrencyRates.Nbp.Models;
using CurrencyRates.Nbp.Models.Rates;
using CurrencyRates.Nbp.Models.Tables;
using Refit;

namespace CurrencyRates.Nbp.HttpClient;

public interface INbpRatesApiClient
{
    /// <summary>
    /// Aktualnie obowiązujący kurs waluty z wskazanej tabeli
    /// </summary>
    /// <param name="tableType">Typ tabeli (A, B, C)</param>
    /// <param name="code">Trzyliterowy kod waluty</param>
    /// <returns></returns>
    [Get("/{tableType}/{code}")]
    Task<NbpCurrency> GetCurrencyRateAsync(string tableType, string code);

    /// <summary>
    /// Kurs waluty z wskazanej tabeli dla wskazanej daty (albo brak wyników)
    /// </summary>
    /// <param name="tableType">Typ tabeli (A, B, C)</param>
    /// <param name="code">Trzyliterowy kod waluty</param>
    /// <param name="date">Data kursu waluty</param>
    /// <returns></returns>
    [Get("/{tableType}/{code}/{date}")]
    Task<NbpCurrency> GetCurrencyRateAsync(string tableType, string code, DateOnly date);

    /// <summary>
    /// Seria kursów waluty z tabeli kursów wskazanego typu opublikowanych w zakresie dat (albo brak danych)
    /// </summary>
    /// <param name="tableType">Typ tabeli (A, B, C)</param>
    /// <param name="code">Trzyliterowy kod waluty</param>
    /// <param name="startDate">Data od</param>
    /// <param name="endDate">Data do</param>
    /// <returns></returns>
    [Get("/{tableType}/{code}/{startDate}/{endDate}")]
    Task<NbpCurrency> GetCurrencyRatesAsync(string tableType, string code, DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// Seria ostatnich kursów waluty z wskazanej tabeli
    /// </summary>
    /// <param name="tableType">Typ tabeli (A, B, C)</param>
    /// <param name="code">Trzyliterowy kod waluty</param>
    /// <param name="limit">Limit wyników</param>
    /// <returns></returns>
    [Get("/{tableType}/{code}/last/{limit}")]
    Task<NbpCurrency> GetLastCurrencyRatesAsync(string tableType, string code, int limit);
    
    /// <summary>
    /// Kurs waluty z tabeli kursów wskazanego typu opublikowany w dniu dzisiejszym (albo brak danych)
    /// </summary>
    /// <param name="tableType">Typ tabeli (A, B, C)</param>
    /// <param name="code">Trzyliterowy kod waluty</param>
    /// <returns></returns>
    [Get("/{tableType}/{code}/today")]
    Task<NbpCurrency> GetTodayCurrencyRateAsync(string tableType, string code);
    
}