
# Currency Exchange Rates API

Projekt służy do integracji z NPB oraz pobieraniem kursów walut. Projekt jest przygotowany do dodania kolejnych integracji z innymi źródłami walut.


## Technologia

API: .NET 9

Narzędzia i biblioteki:

- Entity Framework Core 9 (obsługa bazy danych oraz migracji)
- Hangfire (zadania cykliczne)
- MediatR (CQRS)
- Refit (integracja z NPB i pobieranie danych)
- Fluent Validation (walidacja formularzy)
- xUnit (testy jednostkowe)
- Serilog (logowanie do pliku)
- Scalar (dokumentacja)
## Interfejs API

#### Pobieranie aktualnego kursu waluty

```http
  GET /api/nbp/get-currency-rate/{currencyCode}/actual
```

| Parametr | Typ     | Opis                |
| :-------- | :------- | :------------------------- |
| `currencyCode` | `string` | **Wymagany**. Trzyliterowy kod waluty |

#### Pobieranie kursu waluty na wybrany dzień

```http
  GET /api/nbp/get-currency-rate/{currencyCode}/{date}
```

| Parametr | Typ     | Opis                |
| :-------- | :------- | :-------------------------------- |
| `currencyCode` | `string` | **Wymagany**. Trzyliterowy kod waluty |
| `date` | `date` | **Wymagany**. Data w formacie yyyy-MM-dd |

| Query | Typ     | Opis                |
| :-------- | :------- | :-------------------------------- |
| `showLastBeforeIfNotExist` | `bool` | Szukanie kursów z dni poprzednich (w przypadku dni wolnych od pracy) |


#### Pobieranie kursów waluty z ostatnich n dni

```http
  GET /api/nbp/get-currency-rate/{code}/last/{limit}
```

| Parametr | Typ     | Opis                |
| :-------- | :------- | :------------------------- |
| `currencyCode` | `string` | **Wymagany**. Trzyliterowy kod waluty |
| `limit` | `int` | **Wymagany**. Ilość dni (max 100) |


#### Pobieranie kursów waluty z ostatnich n dni

```http
  GET /api/nbp/get-currency-rate/{code}/{startDate}/{endDate}
```

| Parametr | Typ     | Opis                |
| :-------- | :------- | :------------------------- |
| `currencyCode` | `string` | **Wymagany**. Trzyliterowy kod waluty |
| `startDate` | `date` | **Wymagany**. Data w formacie yyyy-MM-dd (nie mniejsza niż 2 stycznia 2002) |
| `endDate` | `date` | **Wymagany**. Data końcowa (nie większa niż dzień obecny) |


#### Wszystkie powyższe endpointy zwrócą obiekt lub obiekty typu:

| Pole | Typ     | Opis                |
| :-------- | :------- | :------------------------- |
| `forDate` | `date` | **Zawsze.** Data dla której jest kurs |
| `rateDate` | `date` | **Zawsze.** Data publikacji kursu |
| `purchaseRate` | `decimal` | **Dla kursów z tabeli C.** Przeliczony kurs kupna waluty |
| `saleRate` | `decimal` | **Dla kursów z tabeli C.** Przeliczony kurs sprzedaży waluty |
| `avarageRate` | `decimal` | **Dla kursów z tabeli A i B.** Przeliczony kurs średni waluty |



## Funkcje

#### Zadania cykliczne

W projekcie możliwe jest dodawanie cyklicznych jobów, które obsługiwane są poprzez bibliotekę **Hangfire**. Wystarczy zaimplementować interfejs: `IScheduleJob`
oraz zaimplementować dwie właściwość `CronExpression` i metodę `Execute`.
Przykład implementacji właściwości:

```csharp
public string CronExpression => "*/15 * * * *";
```

Przykład implementacji metody:

```csharp
public async Task Execute()
{
    return Task.CompletedTask;
}
```
Wskazana metoda `Execute` będzie wykonywała się cyklicznie według podanej wartości w `CronExpression`.

#### Dokumentacja
API używa standardu OpenAPI oraz biblioteki Scalar do obsługi tego standardu i przedstawienia go w przystępnej formie. Opcja ta dostępna jest
tylko w środowisku developerskim pod adresem `/scalar/v1`.

## Konfiguracja

Konfiguracja wszelkich ustawień odbywa się poprzez plik `.env` w projekcie `CurrencyExchangeRates.Api`. Wymagane pola zawarte są w pliku `.env.example`.
W pliku `.env` deklaruje się `ConnectionString` do bazy `Microsoft SqlServer`, ścieżkę oraz dane do logowania do panelu Hangfire oraz Ip i port pod którym aplikacja ma nasłuchiwać.


## Budowanie

W głównym katalogu solucji należy uruchomić polecenie:
```
dotnet build
```
Wymagane jest zainstalowanie SDK .NET 9.

## Publikowanie

W głównym katalogu solucji należy uruchomić polecenie:
```
dotnet publish
```
Wymagane jest zainstalowanie SDK .NET 9.


#### Uruchmianie

Projekt posiada trzy możliwości uruchomienia

#### Kestrel

W przypadku pracy nad projektem w głównym katalogu solucji należy uruchomić polecenie 
```
dotnet run
```

W przypadku już opublikowanej wersji w katalogu publikacji należy uruchomić polecenie
```
dotnet CurrencyExchangeRates.Api.dll
```
lub uruchomić plik `CurrencyExchangeRates.Api.exe`


#### Usługa systemu windows
W katalogu `.tools` głównym katalogu solucji jest plik `InstallAsService.bat`. Należy go zedytować i w parametrze `binpath` podać ścieżkę do pliku `CurrencyExchangeRates.Api.exe` w opublikowanej wersji projektu. Następnie uruchomić wspomniany plik `InstallAsService.bat` w terminalu z uprawnieniami administratora. Zostanie utworzona nowa usługa systemu Windows `CurrencyExchangeRates` i aplikacja będzie nasłuchiwać na podanych w plikach `.env` IP i porcie. Usługa domyślnie uruchamia się z ustawieniami uruchamiania `Ręcznie`, według preferencji należy to zmienić w ustawieniach usług systemu Windows.


#### Docker
W folderze głównym projektu należy uruchomić polecenie
```
docker-compose
```
## Uruchamianie testów

W głównym katalogu solucji należy uruchomić polecenie:

```bash
  dotnet test
```



## Autor

- Krzysztof Rutana

