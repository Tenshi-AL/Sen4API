using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Persistence;

namespace Sen4.ServiceExtensions;

public static class IdentityExtension
{
    public static IServiceCollection IdentityConfigure(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<Sen4Context>()
            .AddDefaultTokenProviders();
        return serviceCollection;
    }
}