using System.Collections;

namespace Domain.Models;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedDateTime { get; set; }
    public string? Description { get; set; }
    public IList<User> Users { get; set; } = new List<User>();
    
    public IList<ProjectTask>? ProjectTasks { get; set; }
    public DateTime? DeletedBy { get; set; } = null;
}