using System.Net;
using System.Net.Http.Json;
using Infrastructure.DTO;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Sen4.IntegrationTests.Project;

public class Get: TestBase
{
    public Get(IntegrationTestWebAppFactory integrationTestWebAppFactory): base(integrationTestWebAppFactory)
    {
        //register main user
        _authorizationHelper.RegisterUserAsync("admin@gmail.com","Alex","Hlushko","Olegovich",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
        //register guest user
        _authorizationHelper.RegisterUserAsync("guest@gmail.com","John","Doe","Ragnarson",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
    }
    
    [Fact(DisplayName = "Success get by id. User with access.")]
    public async Task SuccessGetByIdUSerWithAccess()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessGetByIdUSerWithAccess");
        
        //act
        var response = await _httpClient.GetAsync($"Project/{project.Id}");
        var result = await response.Content.ReadFromJsonAsync<ProjectReadDTO>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Success get with added user and added rule")]
    public async Task SuccessGetAddedUSerAndAddedRule()
    {
        //create project
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessGetAddedUSerAndAddedRule");
        
        //create invite token
        var postResponse = await _httpClient.PostAsync($"Project/:generateInviteToken/{project.Id.ToString()}", null);
        var token = await postResponse.Content.ReadAsStringAsync();
        
        //login as new user and join to project
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        await _httpClient.PostAsync($"Project/:join/{token}", null);
        
        //login as admin and add "delete" rule for new user
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");

        var operation = await _sen4Context.Operations.FirstOrDefaultAsync(p => p.Controller == "Project" && p.Action == "Get");
        var userId = _userManager.FindByEmailAsync("guest@gmail.com").Result.Id;
        await _projectHelper.SetRuleAsync(userId, project.Id, operation.Id);
        
        //login as new user and check get rule
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var response = await _httpClient.GetAsync($"Project/{project.Id}");
        var result = await response.Content.ReadFromJsonAsync<ProjectReadDTO>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Fail get by id without auth")]
    public async Task FailGetByIdWithoutAuth()
    {
        //act
        var response = await _httpClient.GetAsync($"Project/{Guid.NewGuid().ToString()}");
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Fail get by id with auth user but not access")]
    public async Task FailGetByIdWithAuthUserButNotAccess()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailGetByIdWithAuthUserButNotAccess");
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        
        //act
        var response = await _httpClient.GetAsync($"Project/{project.Id}");
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}