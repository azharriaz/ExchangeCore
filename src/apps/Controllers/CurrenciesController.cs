using ExchangeCore.Application.Common.Models;
using ExchangeCore.Application.CurrencyConverter.Queries.CurrencyConversion;
using ExchangeCore.Application.CurrencyConverter.Queries.GetHistoricRates;
using ExchangeCore.Application.CurrencyConverter.Queries.GetLatestExchangeRates;
using ExchangeCore.Application.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeCore.Api.Controllers;

/// <summary>
/// Currencies controller
/// </summary>
public class CurrenciesController : BaseApiController
{
    /// <summary>
    /// Retrieves the latest exchange rates for a specified base currency.
    /// </summary>
    /// <param name="query">The query containing the base currency information.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Latest exchange rates.</returns>
    /// <response code="200">Returns the latest exchange rates.</response>
    /// <response code="400">If the query is invalid.</response>
    /// <response code="500">If there is an internal server error.</response>
    [HttpGet("latest")]
    [Authorize(Policy = "AppUserRole")]
    [ProducesResponseType(typeof(ServiceResult<ExchangeRatesDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ServiceResult<ExchangeRatesDto>>> GetLatestAsync([FromQuery] GetLatestExchangeRatesQuery query,
         CancellationToken cancellationToken)
    {
        return Ok(await Mediator.Send(query, cancellationToken));
    }

    /// <summary>
    /// Converts an amount from one currency to another.
    /// </summary>
    /// <param name="query">The query containing the amount and currencies to convert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Converted currency value.</returns>
    /// <response code="200">Returns the converted currency value.</response>
    /// <response code="400">If the query is invalid.</response>
    /// <response code="500">If there is an internal server error.</response>
    [HttpGet("convert")]
    [Authorize(Policy = "AppUserRole")]
    [ProducesResponseType(typeof(ServiceResult<ExchangeRatesDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ServiceResult<ExchangeRatesDto>>> ConvertCurrencyAsync([FromQuery] ConvertCurrencyQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await Mediator.Send(query, cancellationToken));
    }

    /// <summary>
    /// Retrieves historical exchange rates for a specified date range and base currency.
    /// </summary>
    /// <param name="query">The query containing the date range and base currency information.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated historical exchange rates.</returns>
    /// <response code="200">Returns the historical exchange rates.</response>
    /// <response code="400">If the query is invalid.</response>
    /// <response code="500">If there is an internal server error.</response>
    [HttpGet("history")]
    [Authorize(Policy = "AdminUserRole")]
    [ProducesResponseType(typeof(ServiceResult<PaginatedGetHistoricRatesDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ServiceResult<PaginatedGetHistoricRatesDto>>> GetLatestAsync(
        [FromQuery] GetHistoricalRatesQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await Mediator.Send(query, cancellationToken));
    }
}
