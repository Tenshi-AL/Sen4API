using Domain.Models;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extension;

public static class UserListExtension
{
    public static IQueryable<User> Filtration(this IQueryable<User> users, Guid? positionId, Guid? projectId)
    {
        if (positionId is not null)
            users = users.Where(p => p.PostId == positionId);
        if(projectId is not null)
            users = users.Where(p => p.Projects.Any(p => p.Id == projectId));
        return users;
    }
    
    public static IQueryable<User> Sort(this IQueryable<User> users, string? sortProperty, bool sortByDescending = true)
    {
        return users = sortProperty?.ToLower() switch
        {
            "name" => sortByDescending ? users.OrderByDescending(p=>p.Name): users.OrderBy(p=>p.Name),
            "surname" => sortByDescending ? users.OrderByDescending(p=>p.Surname): users.OrderBy(p=>p.Surname),
            "middleName" => sortByDescending ? users.OrderByDescending(p=>p.MiddleName): users.OrderBy(p=>p.MiddleName),
            "post" => sortByDescending ? users.OrderByDescending(p=>p.Post.Title): users.OrderBy(p=>p.Post.Title),
            _ => users
        };
    }

    public static IQueryable<User> Pagination(this IQueryable<User> users, int? pageSize, int? page)
    {
        if (pageSize is null || page is null) return users;
        
        if (page <0) throw new ArgumentOutOfRangeException(nameof(page),"Page number must be greater 0");
        if (pageSize <0) throw new ArgumentOutOfRangeException(nameof(pageSize),"Page size must be greater 0");
        
        return users.Skip(((int)page - 1)* (int)pageSize).Take((int)pageSize);
    }
}