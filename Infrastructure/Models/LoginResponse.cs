namespace Infrastructure.Models;

public class RefreshAccessToken
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}