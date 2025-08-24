
namespace Infrastructure.Models;

public class ProjectTaskListRequest
{
    public Guid? ProjectStatusId { get; set; }
    public Guid? UserCreatedId { get; set; }
    public Guid? UserExecutorId { get; set; }
    public Guid ProjectId { get; set; }
    public string? TaskName { get; set; }
    
    public string? SortProperty { get; set; } = null;
    public bool SortByDescending { get; set; } = true;
    
    public int? PageSize { get; set; }
    public int? PageNumber { get; set; }
}