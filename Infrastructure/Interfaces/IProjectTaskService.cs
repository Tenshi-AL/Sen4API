using Domain.Models;
using Infrastructure.DTO;
using Infrastructure.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Infrastructure.Interfaces;

public interface IProjectTaskService
{
    Task<ProjectTaskReadDTO?> Create(ProjectTaskWriteDTO projectWriteDto);
    Task<ProjectTaskReadDTO> Get(Guid id);
    Task<ProjectTaskReadDTO?> Update(Guid id, ProjectTaskWriteDTO projectTask);
    Task<ProjectTaskReadDTO?> Patch(Guid id, JsonPatchDocument<ProjectTaskWriteDTO> projectTask);
    Task Delete(Guid id);
    Task<PaginatedList<ProjectTaskReadDTO>> List(ProjectTaskListRequest projectTaskListRequest);
}