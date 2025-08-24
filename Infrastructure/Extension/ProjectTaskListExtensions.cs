using Domain.Models;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extension;

public static class ProjectTaskListExtensions
{
    public static IQueryable<ProjectTask> Filtered(this IQueryable<ProjectTask> list,
        ProjectTaskListRequest request)
    {
        list = list.Where(p => p.ProjectId == request.ProjectId);
        if (request.TaskName is not null)
            list = list.Where(p => EF.Functions.Like(p.Name, $"%{request.TaskName}%"));
        if (request.ProjectStatusId is not null)
            list = list.Where(p => p.TaskStatusId == request.ProjectStatusId);
        if (request.UserCreatedId is not null)
            list = list.Where(p => p.UserCreatedId == request.UserCreatedId);
        if (request.UserExecutorId is not null)
            list = list.Where(p => p.UserExecutorId == request.UserExecutorId);
        return list;
    }

    public static IQueryable<ProjectTask> Sort(this IQueryable<ProjectTask> list,
        string? sortProperty, bool sortByDescending = true)
    {
        return sortProperty?.ToLower() switch
        {
            "name" => sortByDescending ? list.OrderByDescending(p => p.Name) : list.OrderBy(p => p.Name),
            "projectstatus" => sortByDescending
                ? list.OrderByDescending(p => p.TaskStatus.Name)
                : list.OrderBy(p => p.TaskStatus.Name),
            "usercreated" => sortByDescending
                ? list.OrderByDescending(p => p.UserCreated.Surname)
                : list.OrderBy(p => p.UserCreated.Surname),
            "userexecutor" => sortByDescending
                ? list.OrderByDescending(p => p.UserExecutor.Surname)
                : list.OrderBy(p => p.UserExecutor.Surname),
            _ => list
        };
    }
    public static IQueryable<ProjectTask> Pagination(this IQueryable<ProjectTask> list, int? pageSize, int? page)
    {
        if (pageSize is null || page is null) return list;
        
        if (page <0) throw new ArgumentOutOfRangeException(nameof(page),"Page number must be greater 0");
        if (pageSize <0) throw new ArgumentOutOfRangeException(nameof(pageSize),"Page size must be greater 0");
        
        return list.Skip(((int)page - 1) * (int)pageSize).Take((int)pageSize);
    }
}