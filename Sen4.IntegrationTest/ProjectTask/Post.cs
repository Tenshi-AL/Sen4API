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

public class Post: TestBase
{
    public Post(IntegrationTestWebAppFactory integrationTestWebAppFactory): base(integrationTestWebAppFactory)
    {
        //register main user
        _authorizationHelper.RegisterUserAsync("admin@gmail.com","Alex","Hlushko","Olegovich",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
        //register guest user
        _authorizationHelper.RegisterUserAsync("guest@gmail.com","John","Doe","Ragnarson",new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),"StrongPassword_Kj8_Dn3456_ty5&");
    }
    
    
    [Fact(DisplayName = "Success create task with valid data and access")]
    public async Task SuccessCreateTaskWithValidDataAndAccess()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        
        //act
        var project = await _projectHelper.CreateProjectAsync("SuccessCreateTaskWithValidDataAndAccess");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        var taskPostResponse = await _projectHelper.CreateTaskAsync(project.Id,"FailDeleteWithAuthUserAndNonAccess", user.Id);
        var task = await taskPostResponse.Content.ReadFromJsonAsync<ProjectTaskReadDTO>();
        
        //assert
        taskPostResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        task.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Fail post with repeated body and request id")]
    public async Task FailPostWithRepeatedBodyAndRequestId()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailPostWithRepeatedBodyAndRequestId");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        await _projectHelper.CreateTaskAsync(project.Id,"FailPostWithRepeatedBodyAndRequestId", user.Id);
        
        //act
        var response = await _projectHelper.CreateTaskAsync(project.Id,"FailPostWithRepeatedBodyAndRequestId", user.Id);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "Success post with repeated request id and new body")]
    public async Task SuccessPostWithRepeatedRequestIdAndNewBody()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessPostWithRepeatedRequestIdAndNewBody");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        await _projectHelper.CreateTaskAsync(project.Id,"Test task 1", user.Id);
        
        //act
        var response = await _projectHelper.CreateTaskAsync(project.Id,"Test task 2", user.Id);
        var result = await response.Content.ReadFromJsonAsync<ProjectReadDTO>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        result.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Success post with repeated body and new request id")]
    public async Task SuccessPostWithRepeatedBodyAndNewRequestId()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessPostWithRepeatedBodyAndNewRequestId");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        await _projectHelper.CreateTaskAsync(project.Id,"Test task 1", user.Id);
        
        //act
        _httpClient.DefaultRequestHeaders.Remove("requestId");
        _httpClient.DefaultRequestHeaders.Add("requestId", Guid.NewGuid().ToString());
        var response = await _projectHelper.CreateTaskAsync(project.Id,"Test task 1", user.Id);
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
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailPostWithInvalidRequestId");
        _httpClient.DefaultRequestHeaders.Remove("requestId");
        _httpClient.DefaultRequestHeaders.Add("requestId", requestId);
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        
        //act
        var response = await _projectHelper.CreateTaskAsync(project.Id,"Test task 1", user.Id);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Fail create task with valid project, access and invalid body")]
    public async Task FailCreateTaskWithValidProjectAccessAndInvalidBody()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailCreateTaskWithValidProjectAccessAndInvalidBody");
        
        //create task
        var response = await _httpClient.PostAsJsonAsync("ProjectTask", new ProjectTaskWriteDTO()
        {
            Name = "test task",
            Description = null,
            TaskStatusId = Guid.NewGuid(),
            UserExecutorId = Guid.NewGuid(),
            UserCreatedId = Guid.NewGuid(),
            PriorityId = Guid.NewGuid(),
            ProjectId = project.Id
        });
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Fail create task with valid body but user is not in project")]
    public async Task FailCreateTaskWithValidBodyButUserIsNotInProject()
    {
        //arrange
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailCreateTaskWithValidBodyButUserIsNotInProject");
        
        //act
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        _httpClient.DefaultRequestHeaders.Remove("requestId");
        _httpClient.DefaultRequestHeaders.Add("requestId", new Guid().ToString());
        var response = await _projectHelper.CreateTaskAsync(project.Id,"Test task 1", user.Id);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Fail create task with valid body and new user without access")]
    public async Task FailCreateTaskWithValidBodyAndNewUserWithoutAccess()
    {
        //login as admin, create project and generate invite token
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("FailCreateTaskWithValidBodyAndNewUserWithoutAccess");
        var postResponse = await _httpClient.PostAsJsonAsync("Project/:generateInviteToken", project.Id.ToString());
        var token = await postResponse.Content.ReadAsStringAsync();
        
        //login as new user and join to project
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        await _httpClient.PostAsJsonAsync("Project/:join", token);
        
        //create task
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        var response = await _projectHelper.CreateTaskAsync(project.Id,"Test task 1", user.Id);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Success create task with valid body and new user with access")]
    public async Task SuccessCreateTaskWithValidBodyAndNewUserWithAccess()
    {
        //login as admin, create project and generate invite token
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessCreateTaskWithValidBodyAndNewUserWithAccess");
        var postResponse = await _httpClient.PostAsync($"Project/:generateInviteToken/{project.Id.ToString()}", null);
        var token = await postResponse.Content.ReadAsStringAsync();
        
        //login as new user and join to project
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        await _httpClient.PostAsync($"Project/:join/{token}", null);
        
        //login as admin, create rule for new user
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");

        var operation = await _sen4Context.Operations.FirstOrDefaultAsync(p => p.Controller == "ProjectTask" && p.Action == "Post");
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
        
        //login as new user and check post
        await _authorizationHelper.LoginUserAsync("guest@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var user = await _userManager.FindByEmailAsync("admin@gmail.com");
        var response = await _projectHelper.CreateTaskAsync(project.Id,"Test task 1", user.Id);
        var result = await response.Content.ReadFromJsonAsync<ProjectTaskReadDTO>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        result.ShouldNotBeNull();
    }
    
    [Fact(DisplayName = "Fail create task with invalid creator and executor")]
    public async Task FailCreateTaskWithInvalidCreatorAndExecutor()
    {
        //arrabge
        await _authorizationHelper.LoginUserAsync("admin@gmail.com", "StrongPassword_Kj8_Dn3456_ty5&");
        var project = await _projectHelper.CreateProjectAsync("SuccessCreateTaskWithValidBodyAndNewUserWithAccess");
        
        //act
        var response = await _httpClient.PostAsJsonAsync("ProjectTask", new ProjectTaskWriteDTO()
        {
            Name = "test task",
            Description = null,
            TaskStatusId = Guid.Parse("8f2da16a-0d31-4585-bacf-118135fe4dcd"),
            UserExecutorId = new Guid(),
            UserCreatedId = new Guid(),
            PriorityId = Guid.Parse("8f2da16a-0d31-4585-bacf-118135fe4dcd"),
            ProjectId = project.Id
        });
        var result = await response.Content.ReadFromJsonAsync<ProjectReadDTO>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
    }
}