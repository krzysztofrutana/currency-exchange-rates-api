using CurrencyRates.Nbp.Models;
using CurrencyRates.Nbp.Models.Tables;
using Refit;

namespace CurrencyRates.Nbp.HttpClient;

public interface INbpTablesApiClient
{
    /// <summary>
    /// Aktualnie obowiązująca tabela kursów typu
    /// </summary>
    /// <param name="tableType">Typ tabeli (A, B, C)</param>
    /// <returns></returns>
    [Get("/{tableType}")]
    Task<IEnumerable<NbpTable>> GetTableAsync(string tableType);

    /// <summary>
    /// Tabela kursów typu opublikowana w podanym dniu (albo brak danych)
    /// </summary>
    /// <param name="tableType">Typ tabeli</param>
    /// <param name="date">Wybrany dzień</param>
    /// <returns></returns>
    [Get("/{tableType}/{date}")]
    Task<IEnumerable<NbpTable>> GetTableAsync(string tableType, DateOnly date);

    /// <summary>
    /// Seria tabel kursów typu opublikowanych w zakresie dat
    /// </summary>
    /// <param name="tableType">Typ tabeli</param>
    /// <param name="startDate">Data początkowa</param>
    /// <param name="endDate">Data końcowa</param>
    /// <returns></returns>
    [Get("/{tableType}/{startDate}/{endDate}")]
    Task<IEnumerable<NbpTable>> GetTableAsync(string tableType, DateOnly startDate, DateOnly endDate);
    
    /// <summary>
    /// Seria ostatnich tabel kursów typu
    /// </summary>
    /// <param name="tableType">Typ tabeli</param>
    /// <param name="limit">Limit wyników (maksymalna wartość to 67)</param>
    /// <returns></returns>
    [Get("/{tableType}/last/{limit}")]
    Task<IEnumerable<NbpTable>> GetLastTablesAsync(string tableType, int limit);
    
    /// <summary>
    /// Tabela kursów opublikowana w dniu dzisiejszym (albo brak danych)
    /// </summary>
    /// <param name="tableType">Typ tabeli</param>
    /// <returns></returns>
    [Get("/{tableType}/today")]
    Task<IEnumerable<NbpTable>> GetTodayTableAsync(string tableType);
}