using Infrastructure.DTO;
using Microsoft.AspNetCore.JsonPatch;

namespace Infrastructure.Interfaces;

public interface IRuleService
{
    Task<List<RuleDTO>> Rules(Guid projectId, Guid userId);
    Task<bool> SetRules(Guid userId, Guid projectId, List<RuleDTO> rules);
}