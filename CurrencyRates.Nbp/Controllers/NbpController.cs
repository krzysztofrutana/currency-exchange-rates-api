using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CurrencyRates.Nbp.Models.Responses;
using CurrencyRates.Nbp.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyRates.Nbp.Controllers;

[ApiController]
[Route("api/nbp")]
public class NbpController : ControllerBase
{
    private readonly ISender _sender;

    public NbpController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpGet("get-currency-rate/{code}/actual")]
    [ProducesResponseType(404)]
    [ProducesResponseType<CurrencyRateResponse>(StatusCodes.Status200OK)]
    [EndpointSummary("Pobieranie aktualnego kursu waluty")]
    public async Task<ActionResult<CurrencyRateResponse>> GetActualCurrencyRate(
        [Description("Trzyliterowy kod waluty")] [Required] string code)
    {
        var data = await _sender.Send(new GetCurrencyRateForDateQuery(code, DateOnly.FromDateTime(DateTime.Now), true));

        if (data.IsFailure)
            return BadRequest(data.ErrorMessage);

        return Ok(data.ResultValue);
    }
    
    [HttpGet("get-currency-rate/{code}/{date}")]
    [ProducesResponseType<CurrencyRateResponse>(200)]
    [ProducesResponseType(404)]
    [EndpointSummary("Pobieranie kursu waluty na wybrany dzień")]
    public async Task<ActionResult<CurrencyRateResponse>> GetCurrencyRateForDate(
        [Description("Trzyliterowy kod waluty")] [Required] string code, 
        [Description("Data w formacie yyyy-MM-dd")] [Required] DateOnly date,
        [Description("Szukanie kursów z dni poprzednich (w przypadku dni wolnych od pracy)")] [FromQuery] bool showLastBeforeIfNotExist = false)
    {
        var data = await _sender.Send(new GetCurrencyRateForDateQuery(code, date, showLastBeforeIfNotExist));

        if (data.IsFailure)
            return BadRequest(data.ErrorMessage);

        return Ok(data.ResultValue);
    }
    
    [HttpGet("get-currency-rate/{code}/last/{limit}")]
    [ProducesResponseType<IEnumerable<CurrencyRateResponse>>(200)]
    [ProducesResponseType(404)]
    [EndpointSummary("Pobieranie kursów waluty z ostatnich n dni")]
    public async Task<ActionResult<IEnumerable<CurrencyRateResponse>>> GetActualCurrencyRate(
        [Description("Trzyliterowy kod waluty")] [Required] string code, 
        [Description("Ilość dni (max 100)")] [Required] int limit)
    {
        var data = await _sender.Send(new GetCurrencyRatesFromDatePeriodQuery(code, limit));

        if (data.IsFailure)
            return BadRequest(data.ErrorMessage);

        return Ok(data.ResultValue);
    }
    
    [HttpGet("get-currency-rate/{code}/{startDate}/{endDate}")]
    [ProducesResponseType<IEnumerable<CurrencyRateResponse>>(200)]
    [ProducesResponseType(404)]
    [EndpointSummary("Pobieranie kursów z przedziału dat. ")]
    [EndpointDescription("Przedział dat nie może być większy niż 100 dni")]
    public async Task<ActionResult<IEnumerable<CurrencyRateResponse>>> GetCurrencyRateForDate(
        [Description("Trzyliterowy kod waluty")] [Required] string code, 
        [Description("Data początkowa (nie mniejsza niż 2 stycznia 2002)")] [Required] DateOnly startDate, 
        [Description("Data końcowa (nie większa niż dzień obecny)")] [Required] DateOnly endDate)
    {
        if(startDate == endDate)
            return BadRequest("Start date cannot be the same as end date");
        
        var data = await _sender.Send(new GetCurrencyRatesFromDatePeriodQuery(code, startDate, endDate));

        if (data.IsFailure)
            return BadRequest(data.ErrorMessage);

        return Ok(data.ResultValue);
    }
}