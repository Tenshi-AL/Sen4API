using Domain.Models;

namespace Infrastructure.DTO;

public class PriorityReadDTO
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}