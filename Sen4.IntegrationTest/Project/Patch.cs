using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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

namespace Sen4.IntegrationTests.Project;

public class Patch: TestBase
{
    public Patch(IntegrationTestWebAppFactory integrationTestWebAppFactory):base(integrationTestWebAppFactory)
    {
        //register main user
        _authorizationHelper.RegisterUserAsync("admin@gmail.com","Alex","Hlushko","Olegovich",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
        //register guest user
        _authorizationHelper.RegisterUserAsync("guest@gmail.com","John","Doe","Ragnarson",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
    }
    
    [Fact(DisplayName = "Success patch with valid body and request id")]
    public async Task SuccessPatchWithValidBodyAndRequestId()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessPatchWithValidBodyAndRequestId");
        
        //act
        var jsonPatchDocument = new JsonPatchDocument<ProjectWriteDTO>();
        jsonPatchDocument.Replace(p => p.Name, "new name");
        jsonPatchDocument.Replace(p => p.Description, "New description");
        
        var serializeDoc = JsonConvert.SerializeObject(jsonPatchDocument);
        var requestContent = new StringContent(serializeDoc, Encoding.UTF8, "application/json-patch+json");
        
        var response = await _httpClient.PatchAsync($"Project/{project.Id}",requestContent);
        var result = await response.Content.ReadAsStringAsync();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
    }
    
    [Fact(DisplayName = "Fail patch with valid body and user without access")]
    public async Task FailPatchWithValidBodyAndUSerWithoutAccess()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailPatchWithValidBodyAndUSerWithoutAccess");
        
        //act
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var jsonPatchDocument = new JsonPatchDocument<ProjectWriteDTO>();
        jsonPatchDocument.Replace(p => p.Name, "new name");
        jsonPatchDocument.Replace(p => p.Description, "New description");
        
        var serializeDoc = JsonConvert.SerializeObject(jsonPatchDocument);
        var requestContent = new StringContent(serializeDoc, Encoding.UTF8, "application/json-patch+json");
        
        var response = await _httpClient.PatchAsync($"Project/{project.Id}",requestContent);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Success patch with valid body and new user with access")]
    public async Task SuccessPatchWithValidBodyAndNewUserWithAccess()
    {
        //create project
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessPatchWithValidBodyAndNewUserWithAccess");
        
        //create invite token
        var postResponse = await _httpClient.PostAsync($"Project/:generateInviteToken/{project.Id.ToString()}", null);
        var token = await postResponse.Content.ReadAsStringAsync();
        
        //login as new user and join to project
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        await _httpClient.PostAsync($"Project/:join/{token}", null);
        
        //login as admin and add "patch" rule for new user
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var operation = await _sen4Context.Operations.FirstOrDefaultAsync(p => p.Controller == "Project" && p.Action == "Patch");
        var userId = _userManager.FindByEmailAsync("guest@gmail.com").Result.Id;
        await _projectHelper.SetRuleAsync(userId, project.Id, operation.Id);
        
        //login as new user and check patch rule
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var jsonPatchDocument = new JsonPatchDocument<ProjectWriteDTO>();
        jsonPatchDocument.Replace(p => p.Name, "new name");
        jsonPatchDocument.Replace(p => p.Description, "New description");
        
        var serializeDoc = JsonConvert.SerializeObject(jsonPatchDocument);
        var requestContent = new StringContent(serializeDoc, Encoding.UTF8, "application/json-patch+json");
        
        var response = await _httpClient.PatchAsync($"Project/{project.Id}",requestContent);
        var result = await response.Content.ReadAsStringAsync();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
    }
}