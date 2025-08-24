using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infrastructure.DTO;
using Infrastructure.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Shouldly;

namespace Sen4.IntegrationTests.Project;

public class List: TestBase
{
    public List(IntegrationTestWebAppFactory integrationTestWebAppFactory):base(integrationTestWebAppFactory)
    {
        //register main user
        _authorizationHelper.RegisterUserAsync("admin@gmail.com","Alex","Hlushko","Olegovich",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
        //register guest user
        _authorizationHelper.RegisterUserAsync("guest@gmail.com","John","Doe","Ragnarson",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
    }
    
    [Fact(DisplayName = "Success get list with empty project list request")]
    public async Task EmptyProjectListRequest_SuccessGetList()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var projectListRequest = new ProjectListRequest();
        
        var dictionary = new Dictionary<string, StringValues>();
        dictionary.Add("PageSize", new StringValues(projectListRequest.PageSize.ToString()));
        dictionary.Add("PageNumber", new StringValues(projectListRequest.PageNumber.ToString()));
        
        var requestUrl = QueryHelpers.AddQueryString("Project/List", dictionary);
        
        //act
        var response = await _httpClient.GetAsync(requestUrl);
        var userList = await response.Content.ReadFromJsonAsync<List<ProjectReadDTO>>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        userList.ShouldNotBeNull();
    }
    
    [Theory(DisplayName = "Fail get list with invalid page size and page or page number")]
    [InlineData(-5,1)]
    [InlineData(10,-1)]
    public async Task ProjectListRequestWithInvalidPageSizeAndNumber_Fail(int pageSize, int pageNumber)
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var dictionary = new Dictionary<string, StringValues>();
        dictionary.Add("PageSize", new StringValues(pageSize.ToString()));
        dictionary.Add("PageNumber", new StringValues(pageNumber.ToString()));
        
        var requestUrl = QueryHelpers.AddQueryString("Project/List", dictionary);
        
        //act
        var response = await _httpClient.GetAsync(requestUrl);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}