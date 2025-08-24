using Domain.Models;
using Infrastructure.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Sen4.Authorizations;

public static class APIOperations
{
    public static OperationAuthorizationRequirement TaskGet =
        new OperationAuthorizationRequirement { Name = nameof(TaskGet) };
    public static OperationAuthorizationRequirement TaskPost =
        new OperationAuthorizationRequirement { Name = nameof(TaskPost) };
    public static OperationAuthorizationRequirement TaskPut =
        new OperationAuthorizationRequirement { Name = nameof(TaskPut) };
    public static OperationAuthorizationRequirement TaskPatch =
        new OperationAuthorizationRequirement { Name = nameof(TaskPatch) };
    public static OperationAuthorizationRequirement TaskDelete =
        new OperationAuthorizationRequirement { Name = nameof(TaskDelete) };
    
    //project operations
    public static OperationAuthorizationRequirement ProjectGet =
        new OperationAuthorizationRequirement { Name = nameof(ProjectGet) };
    public static OperationAuthorizationRequirement ProjectPatch =
        new OperationAuthorizationRequirement { Name = nameof(ProjectPatch) };
    public static OperationAuthorizationRequirement ProjectDelete =
        new OperationAuthorizationRequirement { Name = nameof(ProjectDelete) };
    public static OperationAuthorizationRequirement ProjectGenerateInviteToken =
        new OperationAuthorizationRequirement { Name = nameof(ProjectGenerateInviteToken) };
    public static OperationAuthorizationRequirement ProjectSetRules =
        new OperationAuthorizationRequirement { Name = nameof(ProjectSetRules) };
    public static OperationAuthorizationRequirement ProjectTaskList =
        new OperationAuthorizationRequirement { Name = nameof(ProjectTaskList) };
    public static OperationAuthorizationRequirement ProjectCreateTask =
        new OperationAuthorizationRequirement { Name = nameof(ProjectCreateTask) };
    
    //file operations
    public static OperationAuthorizationRequirement FileGet =
        new OperationAuthorizationRequirement { Name = nameof(FileGet) };
    public static OperationAuthorizationRequirement FileDelete =
        new OperationAuthorizationRequirement { Name = nameof(FileDelete) };
    public static OperationAuthorizationRequirement FilePost =
        new OperationAuthorizationRequirement { Name = nameof(FilePost) };
    public static OperationAuthorizationRequirement FileList =
        new OperationAuthorizationRequirement { Name = nameof(FileList) };
}

public class AuthorizationHandler(IHttpContextAccessor httpContextAccessor,
    Sen4Context db, 
    UserManager<User> userManager): AuthorizationHandler<OperationAuthorizationRequirement, Guid>
{
    private (string? controller, string? action) GetContextMetadata()
    {
        var endpoint = httpContextAccessor?.HttpContext?.GetEndpoint();
        var descriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
        var controller = descriptor?.ControllerName;
        var action = descriptor?.ActionName;
        return (controller, action);
    }
    
    private void CheckRules(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Guid projectId)
    {
        (string? controller, string? action) = GetContextMetadata();
        
        var currentUser =  userManager.FindByEmailAsync(context.User.Identity!.Name!).Result;
        if(currentUser is null) context.Fail();
        
        var userProject = db.UsersProjects
            .Include(p => p.Rules)
            .ThenInclude(p=>p.Operation)
            .FirstOrDefault(p => currentUser != null && p.ProjectId == projectId && p.UserId == currentUser.Id);
        if(userProject is null) context.Fail();
                
        var rules = userProject?.Rules;
        if(rules is null) context.Fail();
                
        if (rules != null && rules.Any(p => p.Operation.Controller == controller && p.Operation.Action == action && p.Access))
            context.Succeed(requirement);
    }
    
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Guid resource)
    {
        switch (requirement.Name)
        {
            //task operations
            case nameof(APIOperations.TaskGet): CheckRules(context, requirement, resource); break;
            case nameof(APIOperations.TaskDelete): CheckRules(context, requirement, resource); break;
            case nameof(APIOperations.TaskPatch): CheckRules(context, requirement, resource); break;
            case nameof(APIOperations.TaskPut): CheckRules(context, requirement, resource); break;
            case nameof(APIOperations.TaskPost): CheckRules(context, requirement, resource); break;
            
            //project operations
            case nameof(APIOperations.ProjectGet): CheckRules(context, requirement, resource); break;
            case nameof(APIOperations.ProjectPatch): CheckRules(context, requirement, resource); break;
            case nameof(APIOperations.ProjectDelete): CheckRules(context, requirement, resource); break;
            case nameof(APIOperations.ProjectGenerateInviteToken): CheckRules(context, requirement, resource); break;
            case nameof(APIOperations.ProjectSetRules): CheckRules(context, requirement, resource); break;
            case nameof(APIOperations.ProjectTaskList): CheckRules(context, requirement, resource); break;
            case nameof(APIOperations.ProjectCreateTask): CheckRules(context, requirement, resource); break;
            
            //files operations
            case nameof(APIOperations.FileGet): CheckRules(context, requirement, resource); break;
            case nameof(APIOperations.FileDelete): CheckRules(context, requirement, resource); break;
            case nameof(APIOperations.FilePost): CheckRules(context, requirement, resource); break;
            case nameof(APIOperations.FileList): CheckRules(context, requirement, resource); break;
        }
        return Task.CompletedTask;
    }
}