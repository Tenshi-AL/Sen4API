using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Sen4.IntegrationTests;

public class FakeTokenService
{
    public FakeTokenService(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public IConfiguration Configuration { get; private set; }
    public List<Claim> Claims { get; } = new();
    public int ExpiresInMinutes { get; set; } = 30;
    
    public FakeTokenService WithRole(string roleName)
    {
        Claims.Add(new Claim(ClaimTypes.Role, roleName));
        return this;
    }
    
    public FakeTokenService WithEmail(string email)
    {
        Claims.Add(new Claim(ClaimTypes.Name, email));
        return this;
    }
    
    public string? Build()
    {
        var jwtSecret = Configuration["JWT:Secret"] ?? throw new NullReferenceException();
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        
        var token = new JwtSecurityToken(
            issuer: Configuration["JWT:ValidIssuer"],
            audience: Configuration["JWT:ValidAudience"],
            expires: DateTime.UtcNow.AddHours(int.Parse(Configuration["JWT:AccessValidTime"]!)),
            claims: Claims,
            signingCredentials: new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}