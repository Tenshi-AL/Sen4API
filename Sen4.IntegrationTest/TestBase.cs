using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Sen4.IntegrationTests.Helpers;

namespace Sen4.IntegrationTests;

public class TestBase: IClassFixture<IntegrationTestWebAppFactory>
{
    protected readonly IntegrationTestWebAppFactory _integrationTestWebAppFactory;
    protected readonly HttpClient _httpClient;
    protected readonly Sen4Context _sen4Context;
    protected readonly IServiceScope _serviceScope;
    protected readonly UserManager<User> _userManager;

    protected readonly AuthorizationHelper _authorizationHelper;
    protected readonly ProjectHelper _projectHelper;
    
    public TestBase(IntegrationTestWebAppFactory integrationTestWebAppFactory)
    {
        _integrationTestWebAppFactory = integrationTestWebAppFactory;
        _httpClient = _integrationTestWebAppFactory.CreateClient();
        _serviceScope = _integrationTestWebAppFactory.Services.CreateScope();
        _userManager = _serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();
        _sen4Context = _serviceScope.ServiceProvider.GetRequiredService<Sen4Context>();

        _authorizationHelper = new AuthorizationHelper(_httpClient);
        _projectHelper = new ProjectHelper(_httpClient);
    }
}