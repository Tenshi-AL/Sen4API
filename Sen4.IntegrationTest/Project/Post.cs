using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infrastructure.DTO;
using Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Sen4.IntegrationTests.Project;

//TODO написать тест для проверки создания проекта с новым пользователем и созданными для него правами
//TODO переделать, в целом, с использованием ProjectHelper.cs
public class Post: TestBase
{
    public Post(IntegrationTestWebAppFactory integrationTestWebAppFactory): base(integrationTestWebAppFactory)
    {
        _authorizationHelper.RegisterUserAsync("admin@gmail.com","Alex","Hlushko","Olegovich",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
        var response = _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&").Result;
    }
    
    [Fact(DisplayName = "Success post with valid body and valid request id")]
    public async Task SuccessPostWithValidBodyAndValidRequestId()
    {
        //arrange
        var body = new ProjectWriteDTO()
        {
            Name = "Test valid body success post project",
            CreatedDateTime = DateTime.UtcNow,
            Description = null,
        };
        _httpClient.DefaultRequestHeaders.Add("requestId", Guid.NewGuid().ToString());
        
        //act
        var response = await _httpClient.PostAsJsonAsync("project", body);
        var result = await response.Content.ReadFromJsonAsync<ProjectReadDTO>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        result.ShouldNotBeNull();
    }
    
    [Fact(DisplayName = "Fail post with repeated body and request id")]
    public async Task FailPostWithRepeatedBodyAndRequestId()
    {
        //arrange
        var body = new ProjectWriteDTO()
        {
            Name = "Test fail post with repeated body and requestId",
            CreatedDateTime = DateTime.UtcNow,
            Description = null,
        };
        _httpClient.DefaultRequestHeaders.Add("requestId", Guid.NewGuid().ToString());
        await _httpClient.PostAsJsonAsync("project", body);
        
        //act
        var response = await _httpClient.PostAsJsonAsync("project", body);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
    
    [Fact(DisplayName = "Success post with repeated request id and new body")]
    public async Task SuccessPostWithRepeatedRequestIdAndNewBody()
    {
        //arrange
        _httpClient.DefaultRequestHeaders.Add("requestId", Guid.NewGuid().ToString());
        await _httpClient.PostAsJsonAsync("project", new ProjectWriteDTO()
        {
            Name = "First body name",
            CreatedDateTime = DateTime.UtcNow,
            Description = null,
        });
        
        //act
        var response = await _httpClient.PostAsJsonAsync("project", new ProjectWriteDTO()
        {
            Name = "Second body name",
            CreatedDateTime = DateTime.UtcNow,
            Description = null,
        });
        var result = await response.Content.ReadFromJsonAsync<ProjectReadDTO>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        result.ShouldNotBeNull();
    }
    
    [Fact(DisplayName = "Success post with repeated body and new request id")]
    public async Task SuccessPostWithRepeatedBodyAndNewRequestId()
    {
        //arrange
        var body = new ProjectWriteDTO()
        {
            Name = "SuccessPostWithRepeatedBodyAndNewRequestId",
            CreatedDateTime = DateTime.UtcNow,
            Description = null,
        };
        _httpClient.DefaultRequestHeaders.Add("requestId", Guid.NewGuid().ToString());
        await _httpClient.PostAsJsonAsync("project", body);
        
        _httpClient.DefaultRequestHeaders.Remove("requestId");
        _httpClient.DefaultRequestHeaders.Add("requestId", Guid.NewGuid().ToString());
        
        //act
        var response = await _httpClient.PostAsJsonAsync("project", body);
        var result = await response.Content.ReadFromJsonAsync<ProjectReadDTO>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        result.ShouldNotBeNull();
    }
    
    [Theory(DisplayName = "Fail post with invalid request id")]
    [InlineData("")]
    [InlineData(null)]
    public async Task FailPostWithInvalidRequestId(string? requestId)
    {
        //arrange
        var body = new ProjectWriteDTO()
        {
            Name = "FailPostWithInvalidRequestId",
            CreatedDateTime = DateTime.UtcNow,
            Description = null,
        };
        
        _httpClient.DefaultRequestHeaders.Add("requestId", requestId);
        
        //act
        var response = await _httpClient.PostAsJsonAsync("project", body);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    [Fact(DisplayName = "Fail post with invalid data")]
    public async Task InvalidData_FailPost()
    {
        //act
        _httpClient.DefaultRequestHeaders.Add("requestId", Guid.NewGuid().ToString());
        var response = await _httpClient.PostAsJsonAsync("project",  new ProjectWriteDTO()
        {
            Name = " ",
            CreatedDateTime = DateTime.UtcNow,
            Description = null,
        });
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}