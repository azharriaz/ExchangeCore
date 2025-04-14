using ExchangeCore.Application.Auth.Commands;
using ExchangeCore.Application.Common.Interfaces;
using ExchangeCore.Application.Dto;
using ExchangeCore.Domain.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExchangeCore.Infrastructure.Services;

public class AuthService(UserManager<IdentityUser> userManager, IConfiguration configuration) : IAuthService
{
    public async Task<LoginDto> AuthenticateAsync(LoginCommand request)
    {
        // uncomment to use the supported password hashes.
        //var passwordHasher = new PasswordHasher<IdentityUser>();

        //// Generate hashes for your passwords
        //var adminHash = passwordHasher.HashPassword(null, "Admin@123");
        //var userHash = passwordHasher.HashPassword(null, "User@123");

        var user = await userManager.FindByNameAsync(request.Username);
        if (user is not null && await userManager.CheckPasswordAsync(user, request.Password))
        {
            var userRoles = await userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            authClaims.Add(new Claim(Constants.JWT_Client_Id_Key, Guid.CreateVersion7().ToString()));

            var token = GetToken(authClaims);

            return new()
            {
                Token= new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            };
        }

        throw new UnauthorizedAccessException();
    }

    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

        return token;
    }
}
