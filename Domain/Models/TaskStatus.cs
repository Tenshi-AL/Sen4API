using System.Collections;

namespace Domain.Models;

public class TaskStatus
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public IList<ProjectTask> ProjectTasks = new List<ProjectTask>();
}