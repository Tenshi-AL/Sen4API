using Domain.Models;
using Infrastructure.DTO;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extension;

public static class ProjectListExtension
{
    public static IQueryable<Project> Filtration(this IQueryable<Project> projects, Guid? id, string? name, bool showDeleted)
    {
        if(id is not null)
            projects = projects.Where(p => p.Users.Any(user => user.Id == id));
        if(name is not null)
            projects = projects.Where(p => EF.Functions.Like(p.Name, $"%{name}%"));
        
        return showDeleted ? projects.Where(p => p.DeletedBy != null) : projects.Where(p => p.DeletedBy == null);
    }
    
    public static IQueryable<Project> Sort(this IQueryable<Project> projects, string? sortProperty, bool sortByDescending = true)
    {
        return sortProperty?.ToLower() switch
        {
            "name" => sortByDescending ? projects.OrderByDescending(p => p.Name) : projects.OrderBy(p => p.Name),
            "createddatetime" => sortByDescending
                ? projects.OrderByDescending(p => p.CreatedDateTime)
                : projects.OrderBy(p => p.CreatedDateTime),
            _ => projects
        };
    }
    
    public static IQueryable<Project> Pagination(this IQueryable<Project> projects, int? pageSize, int? page)
    {
        if (pageSize is null || page is null) return projects;
        
        if (page <0) throw new ArgumentOutOfRangeException(nameof(page),"Page number must be greater 0");
        if (pageSize <0) throw new ArgumentOutOfRangeException(nameof(pageSize),"Page size must be greater 0");
        
        return projects.Skip(((int)page - 1)* (int)pageSize).Take((int)pageSize);
    }
}