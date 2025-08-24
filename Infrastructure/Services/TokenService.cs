using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Infrastructure.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class TokenService: ITokenService
{
    public ClaimsPrincipal ValidateToken(string token,TokenValidationParameters validationParameters, string jwtSecret)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.ValidateToken(token, validationParameters, out _);
    }
    
    public string CreateJwtToken(List<Claim> claims, string jwtSecret, string issuer, string audience, DateTime? expires)
    {
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            expires: expires,
            claims: claims,
            signingCredentials: new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256)
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? GetClaimsPrincipalFromExpiredToken(string token, string secret, TokenValidationParameters validationParameters)
    {
        try
        {
            var result = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
            return result;
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
    public string CreateRefreshToken()
    {
        var token = new byte[64];
        using var generator = RandomNumberGenerator.Create();

        generator.GetBytes(token);
        return Convert.ToBase64String(token);
    }
}