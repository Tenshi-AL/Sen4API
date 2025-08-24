using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Domain.Models;
using Infrastructure.DTO;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Persistence;
using Shouldly;

namespace Sen4.IntegrationTests.ProjectTask;

public class Patch: TestBase
{
    public Patch(IntegrationTestWebAppFactory integrationTestWebAppFactory): base(integrationTestWebAppFactory)
    {
        //register main user
        _authorizationHelper.RegisterUserAsync("admin@gmail.com","Alex","Hlushko","Olegovich",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
        //register guest user
        _authorizationHelper.RegisterUserAsync("guest@gmail.com","John","Doe","Ragnarson",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
    }
    
    
    [Fact(DisplayName = "Success patch with auth user with access and valid body")]
    public async Task SuccessPatchWithAuthUserWithAccessAndValidBody()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessPatchWithAuthUserWithAccessAndValidBody");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        var taskPostResponse = await _projectHelper.CreateTaskAsync(project.Id,"FailDeleteWithAuthUserAndNonAccess", user.Id);
        var task = await taskPostResponse.Content.ReadFromJsonAsync<ProjectTaskReadDTO>();
        
        //act
        var jsonPatchDocument = new JsonPatchDocument<ProjectTaskWriteDTO>();
        jsonPatchDocument.Replace(p => p.Name, "new name");
        jsonPatchDocument.Replace(p => p.Description, "New description");
        
        var serializeDoc = JsonConvert.SerializeObject(jsonPatchDocument);
        var requestContent = new StringContent(serializeDoc, Encoding.UTF8, "application/json-patch+json");
        
        var response = await _httpClient.PatchAsync($"ProjectTask/{task.Id}",requestContent);
        var result = await response.Content.ReadAsStringAsync();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Fail patch with auth user without access")]
    public async Task FailPatchWithAuthUserWithoutAccess()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailPatchWithAuthUserWithoutAccess");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        var taskPostResponse = await _projectHelper.CreateTaskAsync(project.Id,"FailDeleteWithAuthUserAndNonAccess", user.Id);
        var task = await taskPostResponse.Content.ReadFromJsonAsync<ProjectTaskReadDTO>();
        
        //act
        var jsonPatchDocument = new JsonPatchDocument<ProjectTaskWriteDTO>();
        jsonPatchDocument.Replace(p => p.Name, "new name");
        jsonPatchDocument.Replace(p => p.Description, "New description");
        
        var serializeDoc = JsonConvert.SerializeObject(jsonPatchDocument);
        var requestContent = new StringContent(serializeDoc, Encoding.UTF8, "application/json-patch+json");

        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var response = await _httpClient.PatchAsync($"ProjectTask/{task.Id}",requestContent);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Success patch with new user with access")]
    public async Task SuccessPatchWithNewUserWithAccess()
    {
        //create project and task
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessPatchWithNewUserWithAccess");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        var taskPostResponse = await _projectHelper.CreateTaskAsync(project.Id,"FailDeleteWithAuthUserAndNonAccess", user.Id);
        var task = await taskPostResponse.Content.ReadFromJsonAsync<ProjectTaskReadDTO>();
        
        //create invite token
        var postResponse = await _httpClient.PostAsync($"Project/:generateInviteToken/{project.Id.ToString()}", null);
        var token = await postResponse.Content.ReadAsStringAsync();
        
        //login as new user and join to project
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        await _httpClient.PostAsync($"Project/:join/{token}", null);
        
        //login as admin and add "patch" rule for new user
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var operation = await _sen4Context.Operations.FirstOrDefaultAsync(p => p.Controller == "ProjectTask" && p.Action == "Patch");
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
        
        //login as new user and check patch rule
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var jsonPatchDocument = new JsonPatchDocument<ProjectTaskWriteDTO>();
        jsonPatchDocument.Replace(p => p.Name, "new name");
        jsonPatchDocument.Replace(p => p.Description, "New description");
        
        var serializeDoc = JsonConvert.SerializeObject(jsonPatchDocument);
        var requestContent = new StringContent(serializeDoc, Encoding.UTF8, "application/json-patch+json");

        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var response = await _httpClient.PatchAsync($"ProjectTask/{task.Id}",requestContent);
        var result = await response.Content.ReadAsStringAsync();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
    }
}