using Domain.Models;
using Infrastructure.DTO;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;

namespace Infrastructure.Interfaces;

public interface IUserService
{
    Task<List<PostReadDTO>> GetPosts();
    Task<Guid?> PatchUpdate(Guid id, JsonPatchDocument<UserUpdateDTO> userUpdateDto);
    Task<UserReadDTO?> GetUserById(Guid id);
    Task<UserReadDTO?> GetUserByEmail(string email);
    Task<PaginatedList<UserReadDTO>> List(UserListRequest userListRequest);
    int Count();
}