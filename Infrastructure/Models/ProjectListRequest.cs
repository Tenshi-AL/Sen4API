
namespace Infrastructure.Models;

public class ProjectListRequest
{
    public Guid? UserId { get; set; } = null;
    public bool ShowDeleted { get; set; } = false;
    public string? Name { get; set; }
    public string? SortProperty { get; set; } = null;
    public bool SortByDescending { get; set; } = true;
    public int? PageSize { get; set; }
    public int? PageNumber { get; set; }
}