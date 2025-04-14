using ExchangeCore.Application.Auth.Commands;
using ExchangeCore.Application.Dto;

namespace ExchangeCore.Application.Common.Interfaces;

public interface IAuthService
{
    Task<LoginDto> AuthenticateAsync(LoginCommand loginCommand);
}
