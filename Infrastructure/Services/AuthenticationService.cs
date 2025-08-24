using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Domain.Models;
using Infrastructure.DTO;
using Infrastructure.Helpers;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Persistence;

namespace Infrastructure.Services;

public class AuthenticationService(Sen4Context db, UserManager<User> userManager, IMapper mapper, ITokenService tokenService, IConfiguration configuration): ISen4AuthService
{
    public async Task<IdentityResult> Register(UserRegistrationDTO userRegistrationDto)
    {
        var newUser = mapper.Map<User>(userRegistrationDto);
        var result = await userManager.CreateAsync(newUser, userRegistrationDto.Password);
        return result;
    }
    
    public async Task<RefreshAccessToken?> Login(LoginDTO loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        
        return user is not null && await userManager.CheckPasswordAsync(user, loginDto.Password)
            ? await GenerateRefreshAccessToken(user)
            : null;
    }

    private async Task<RefreshAccessToken> GenerateRefreshAccessToken(User user)
    {
        //create access&refresh tokens
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var jwtSecret = AppConfiguration.GetRequiredConfigurationValue(configuration,"JWT:Secret");
        var issuer = AppConfiguration.GetRequiredConfigurationValue(configuration, "JWT:ValidIssuer");
        var audience = AppConfiguration.GetRequiredConfigurationValue(configuration, "JWT:ValidAudience");
        var expires = DateTime.UtcNow.AddHours(int.Parse(AppConfiguration.GetRequiredConfigurationValue(configuration, "JWT:AccessValidTime")));
        
        var accessToken = tokenService.CreateJwtToken(claims, jwtSecret, issuer, audience, expires);
        var refreshToken = tokenService.CreateRefreshToken();

        //save refresh token in db
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(int.Parse(AppConfiguration.GetRequiredConfigurationValue(configuration,"JWT:RefreshValidTime")));
            
        db.Users.Update(user);
        await db.SaveChangesAsync();
        
        return new RefreshAccessToken()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
    
    public async Task<RefreshAccessToken?> GoogleAuth(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user is not null
            ? await GenerateRefreshAccessToken(user)
            : null;
    }
    
    public async Task<string?> RefreshToken(RefreshAccessToken refreshAccessToken)
    {
        //validate token
        var secret = AppConfiguration.GetRequiredConfigurationValue(configuration, "JWT:Secret");
        var validationParameters = new TokenValidationParameters()
        {
            ValidIssuer = AppConfiguration.GetRequiredConfigurationValue(configuration,"JWT:ValidIssuer"),
            ValidAudience = AppConfiguration.GetRequiredConfigurationValue(configuration,"JWT:ValidAudience"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = false
        };
        
        var principal = tokenService.GetClaimsPrincipalFromExpiredToken(refreshAccessToken.AccessToken, secret, validationParameters);
        if (principal?.Identity?.Name is null) return null;
        var user = await userManager.FindByNameAsync(principal.Identity.Name);

        if (user is null || user.RefreshToken != refreshAccessToken.RefreshToken ||
            user.RefreshTokenExpiry < DateTime.UtcNow)
            return null;
            
        //create access&refresh tokens
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var jwtSecret = AppConfiguration.GetRequiredConfigurationValue(configuration,"JWT:Secret");
        var issuer = AppConfiguration.GetRequiredConfigurationValue(configuration, "JWT:ValidIssuer");
        var audience = AppConfiguration.GetRequiredConfigurationValue(configuration, "JWT:ValidAudience");
        var expires = DateTime.UtcNow.AddHours(int.Parse(AppConfiguration.GetRequiredConfigurationValue(configuration, "JWT:AccessValidTime")));
        
        var accessToken = tokenService.CreateJwtToken(claims, jwtSecret, issuer, audience, expires);

        return accessToken;
    }
}