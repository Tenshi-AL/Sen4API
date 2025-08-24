using Infrastructure.DTO;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Interfaces;

public interface ISen4AuthService
{
    Task<RefreshAccessToken?> GoogleAuth(string email);
    Task<IdentityResult> Register(UserRegistrationDTO userRegistrationDto);
    Task<RefreshAccessToken?> Login(LoginDTO loginDto);
    Task<string?> RefreshToken(RefreshAccessToken refreshAccessToken);
}