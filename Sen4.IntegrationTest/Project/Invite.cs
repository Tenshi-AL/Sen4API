using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Domain.Models;
using Infrastructure.DTO;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Shouldly;

namespace Sen4.IntegrationTests.Project;

public class Invite: TestBase
{
    public Invite(IntegrationTestWebAppFactory integrationTestWebAppFactory): base(integrationTestWebAppFactory)
    { 
        //register main user
        _authorizationHelper.RegisterUserAsync("admin@gmail.com","Alex","Hlushko","Olegovich",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
        //register guest user
        _authorizationHelper.RegisterUserAsync("guest@gmail.com","John","Doe","Ragnarson",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
    }
    
    [Fact(DisplayName = "Success generate invite token with auth user and access")]
    public async Task SuccessGenerateInviteTokenWithAuthUserAndAccess()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessGenerateInviteTokenWithAuthUserAndAccess");
        
        //act
        var response = await _httpClient.PostAsync($"Project/:generateInviteToken/{project.Id.ToString()}", null);
        var result = await response.Content.ReadAsStringAsync();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Success invite with added user and added rule")]
    public async Task SuccessInviteWithAddedUserAndAddedRule()
    {
        //create project
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessInviteWithAddedUserAndAddedRule");
        
        //create invite token
        var postResponse = await _httpClient.PostAsync($"Project/:generateInviteToken/{project.Id.ToString()}", null);
        var token = await postResponse.Content.ReadAsStringAsync();
        
        //login as new user and join to project
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var debug1 = await _httpClient.PostAsync($"Project/:join/{token}", null);
        
        //login as admin and add "delete" rule for new user
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&"); await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");

        var operation = await _sen4Context.Operations.FirstOrDefaultAsync(p => p.Controller == "Project" && p.Action == "GenerateInviteToken");
        var userId = _userManager.FindByEmailAsync("guest@gmail.com").Result.Id;
        await _projectHelper.SetRuleAsync(userId, project.Id, operation.Id);
        
        //login as new user and check delete rule
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var response = await _httpClient.PostAsync($"Project/:generateInviteToken/{project.Id.ToString()}", null);
        var result = await response.Content.ReadAsStringAsync();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
    }
    
    [Fact(DisplayName = "Fail generate invite token with auth user which is not in project")]
    public async Task FailGenerateInviteTokenWithAuthUserWithIsNotInProject()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailGenerateInviteTokenWithAuthUserWithIsNotInProject");
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        
        //act
        var response = await _httpClient.PostAsync($"Project/:generateInviteToken/{project.Id.ToString()}", null);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Fail generate invite token with auth user in project but without rule")]
    public async Task FailGenerateInviteTokenWithAuthUserInProjectButWithoutRule()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailGenerateInviteTokenWithAuthUserInProjectButWithoutRule");
        var response = await _httpClient.PostAsJsonAsync("Project/:generateInviteToken", project.Id.ToString());
        var token = await response.Content.ReadAsStringAsync();
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        await _httpClient.PostAsJsonAsync("Project/:join", token);
        
        //act
        var result = await _httpClient.PostAsync($"Project/:generateInviteToken/{project.Id.ToString()}", null);
        
        //assert
        result.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Fail generate invite token with non auth user")]
    public async Task FailGenerateInviteTokenWithNonAuthUser()
    {
        //act
        var response = await _httpClient.PostAsync($"Project/:generateInviteToken/{Guid.NewGuid().ToString()}", null);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}