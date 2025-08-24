using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Persistence;
using Testcontainers.PostgreSql;

namespace Sen4.IntegrationTests;

public sealed class IntegrationTestWebAppFactory: WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:12.19")
        .WithDatabase("sen4db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            //configure postgres for test server
            var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<Sen4Context>));
            if (descriptor is not null) services.Remove(descriptor);
            services.AddDbContext<Sen4Context>(p => p.UseNpgsql(_postgreSqlContainer.GetConnectionString()));
        });
    }

    public Task InitializeAsync()
    {
        return _postgreSqlContainer.StartAsync();
    }

    public new Task DisposeAsync()
    {
        return _postgreSqlContainer.StopAsync();
    }
}