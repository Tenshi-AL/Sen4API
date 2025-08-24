using AutoMapper;
using Domain.Models;
using Infrastructure.DTO;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Services;

public class RuleService(Sen4Context db, IMapper mapper): IRuleService
{
    public async Task<List<RuleDTO>> Rules(Guid projectId, Guid userId)
    {
        var rules = await db.Rules
            .Include(p => p.Operation)
            .Include(p => p.UsersProjects)
            .Where(p => p.UsersProjects.ProjectId == projectId &&
                        p.UsersProjects.UserId == userId)
            .ToListAsync();

        return mapper.Map<List<RuleDTO>>(rules);
    }
    
    public async Task<bool> SetRules(Guid userId, Guid projectId, List<RuleDTO> rules)
    {
        var userProject = await db.UsersProjects
            .Include(p=>p.Rules)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.ProjectId == projectId);
        if (userProject is null) return false;
        
        await using var transaction =  await db.Database.BeginTransactionAsync();
        try
        {
            var oldRules = userProject.Rules;
            db.Rules.RemoveRange(oldRules);
            
            var distinctList = rules.DistinctBy(p => p.OperationId);
            foreach (var rule in distinctList)
            {
                userProject.Rules.Add(new Rule()
                {
                    OperationId = rule.OperationId,
                    Access = rule.Access
                });
            }

            await transaction.CommitAsync();
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw new ProjectServiceException(e.Message);
        }
    }
}