using AutoMapper;
using Domain.Models;
using Infrastructure.DTO;
using Infrastructure.Extension;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Services;

public class UserService(Sen4Context db, UserManager<User> userManager, IMapper mapper): IUserService
{
    public async Task<List<PostReadDTO>> GetPosts()
    {
        var list = await db.Posts.ToListAsync();
        return mapper.Map<List<PostReadDTO>>(list);
    }
    
    public async Task<UserReadDTO?> GetUserByEmail(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return mapper.Map<UserReadDTO>(user);
    }
    
    public async Task<Guid?> PatchUpdate(Guid id, JsonPatchDocument<UserUpdateDTO> userUpdateDto)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
            return null;
        
        var updateProject = mapper.Map<JsonPatchDocument<User>>(userUpdateDto);
        updateProject.ApplyTo(user);

        db.Users.Update(user);
        await db.SaveChangesAsync();
        
        return user.Id;
    }
    
    public async Task<UserReadDTO?> GetUserById(Guid id)
    {
        var user = mapper.Map<UserReadDTO>(await db.Users.FindAsync(id));
        return user;
    }
    
    public async Task<PaginatedList<UserReadDTO>> List(UserListRequest userListRequest)
    {
        var filteredQuery = db.Users
            .Include(user => user.Post)
            .Include(p => p.ResponsibilitiesTasks)
            .ThenInclude(p => p.Project)
            .AsNoTracking()
            .Filtration(positionId: userListRequest.PositionId, projectId: userListRequest.ProjectId);

        var usersTotalCount = await filteredQuery.CountAsync();

        var users = await filteredQuery
            .Sort(userListRequest.SortProperty, userListRequest.SortByDescending)
            .Pagination(userListRequest.PageSize, userListRequest.PageNumber)
            .ToListAsync();

        var mappedUsers = mapper.Map<List<UserReadDTO>>(users);

        int? totalPage = (userListRequest.PageNumber == null || userListRequest.PageSize == null)
            ? null
            : (int)Math.Ceiling(usersTotalCount / (double)userListRequest.PageSize);
        
        return new PaginatedList<UserReadDTO>(
            list: mappedUsers,
            pageIndex: userListRequest.PageNumber,
            totalPages: totalPage);
        
    }
    public int Count() => db.Users.Count();
}