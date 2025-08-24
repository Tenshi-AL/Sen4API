using System.ComponentModel;
using System.Security.Claims;
using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sen4.Authorizations;
using Sen4.Hubs;
using Infrastructure.Models;

namespace Sen4.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class RuleController(IRuleService operationService, IProjectService projectService, 
    IAuthorizationService authorizationService, IHubContext<NotificationHub> notificationHub): ControllerBase
{
    /// <summary>
    /// Set user rules in project
    /// </summary>
    /// <param name="ruleModel">Rule body</param>
    /// <response code="200">Success</response>
    /// <response code="401">Authorization error</response>
    /// <response code="403">Forbidden</response>
    /// <response code="400">BadRequest</response>
    [HttpPost]
    [Description("This method allows you to assign rules to users.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] SetRuleModel ruleModel)
    {
        var authorizationResult = await authorizationService.AuthorizeAsync(User, ruleModel.ProjectId, APIOperations.TaskGet);
        if (!authorizationResult.Succeeded) return Forbid();
        
        var result = await operationService.SetRules(ruleModel.UserId, ruleModel.ProjectId, ruleModel.Rules);
        if (result)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await notificationHub.Clients.User(userId!).SendAsync("notification", "Success update rules!");
            await notificationHub.Clients.User(ruleModel.UserId.ToString()).SendAsync("notification", $"You rules has been updated");
            return Ok();
        }
        return BadRequest();
    }
    
    [HttpGet("List")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Description("This method allows you to view the list of user rules in the project.")]
    public async Task<IActionResult> List([FromQuery]Guid projectId, [FromQuery]Guid userId)
    {
        var result = await operationService.Rules(projectId, userId);
        return Ok(result);
    }
    
    /// <summary>
    /// Method not implemented
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
    public IActionResult Get()
    {
        return StatusCode(StatusCodes.Status405MethodNotAllowed);
    }
    
    /// <summary>
    /// Method not implemented
    /// </summary>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
    public async Task<IActionResult> Patch(Guid userId, Guid projectId, JsonPatchDocument<List<RuleDTO>> jsonPatchDocument)
    {
        return StatusCode(StatusCodes.Status405MethodNotAllowed);
    }
    
    /// <summary>
    /// Method not implemented
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
    public IActionResult Delete()
    {
        return StatusCode(StatusCodes.Status405MethodNotAllowed);
    }
}