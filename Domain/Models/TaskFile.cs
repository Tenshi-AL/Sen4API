namespace Domain.Models;

public class TaskFile
{
    public Guid Id { get; set; }
    public string FilePath { get; set; } = null!;
    
    public Guid TaskId { get; set; }
    public ProjectTask Task { get; set; } = null!;
}