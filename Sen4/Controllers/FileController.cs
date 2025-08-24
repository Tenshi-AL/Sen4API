using System.ComponentModel;
using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minio.DataModel;
using Sen4.Authorizations;

namespace Sen4.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class FileController(IAuthorizationService authorizationService, IFileService fileService): ControllerBase
{
    /// <summary>
    /// Get file by file name.
    /// </summary>
    /// <param name="objectName">file name</param>
    /// <param name="bucketName">Bucket name</param>
    /// <response code="200">Success</response>
    /// <response code="400">Bad request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet()]
    [Description("This method return file.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get(string objectName, string bucketName)
    {
        var authorizationResult = await authorizationService.AuthorizeAsync(User, new Guid(bucketName), APIOperations.FileGet);
        if (!authorizationResult.Succeeded) return Forbid();
        
        var result = await fileService.GetObject(objectName, bucketName);
        return result is null ? BadRequest() : File(result, "application/octet-stream", objectName);
    }
    
    /// <summary>
    /// Delete file by filename
    /// </summary>
    /// <param name="objectName">File name</param>
    /// <param name="bucketName">Bucket name</param>
    /// <response code="200">Success</response>
    /// <response code="400">Bad request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpDelete("{objectName}/{bucketName}")]
    [Description("This method delete file.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(string objectName, string bucketName)
    {
        var authorizationResult = await authorizationService.AuthorizeAsync(User, new Guid(bucketName), APIOperations.FileDelete);
        if (!authorizationResult.Succeeded) return Forbid();
        
        await fileService.RemoveObject(objectName, bucketName);
        return Ok();
    }
    
    /// <summary>
    /// Upload file in storage
    /// </summary>
    /// <param name="fileWriteDto"></param>
    /// <response code="200">Success</response>
    /// <response code="400">Bad request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpPost]
    [Description("This method upload file.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Post([FromForm] FileWriteDTO fileWriteDto)
    {
        var authorizationResult = await authorizationService.AuthorizeAsync(User, fileWriteDto.ProjectId, APIOperations.FilePost);
        if (!authorizationResult.Succeeded) return Forbid();
        
        var metaData = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            { "Project", fileWriteDto.ProjectId.ToString() },
        };
        if (fileWriteDto.TaskId is not null)
            metaData.Add("Task", fileWriteDto.TaskId.ToString());
        
        var result = await fileService.PutObject(fileWriteDto,metaData);
        return StatusCode((int)result.ResponseStatusCode);
    }
    
    /// <summary>
    /// Get file list from storage
    /// </summary>
    /// <param name="projectId">Project id</param>
    /// <param name="taskId">Task id</param>
    /// <response code="200">Success</response>
    /// <response code="400">Bad request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("List")]
    [Description("This method return file list.")]
    [ProducesResponseType(typeof(List<Item>),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(string? name, string projectId, string? taskId)
    {
        var authorizationResult = await authorizationService.AuthorizeAsync(User, new Guid(projectId), APIOperations.FileList);
        if (!authorizationResult.Succeeded) return Forbid();
        
        var result = await fileService.ListProjectsObject(
            name: name,
            projectId: projectId, 
            taskId: taskId);
        return Ok(result);
    }
    
    /// <summary>
    /// Method not implemented.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Put()
    {
        return StatusCode(StatusCodes.Status405MethodNotAllowed);
    }
    
    /// <summary>
    /// Method not implemented.
    /// </summary>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Patch()
    {
        return StatusCode(StatusCodes.Status405MethodNotAllowed);
    }
}