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

namespace Sen4.IntegrationTests.ProjectTask;

public class Delete: TestBase
{
    public Delete(IntegrationTestWebAppFactory integrationTestWebAppFactory): base(integrationTestWebAppFactory)
    {
        //register main user
        _authorizationHelper.RegisterUserAsync("admin@gmail.com","Alex","Hlushko","Olegovich",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
        //register guest user
        _authorizationHelper.RegisterUserAsync("guest@gmail.com","John","Doe","Ragnarson",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
    }
    
    [Fact(DisplayName = "Success delete with auth user and access")]
    public async Task SuccessDeleteWithAuthUserAndAccess()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessDeleteWithAuthUserAndAccess");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        var taskPostResponse = await _projectHelper.CreateTaskAsync(project.Id,"FailDeleteWithAuthUserAndNonAccess", user.Id);
        var task = await taskPostResponse.Content.ReadFromJsonAsync<ProjectTaskReadDTO>();
        
        //act
        var response = await _httpClient.DeleteAsync($"ProjectTask/{task.Id}");
        
        //arrange
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Fail delete with non auth user")]
    public async Task FailDeleteWithNonAuthUser()
    {
        //act
        var response = await _httpClient.DeleteAsync($"ProjectTask/{Guid.NewGuid()}");
        
        //arrange
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Fail delete with auth user and non access")]
    public async Task FailDeleteWithAuthUserAndNonAccess()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailDeleteWithAuthUserAndNonAccess");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        var taskPostResponse = await _projectHelper.CreateTaskAsync(project.Id,"FailDeleteWithAuthUserAndNonAccess", user.Id);
        var task = await taskPostResponse.Content.ReadFromJsonAsync<ProjectTaskReadDTO>();
        //act
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var response = await _httpClient.DeleteAsync($"ProjectTask/{task.Id}");
        
        //arrange
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Success delete with added user with access")]
    public async Task SuccessDeleteWithAddedUserWithAccess()
    {
        //create project and task
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessDeleteWithAddedUserWithAccess");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        var taskPostResponse = await _projectHelper.CreateTaskAsync(project.Id,"FailDeleteWithAuthUserAndNonAccess", user.Id);
        var task = await taskPostResponse.Content.ReadFromJsonAsync<ProjectTaskReadDTO>();
        
        //create invite token
        var postResponse = await _httpClient.PostAsync($"Project/:generateInviteToken/{project.Id.ToString()}", null);
        var token = await postResponse.Content.ReadAsStringAsync();
        
        //login as new user and join to project
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        await _httpClient.PostAsync($"Project/:join/{token}", null);
        
        //login as admin and add "delete" rule for new user
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var operation = await _sen4Context.Operations.FirstOrDefaultAsync(p => p.Controller == "ProjectTask" && p.Action == "Delete");
        await _httpClient.PostAsJsonAsync("Rule", new SetRuleModel()
        {
            UserId = _userManager.FindByEmailAsync("guest@gmail.com").Result.Id,
            ProjectId = project.Id,
            Rules = new List<RuleDTO>()
            {
                new RuleDTO()
                {
                    OperationId = operation.Id,
                    Access = true
                }
            }
        });
        
        //login as new user and check delete rule
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var response = await _httpClient.DeleteAsync($"ProjectTask/{task.Id}");
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}