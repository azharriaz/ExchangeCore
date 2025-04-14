namespace ExchangeCore.Application.Dto;

public class LoginDto
{
    public required string Token { get; set; }
    public DateTime Expiration { get; set; }
}
