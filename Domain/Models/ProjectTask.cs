using System.Collections;

namespace Domain.Models;

public class ProjectTask
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    public Guid TaskStatusId { get; set; }
    public TaskStatus TaskStatus { get; set; } = null!;
    
    public Guid UserCreatedId { get; set; }
    public User UserCreated { get; set; } = null!;
    
    public Guid UserExecutorId { get; set; }
    public User UserExecutor { get; set; } = null!;

    public IList<TaskFile>? TaskFiles { get; set; }
    public Guid PriorityId { get; set; }
    public Priority Priority { get; set; } = null!;
    
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public DateTime CreatedDate { get; set; }
    public DateTime DeadlineDate { get; set; }
}