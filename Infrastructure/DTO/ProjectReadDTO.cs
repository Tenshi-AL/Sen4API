namespace Infrastructure.DTO;

public class ProjectReadDTO
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required DateTime CreatedDateTime { get; init; }
    public string? Description { get; init; }
    public required string Priority { get; init; }
    public DateTime? DeletedBy { get; init; }
}