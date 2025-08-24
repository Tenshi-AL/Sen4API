using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sen4.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class TaskStatusController(ITaskStatusService taskStatusService): ControllerBase
{
    /// <summary>
    /// Get task status list
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("List")]
    [ProducesResponseType(typeof(List<TaskStatusReadDTO>),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List()
    {
        var result = await taskStatusService.List();
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
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
    public IActionResult Post()
    {
        return StatusCode(StatusCodes.Status405MethodNotAllowed);
    }
    
    /// <summary>
    /// Method not implemented
    /// </summary>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
    public IActionResult Patch()
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