namespace Infrastructure.DTO;

public class UserReadDTO
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; } 
    public required string Surname { get; init; }
    public required string MiddleName { get; init; }
    
    public string? AdditionalEmail { get; init; }
    public string? Telegram { get; init; }
    public string? Instagram { get; init; }
    public string? Facebook { get; init; }
    public string? Viber { get; init; }
    public string? AboutMyself { get; init; }
}