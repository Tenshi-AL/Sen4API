using TaskStatus = Domain.Models.TaskStatus;

namespace Infrastructure.DTO;

public class TaskStatusReadDTO
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}