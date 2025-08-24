using System.ComponentModel;
using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sen4.Authorizations;
using Sen4.Filters;
using Sen4.Hubs;

namespace Sen4.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ProjectTaskController(IAuthorizationService authorizationService,IProjectService projectService, 
    IProjectTaskService projectTaskService, IHttpContextAccessor httpContextAccessor, IUserService userService, IHubContext<NotificationHub> notificationHub): ControllerBase
{
    /// <summary>
    /// Get project task by task id
    /// </summary>
    /// <param name="id">Task id</param>
    /// <response code="200">Success</response>
    /// <response code="400">Fail</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("{id}")]
    [Description("This method return task information.")]
    [ProducesResponseType(typeof(ProjectTaskReadDTO),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get(Guid id)
    {
        var task = await projectTaskService.Get(id);
        var authorizationResult = await authorizationService.AuthorizeAsync(User, task.ProjectId, APIOperations.TaskGet);
        if (!authorizationResult.Succeeded) return Forbid();
        
        var result = await projectTaskService.Get(id);
        return Ok(result);
    }
    
    /// <summary>
    /// Create task
    /// </summary>
    /// <param name="body">Task body</param>
    /// <param name="requestId">Idempotency key in Guid format</param>
    /// <response code="201">Success</response>
    /// <response code="400">Fail</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="409">Duplicate request</response>
    [HttpPost]
    [Description("This method create new task.")]
    [Idempotent(nameof(body), nameof(requestId))]
    [ProducesResponseType(typeof(ProjectTaskReadDTO),StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Post([FromBody] ProjectTaskWriteDTO body, [FromHeader] Guid requestId)
    {
        var authorizationResult = await authorizationService.AuthorizeAsync(User, body.ProjectId, APIOperations.TaskPost);
        if (!authorizationResult.Succeeded) return Forbid();
        
        var result = await projectTaskService.Create(body);
        if (result is not null)
        {
            await notificationHub.Clients.User(body.UserCreatedId.ToString()).SendAsync("notification", "Success create task!");
            await notificationHub.Clients.User(body.UserExecutorId.ToString()).SendAsync("notification", "You have a new task!"); 
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        return BadRequest();
    }

    /// <summary>
    /// Method not implemented
    /// </summary>
    [HttpPut("{id}")]
    [Description("This method edit task.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Put(Guid id, [FromBody] ProjectTaskWriteDTO projectTaskWriteDto)
    {
        var task = await projectTaskService.Get(id);
        var authorizationResult = await authorizationService.AuthorizeAsync(User, task.ProjectId, APIOperations.TaskPut);
        if (!authorizationResult.Succeeded) return Forbid();
        
        var result = await projectTaskService.Update(id, projectTaskWriteDto);
        if (result is not null)
        {
            await notificationHub.Clients.User(result.UserCreatedId.ToString()).SendAsync("notification", "Success update task!");
            await notificationHub.Clients.User(result.UserExecutorId.ToString()).SendAsync("notification", "Task has been updated!");
            return Ok(result);
        }
        return NotFound();
    }

    /// <summary>
    /// Patch project task
    /// </summary>
    /// <param name="id">Task id</param>
    /// <param name="jsonPatchDocument">Patch body</param>
    /// <response code="200">Success</response>
    /// <response code="404">Not found task by id</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="409">Duplicate request</response>
    [HttpPatch("{id}")]
    [Description("This method edit task")]
    [Idempotent(nameof(jsonPatchDocument), nameof(requestId))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Patch(Guid id, [FromBody]JsonPatchDocument<ProjectTaskWriteDTO> jsonPatchDocument,[FromHeader] Guid requestId)
    {
        var task = await projectTaskService.Get(id);
        var authorizationResult = await authorizationService.AuthorizeAsync(User, task.ProjectId, APIOperations.TaskPatch);
        if (!authorizationResult.Succeeded) return Forbid();
        
        var result = await projectTaskService.Patch(id, jsonPatchDocument);
        if (result is not null)
        {
            await notificationHub.Clients.User(result.UserCreatedId.ToString()).SendAsync("notification", "Success update task!");
            await notificationHub.Clients.User(result.UserExecutorId.ToString()).SendAsync("notification", "Task has been updated!");
            return Ok(result);
        }
        return NotFound();
    }

    /// <summary>
    /// Delete task by id
    /// </summary>
    /// <param name="id">Task id</param>
    /// /// <response code="200">Success</response>
    /// /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpDelete("{id}")]
    [Description("This method archives task.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var task = await projectTaskService.Get(id);
        var authorizationResult = await authorizationService.AuthorizeAsync(User, task.ProjectId, APIOperations.TaskDelete);
        if (!authorizationResult.Succeeded) return Forbid();
        
        await projectTaskService.Delete(id);
        
        await notificationHub.Clients.User(task.UserCreatedId.ToString()).SendAsync("notification", "Success remove task!");
        await notificationHub.Clients.User(task.UserExecutorId.ToString()).SendAsync("notification", $"Task {task.Name} has been deleted!"); 
        return Ok();
    }

    /// <summary>
    /// Get task list
    /// </summary>
    /// <param name="projectTaskListRequest">Query params</param>
    /// /// <response code="200">Success</response>
    /// /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("List")]
    [Description("This method return task list.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List([FromQuery] ProjectTaskListRequest projectTaskListRequest)
    {
        var authorizationResult = await authorizationService.AuthorizeAsync(User, projectTaskListRequest.ProjectId, APIOperations.TaskGet);
        if (!authorizationResult.Succeeded) return Forbid();
        
        var list = await projectTaskService.List(projectTaskListRequest);
        return Ok(list);
    }
}