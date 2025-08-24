using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoMapper;
using Infrastructure.DTO;
using Infrastructure.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Persistence;
using Sen4.IntegrationTests;
using Shouldly;

namespace Sen4.IntegrationTest.User;

public class List: IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _integrationTestWebAppFactory;
    private readonly HttpClient _httpClient;
    private readonly IServiceScope _serviceScope;
    private readonly Sen4Context _dataBase;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    
    public List(IntegrationTestWebAppFactory integrationTestWebAppFactory)
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
    
    [Fact(DisplayName = "Success get list with empty list request")]
    public async Task EmptyUserListRequest_SuccessGetList()
    {
        //arrange
        var userListRequest = new UserListRequest();

        var dictionary = new Dictionary<string, StringValues>();
        dictionary.Add("SortProperty", new StringValues(userListRequest.SortProperty));
        dictionary.Add("SortByDescending", new StringValues(userListRequest.SortByDescending.ToString()));
        dictionary.Add("PageSize", new StringValues(userListRequest.PageSize.ToString()));
        dictionary.Add("PageNumber", new StringValues(userListRequest.PageNumber.ToString()));

        var requestUrl = QueryHelpers.AddQueryString("User/List", dictionary);
        
        //act
        var response = await _httpClient.GetAsync(requestUrl);
        var userList = await response.Content.ReadFromJsonAsync<List<UserReadDTO>>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        userList.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Success get list with specified page size and number")]
    public async Task UserListRequestWithPageSizeAndPageNumber_SuccessGetList()
    {
        //arrange
        for (int i = 0; i < 5; i++)
        {
            await _httpClient.PostAsJsonAsync("user/:register", new UserRegistrationDTO()
            {
                Email = $"testWithPageSizeNumber{i}@gmail.com",
                Name = "Valid",
                Surname = "With",
                MiddleName = "Existed",
                PostId = new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),
                Password = "StrongPassword_Kj8_Dn3456_ty5&"
            });
        }
        
        var userListRequest = new UserListRequest();
        userListRequest.PageNumber = 1;
        userListRequest.PageSize = 5;
        
        var dictionary = new Dictionary<string, StringValues>();
        dictionary.Add("SortProperty", new StringValues(userListRequest.SortProperty));
        dictionary.Add("SortByDescending", new StringValues(userListRequest.SortByDescending.ToString()));
        dictionary.Add("PageSize", new StringValues(userListRequest.PageSize.ToString()));
        dictionary.Add("PageNumber", new StringValues(userListRequest.PageNumber.ToString()));
        
        var requestUrl = QueryHelpers.AddQueryString("User/List", dictionary);
        
        //act
        var response = await _httpClient.GetAsync(requestUrl);
        var userList = await response.Content.ReadFromJsonAsync<List<UserReadDTO>>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        userList.ShouldNotBeNull();
        userList.Count.ShouldBe(5);
    }

    [Fact(DisplayName = "Fail get list with outside page number")]
    public async Task UserListRequestWithOutsidePageNumber_FailGetList()
    {
        //assert
        var userListRequest = new UserListRequest();
        userListRequest.PageNumber = int.MaxValue;
        userListRequest.PageSize = 5;
        
        var dictionary = new Dictionary<string, StringValues>();
        dictionary.Add("SortProperty", new StringValues(userListRequest.SortProperty));
        dictionary.Add("SortByDescending", new StringValues(userListRequest.SortByDescending.ToString()));
        dictionary.Add("PageSize", new StringValues(userListRequest.PageSize.ToString()));
        dictionary.Add("PageNumber", new StringValues(userListRequest.PageNumber.ToString()));
        
        var requestUrl = QueryHelpers.AddQueryString("User/List", dictionary);
        
        //act
        var response = await _httpClient.GetAsync(requestUrl);
        var userList = await response.Content.ReadFromJsonAsync<List<UserReadDTO>>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        userList.ShouldNotBeNull();
        userList.Count.ShouldBe(0);
    }
    
    [Theory(DisplayName = "Fail get list with invalid specified page size and number")]
    [InlineData(-5,1)]
    [InlineData(10,-1)]
    public async Task UserListRequestWithInvalidPageSizeAndNumber_FailGetList(int pageSize, int pageNumber)
    {
        //assert
        var userListReguest = new UserListRequest();
        userListReguest.PageNumber = pageNumber;
        userListReguest.PageSize = pageSize;
        
        var dictionary = new Dictionary<string, StringValues>();
        dictionary.Add("PageSize", new StringValues(userListReguest.PageSize.ToString()));
        dictionary.Add("PageNumber", new StringValues(userListReguest.PageNumber.ToString()));
        
        var requestUrl = QueryHelpers.AddQueryString("User/List", dictionary);
        
        //act
        var response = await _httpClient.GetAsync(requestUrl);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}