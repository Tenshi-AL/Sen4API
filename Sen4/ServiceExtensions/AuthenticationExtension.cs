using System.Text;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Sen4.ServiceExtensions;

public static class AuthenticationExtension
{
    public static IServiceCollection AuthenticationConfigure(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ClockSkew = new TimeSpan(0, 0, 5),
                ValidAudience = configuration["JWT:ValidAudience"],
                ValidIssuer = configuration["JWT:ValidIssuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!))
            };
            
            //add token to hubs
            options.Events = new JwtBearerEvents()
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notification"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        }).AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = AppConfiguration.GetRequiredConfigurationValue(configuration,"GoogleAuthOptions:ClientId");
            googleOptions.ClientSecret = AppConfiguration.GetRequiredConfigurationValue(configuration, "GoogleAuthOptions:ClientSecret");
        });
        
        return serviceCollection;
    }
}