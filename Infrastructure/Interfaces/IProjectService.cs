using Domain.Models;
using Infrastructure.DTO;
using Infrastructure.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Infrastructure.Interfaces;

public interface IProjectService
{
    Task<bool> JoinToProject(string token, string userEmail);
    string GenerateInviteToken(Guid projectId);
    int ProjectCount();
    Task<Guid?> Archiving(Guid id);
    Task<ProjectReadDTO> Get(Guid id);
    Task<ProjectReadDTO> Create(ProjectWriteDTO projectWriteDto, Guid userId);
    Task<Guid?> PatchUpdate(Guid id, JsonPatchDocument<ProjectWriteDTO> projectUpdateDto);
    Task<PaginatedList<ProjectReadDTO>> List(ProjectListRequest projectListRequest);
}