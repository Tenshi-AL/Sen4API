using AutoMapper;
using Domain.Models;
using Infrastructure.DTO;
using Infrastructure.Extension;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Infrastructure.Exceptions;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence;

namespace Infrastructure.Services;

public class ProjectService(IConfiguration configuration,Sen4Context db, IMapper mapper, UserManager<User> userManager, ITokenService tokenService): IProjectService
{
    public string GenerateInviteToken(Guid projectId)
    {
        var claims = new List<Claim>() { new Claim(type: "ProjectId", value: projectId.ToString()) };
        var jwtSecret = AppConfiguration.GetRequiredConfigurationValue(configuration, "InviteJWT:Secret");
        var issuer = AppConfiguration.GetRequiredConfigurationValue(configuration, "InviteJWT:ValidIssuer");
        var audience = AppConfiguration.GetRequiredConfigurationValue(configuration, "InviteJWT:ValidAudience");
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(AppConfiguration.GetRequiredConfigurationValue(configuration, "InviteJWT:ValidTime")));

        var token = tokenService.CreateJwtToken(claims, jwtSecret, issuer, audience, expires);
        return token;
    }
    
    private ClaimsPrincipal ValidateToken(string token)
    {
        var jwtSecret = AppConfiguration.GetRequiredConfigurationValue(configuration, "InviteJWT:Secret");
        var validationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = new TimeSpan(0, 0, 5),
            ValidateLifetime = true
        };

        return tokenService.ValidateToken(token, validationParameters, jwtSecret);
    }

    public async Task<bool> JoinToProject(string token, string userEmail)
    {
        //validate token
        ClaimsPrincipal claimsPrincipal;
        try
        {
            claimsPrincipal = ValidateToken(token);
        }
        catch (Exception e)
        {
            return false;
        }
        
        //get project id
        var id = claimsPrincipal.Claims
            .Where(p => p.Type == "ProjectId")
            .Select(p => p.Value)
            .SingleOrDefault();
        if (id is null) return false;
        
        //search project by id
        var project = await db.Projects
            .Include(p => p.Users)
            .Where(p => p.Id == new Guid(id))
            .FirstOrDefaultAsync();
        if (project is null) return false;
        
        //get current user
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user is null) return false;

        //check if the user has already been added
        var usersProjects =
            await db.UsersProjects.FirstOrDefaultAsync(p => p.UserId == user.Id && p.ProjectId == project.Id);
        if (usersProjects is not null) return false;

        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        { 
            //add user to project
            var userProject = new UsersProjects()
            {
                UserId = user.Id,
                ProjectId = project.Id
            };
            db.UsersProjects.Add(userProject);
        
            //create max rules for user
            var operations = await db.Operations.ToListAsync();
            foreach (var operation in operations)
            {
                var rule = new Rule() { OperationId = operation.Id, Access = false };
                db.Rules.Add(rule);
                userProject.Rules.Add(rule);
            }
        
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw new ProjectServiceException(e.Message);
        }
    }
    
    public int ProjectCount() => db.Projects.Count();
    public async Task<Guid?> Archiving(Guid id)
    {
        var project = await db.Projects.FindAsync(id);
        if (project is null) return null;
        if (project.DeletedBy is not null) return null;
        
        project.DeletedBy = DateTime.UtcNow;
        db.Projects.Update(project);
        await db.SaveChangesAsync();
        
        return project.Id;
    }
    
    public async Task<ProjectReadDTO> Get(Guid id)
    {
        var project = await db.Projects
            .Where(p => p.Id == id).FirstOrDefaultAsync();
        return mapper.Map<ProjectReadDTO>(project);
    }
    
    public async Task<ProjectReadDTO> Create(ProjectWriteDTO projectWriteDto, Guid userId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            //create project
            var project = mapper.Map<Project>(projectWriteDto);
            db.Projects.Add(project);

            //attach current user to project
            var userProject = new UsersProjects() { UserId = userId, ProjectId = project.Id };
            db.UsersProjects.Add(userProject);

            //create max rules for user
            var operations = await db.Operations.ToListAsync();
            foreach (var operation in operations)
            {
                var rule = new Rule() { OperationId = operation.Id, Access = true };
                db.Rules.Add(rule);
                userProject.Rules.Add(rule);
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return mapper.Map<ProjectReadDTO>(project);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw new ProjectServiceException(e.Message);
        }
    }

    public async Task<Guid?> PatchUpdate(Guid id,JsonPatchDocument<ProjectWriteDTO> projectUpdateDto)
    {
        var project = await db.Projects.FindAsync(id);
        if (project is null) return null;

        var updateProject = mapper.Map<JsonPatchDocument<Project>>(projectUpdateDto);
        updateProject.ApplyTo(project);
        db.Projects.Update(project);
        await db.SaveChangesAsync();
        
        return project.Id;
    }

    public async Task<PaginatedList<ProjectReadDTO>> List(ProjectListRequest projectListRequest)
    {
        var filteredQuery = db.Projects
            .AsNoTracking()
            .Filtration(id: projectListRequest.UserId,
                name: projectListRequest.Name,
                showDeleted: projectListRequest.ShowDeleted);

        var projectsTotalCount = await filteredQuery.CountAsync();

        var projects = await filteredQuery
            .Sort(projectListRequest.SortProperty, projectListRequest.SortByDescending)
            .Pagination(projectListRequest.PageSize, projectListRequest.PageNumber)
            .ToListAsync();

        var mappedProjects = mapper.Map<List<ProjectReadDTO>>(projects);
        
        int? totalPage = (projectListRequest.PageNumber == null || projectListRequest.PageSize == null)
            ? null
            : (int)Math.Ceiling(projectsTotalCount / (double)projectListRequest.PageSize);

        return new PaginatedList<ProjectReadDTO>(
            list: mappedProjects,
            pageIndex: projectListRequest.PageNumber,
            totalPages: totalPage);
    }

}