using System.Collections;

namespace Domain.Models;

public class Priority
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public IList<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
}