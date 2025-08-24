using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Sen4.IntegrationTests.Project;

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
        
        //act
        var response = await _httpClient.DeleteAsync($"Project/{project.Id}");
        var result = await response.Content.ReadFromJsonAsync<Guid>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldBeEquivalentTo(project.Id);
    }

    [Fact(DisplayName = "Fail delete with non auth user")]
    public async Task FailDeleteWithNonAuthUser()
    {
        //act
        var response = await _httpClient.DeleteAsync($"Project/{Guid.NewGuid().ToString()}");
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Fail delete with auth user and non access")]
    public async Task FailDeleteWithAuthAndNonAccess()
    {
        //assert
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailDeleteWithAuthAndNonAccess");
        
        //act
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var response = await _httpClient.DeleteAsync($"Project/{project.Id}");
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Success delete with added user and added rule")]
    public async Task SuccessDeleteWithAddedUSerAndAddedRule()
    {
        //create project
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessDeleteWithAddedUSerAndAddedRule");
        
        //create invite token
        var postResponse = await _httpClient.PostAsync($"Project/:generateInviteToken/{project.Id.ToString()}", null);
        var token = await postResponse.Content.ReadAsStringAsync();
        
        //login as new user and join to project
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        await _httpClient.PostAsync($"Project/:join/{token}", null);
        
        //login as admin and add "delete" rule for new user
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");

        var operation = await _sen4Context.Operations.FirstOrDefaultAsync(p => p.Controller == "Project" && p.Action == "Delete");
        var userId = _userManager.FindByEmailAsync("guest@gmail.com").Result.Id;
        await _projectHelper.SetRuleAsync(userId, project.Id, operation.Id);
        
        //login as new user and check delete rule
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var response = await _httpClient.DeleteAsync($"Project/{project.Id}");
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [Fact(DisplayName = "Fail delete duplicate with auth user and access")]
    public async Task FailDeleteDuplicateWithAuthAndAccess()
    {
        //assert
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailDeleteDuplicateWithAuthAndAccess");
        await _httpClient.DeleteAsync($"Project/{project.Id}");
        
        //act
        var response = await _httpClient.DeleteAsync($"Project/{project.Id}");
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
}