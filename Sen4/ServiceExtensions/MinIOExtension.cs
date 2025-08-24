using Infrastructure.Helpers;
using Minio;

namespace Sen4.ServiceExtensions;

public static class MinIOExtension
{
    public static IServiceCollection AddMinIOConfiguration(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var endpoint = AppConfiguration.GetRequiredConfigurationValue(configuration, "MinIOCredentials:url");
        var accessKey = AppConfiguration.GetRequiredConfigurationValue(configuration, "MinIOCredentials:accessKey");
        var secretKey = AppConfiguration.GetRequiredConfigurationValue(configuration, "MinIOCredentials:secretKey");
        
        serviceCollection.AddMinio(configureClient => configureClient
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(false)
            .Build());
        return serviceCollection;
    }
}