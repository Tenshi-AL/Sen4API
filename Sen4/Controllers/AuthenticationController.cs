using System.Security.Claims;
using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Sen4.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController(ISen4AuthService authenticationService): ControllerBase
{
    /// <summary>
    /// User registration
    /// </summary>
    /// <param name="userRegistrationDto">User information</param>
    /// <response code="200">Success registration</response>
    /// <response code="400">Registration failure</response>
    [HttpPost(":register")]
    [ProducesResponseType(typeof(IdentityResult),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody]UserRegistrationDTO userRegistrationDto)
    {
        var result = await authenticationService.Register(userRegistrationDto);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Login
    /// </summary>
    /// <param name="loginDto">Login data</param>
    /// <response code="200">Success login</response>
    /// <response code="400">Login failure</response>
    [HttpPost(":login")]
    [ProducesResponseType(typeof(RefreshAccessToken),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
    {
        var result = await authenticationService.Login(loginDto);
        return result is not null ? Ok(result) : Unauthorized();
    }
    
    /// <summary>
    /// Refresh token
    /// </summary>
    /// <param name="refreshAccessToken">User access and refresh token</param>
    /// <response code="200">Success refresh</response>
    /// <response code="401">Failure</response>
    [Authorize]
    [HttpPost(":refresh")]
    [ProducesResponseType(typeof(string),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshAccessToken refreshAccessToken)
    {
        var result = await authenticationService.RefreshToken(refreshAccessToken);
        return result is not null ? Ok(result) : Unauthorized();
    }

    /// <summary>
    /// Login by google
    /// </summary>
    /// <response code="200">Success login</response>
    /// <response code="401">Login failure</response>
    [Authorize(AuthenticationSchemes = GoogleDefaults.AuthenticationScheme)]
    [HttpGet(":google-auth")]
    [ProducesResponseType(typeof(RefreshAccessToken),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleAuth()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Name);
        if (userEmail is null) return Unauthorized();
        var result = await authenticationService.GoogleAuth(userEmail);
        return result is null ? Unauthorized() : Ok(result);
    }
    
    
    /// <summary>
    /// Method not implemented
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
    public IActionResult List() => StatusCode(StatusCodes.Status405MethodNotAllowed);
    
    /// <summary>
    /// Method not implemented
    /// </summary>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
    public IActionResult Patch() => StatusCode(StatusCodes.Status405MethodNotAllowed);
    
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