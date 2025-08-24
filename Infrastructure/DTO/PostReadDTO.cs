using Domain.Models;

namespace Infrastructure.DTO;

public class PostReadDTO
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
}