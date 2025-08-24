using System.Security.Claims;
using Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Interfaces;

public interface ITokenService
{
    ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, string jwtSecret);
    string CreateRefreshToken();
    ClaimsPrincipal? GetClaimsPrincipalFromExpiredToken(string token, string secret, TokenValidationParameters validationParameters);
    string CreateJwtToken(List<Claim> claims, string jwtSecret, string issuer, string audience, DateTime? expires);
}