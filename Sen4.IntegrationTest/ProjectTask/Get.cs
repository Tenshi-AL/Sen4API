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

public class Get: TestBase
{
    public Get(IntegrationTestWebAppFactory integrationTestWebAppFactory): base(integrationTestWebAppFactory)
    {
        //register main user
        _authorizationHelper.RegisterUserAsync("admin@gmail.com","Alex","Hlushko","Olegovich",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
        //register guest user
        _authorizationHelper.RegisterUserAsync("guest@gmail.com","John","Doe","Ragnarson",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
    }
    
    
    [Fact(DisplayName = "Success get task by id with user with access")]
    public async Task SuccessGetTaskByIdWithUserWithAccess()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessGetTaskByIdWithUserWithAccess");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        var taskPostResponse = await _projectHelper.CreateTaskAsync(project.Id,"FailDeleteWithAuthUserAndNonAccess", user.Id);
        var task = await taskPostResponse.Content.ReadFromJsonAsync<ProjectTaskReadDTO>();
        
        //act
        var response = await _httpClient.GetAsync($"ProjectTask/{task.Id.ToString()}");
        var result = await response.Content.ReadFromJsonAsync<ProjectTaskReadDTO>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Success get with new user with access")]
    public async Task SuccessGetWithNewUserWithAccess()
    {
        //create project and task
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessGetWithNewUserWithAccess");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        var taskPostResponse = await _projectHelper.CreateTaskAsync(project.Id,"FailDeleteWithAuthUserAndNonAccess", user.Id);
        var task = await taskPostResponse.Content.ReadFromJsonAsync<ProjectTaskReadDTO>();
        
        //create invite token
        var postResponse = await _httpClient.PostAsync($"Project/:generateInviteToken/{project.Id.ToString()}", null);
        var token = await postResponse.Content.ReadAsStringAsync();
        
        //login as new user and join to project
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        await _httpClient.PostAsync($"Project/:join/{token}", null);
        
        //login as admin and add "get" rule for new user
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var operation = await _sen4Context.Operations.FirstOrDefaultAsync(p => p.Controller == "ProjectTask" && p.Action == "Get");
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
        
        //login as new user and check get rule
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        //act
        var response = await _httpClient.GetAsync($"ProjectTask/{task.Id.ToString()}");
        var result = await response.Content.ReadFromJsonAsync<ProjectTaskReadDTO>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Fail get without auth")]
    public async Task FailGetWithoutAuth()
    {
        //act
        var response = await _httpClient.GetAsync($"ProjectTask/{Guid.NewGuid().ToString()}");
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Fail get by id with auth user but not access")]
    public async Task FailGetByIdWithAuthUserButNotAccess()
    {
        //create project and task
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailGetByIdWithAuthUserButNotAccess");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        var taskPostResponse = await _projectHelper.CreateTaskAsync(project.Id,"FailDeleteWithAuthUserAndNonAccess", user.Id);
        var task = await taskPostResponse.Content.ReadFromJsonAsync<ProjectTaskReadDTO>();

        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        
        //act
        var response = await _httpClient.GetAsync($"ProjectTask/{task.Id.ToString()}");
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}