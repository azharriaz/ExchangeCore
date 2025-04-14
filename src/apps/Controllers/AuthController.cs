using ExchangeCore.Api.Controllers;
using ExchangeCore.Application.Auth.Commands;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeCore.API.Controllers;

/// <summary>
/// auth controller, manages the basic sign in operations
/// </summary>
public class AuthController : BaseApiController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand loginCommand, CancellationToken cancellationToken)
    {
        return Ok(await Mediator.Send(loginCommand, cancellationToken));
    }
}
