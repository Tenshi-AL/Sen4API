using Domain.Models;

namespace Infrastructure.DTO;

public class ProjectStatusReadDTO
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}