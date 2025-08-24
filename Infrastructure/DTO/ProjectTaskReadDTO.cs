namespace Infrastructure.DTO;

public class ProjectTaskReadDTO
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required Guid TaskStatusId { get; init; }
    public required string Status { get; init; }
    public required Guid UserCreatedId { get; init; }
    public required string UserCreated { get; init; }
    public required string UserCreatorEmail { get; init; }
    public required Guid UserExecutorId { get; init; }
    public required string UserExecutor { get; init; }
    public required string UserExecutorEmail { get; init; }
    public required Guid ProjectId { get; init; }
    public required DateTime CreatedDate { get; init; }
    public required DateTime DeadlineDate { get; init; }
    public required Guid PriorityId { get; init; }
    public required string Priority { get; init; }
}