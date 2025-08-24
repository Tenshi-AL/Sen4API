using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoMapper;
using Infrastructure.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Sen4.IntegrationTests;
using Shouldly;

namespace Sen4.IntegrationTest.User;

public class Get : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _integrationTestWebAppFactory;
    private readonly HttpClient _httpClient;
    private readonly IServiceScope _serviceScope;
    private readonly Sen4Context _dataBase;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    
    public Get(IntegrationTestWebAppFactory integrationTestWebAppFactory)
    {
        _integrationTestWebAppFactory = integrationTestWebAppFactory;
        _httpClient = _integrationTestWebAppFactory.CreateClient();
        _serviceScope = _integrationTestWebAppFactory.Services.CreateScope();
        _dataBase = _serviceScope.ServiceProvider.GetRequiredService<Sen4Context>();
        _mapper = _serviceScope.ServiceProvider.GetRequiredService<IMapper>();
        _configuration = _serviceScope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        var token = new FakeTokenService(_configuration).WithRole("Admin").Build();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
    
    [Fact(DisplayName = "Success get with valid id and existed user")]
    public async Task ValidId_With_Existed_User_GetSuccess()
    {
        //arrange
        await _httpClient.PostAsJsonAsync("user/:register", new UserRegistrationDTO()
        {
            Email = "validIdwithExistedUser@gmail.com",
            Name = "Valid",
            Surname = "With",
            MiddleName = "Existed",
            PostId = new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),
            Password = "StrongPassword_Kj8_Dn3456_ty5&"
        });

        var userFromDatabase = await _dataBase.Users.Where(p => p.Email == "validIdwithExistedUser@gmail.com").FirstOrDefaultAsync();
        var expectedUser = _mapper.Map<UserReadDTO>(userFromDatabase);
        
        //act
        var response = await _httpClient.GetAsync($"user/{userFromDatabase.Id}");
        var userFromResponse = await response.Content.ReadFromJsonAsync<UserReadDTO>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        userFromResponse.ShouldNotBeNull();
        userFromResponse.ShouldBeEquivalentTo(expectedUser);
    }

    [Theory(DisplayName = "Fail get with invalid id")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task InvalidId_GetFail(string id)
    {
        //act
        var response = await _httpClient.GetAsync($"user/{id}");
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}