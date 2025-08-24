namespace Infrastructure.DTO;

public class OperationReadDTO
{
    public required Guid Id { get; init; }
    public required string Controller { get; init; }
    public required string Action { get; init; }
    public required string Description { get; init; }
}