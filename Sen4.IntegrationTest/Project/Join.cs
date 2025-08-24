using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infrastructure.DTO;
using Infrastructure.Models;
using Shouldly;

namespace Sen4.IntegrationTests.Project;

public class Join: TestBase
{
    public Join(IntegrationTestWebAppFactory integrationTestWebAppFactory): base(integrationTestWebAppFactory)
    {
        //register main user
        _authorizationHelper.RegisterUserAsync("admin@gmail.com","Alex","Hlushko","Olegovich",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
        //register guest user
        _authorizationHelper.RegisterUserAsync("guest@gmail.com","John","Doe","Ragnarson",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
    }
    
    
    [Fact(DisplayName = "Success join to project with valid token and auth user")]
    public async Task SuccessJoinToProjectWithValidTokenAndAuthUser()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessJoinToProjectWithValidTokenAndAuthUser");
        var postResponse = await _httpClient.PostAsync($"Project/:generateInviteToken/{project.Id.ToString()}", null);
        var token = await postResponse.Content.ReadAsStringAsync();
        
        //act
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var response = await _httpClient.PostAsync($"Project/:join/{token}", null);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Fail join with invalid token")]
    public async Task FailJoinWithInvalidProjectIdAndInvalidToken()
    {
        //arrange
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        
        //act
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var response = await _httpClient.PostAsync($"Project/:join/{token}", null);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Fail join to project with already exists user")]
    public async Task FailJoinToProjectWithAlreadyExistsUser()
    {
        //assert
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailJoinToProjectWithAlreadyExistsUser");
        var response = await _httpClient.PostAsync($"Project/:generateInviteToken/{project.Id.ToString()}", null);
        var token = await response.Content.ReadAsStringAsync();
        
        //act
        var firstUser = await _httpClient.PostAsync($"Project/:join/{token}", null);
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        await _httpClient.PostAsync($"Project/:join/{token}", null);
        var secondUser = await _httpClient.PostAsync($"Project/:join/{token}", null);
        
        //assert
        firstUser.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        secondUser.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}