using System.Security.Claims;
using Domain.Models;
using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Sen4.Controllers;

/*  The essence of the problem is in the authorization of specific methods,
    for example Get and Patch should be available only to authorized 
    users and only if the user id matches the current id, so that the current user can change only his own data, 
    and not some random person.
    For now, I will leave it as is because it is needed for purposes, rather testing.   */
[ApiController]
[Route("[controller]")]
public class UserController(IUserService userService): ControllerBase
{
    /// <summary>
    /// Get user by id
    /// </summary>
    /// <param name="id">User id</param>
    /// <response code="200">Success</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserReadDTO),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await userService.GetUserById(id);
        return Ok(result);
    }
    
    /// <summary>
    /// Get current auth user data
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(UserReadDTO),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Name);
        var result = await userService.GetUserByEmail(userEmail!);
        return Ok(result);
    }
    
    /// <summary>
    /// Patch user by id
    /// </summary>
    /// <param name="id">User id</param>
    /// <param name="userUpdateDto">User data</param>
    /// <response code="200">Success patch</response>
    /// <response code="404">User by id not found</response>
    /// <response code="400">Error in json patch document</response>
    /// <response code="401">Unauthorized</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(Guid),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Patch(Guid id,JsonPatchDocument<UserUpdateDTO> userUpdateDto)
    {
        var result = await userService.PatchUpdate(id, userUpdateDto);
        return result is not null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Get users posts
    /// </summary>
    /// /// <response code="200">Success patch</response>
    /// /// <response code="401">Unauthorized</response>
    [HttpGet(":posts")]
    [ProducesResponseType(typeof(List<PostReadDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Posts()
    {
        var result = await userService.GetPosts();
        return Ok(result);
    }
    
    /// <summary>
    /// Get user list
    /// </summary>
    /// <param name="userListRequest">Request parameters</param>
    /// <response code="201">Success</response>
    /// <response code="401">Unauthorized</response>
    
    [HttpGet("List")]
    [ProducesResponseType(typeof(List<UserReadDTO>),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List([FromQuery]UserListRequest userListRequest)
    {
        var result = await userService.List(userListRequest);
        return Ok(result);
    }
    /// <summary>
    /// Method not implemented
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
    public IActionResult Post() => StatusCode(StatusCodes.Status405MethodNotAllowed);
    
    /// <summary>
    /// Method not implemented
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
    public IActionResult Put() => StatusCode(StatusCodes.Status405MethodNotAllowed);
    
    /// <summary>
    /// Method not implemented
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
    public IActionResult Delete() => StatusCode(StatusCodes.Status405MethodNotAllowed);
}