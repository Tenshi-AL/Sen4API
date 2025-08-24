using Infrastructure.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Helpers;

public static class AppConfiguration
{
    public static string GetRequiredConfigurationValue(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrEmpty(value))
            throw new AppConfigurationException($"{key} is missing or empty in the configuration.");
        return value;
    }
}