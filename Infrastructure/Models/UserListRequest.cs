
namespace Infrastructure.Models;

public class UserListRequest
{
    public Guid? ProjectId { get; set; } = null;
    public Guid? PositionId { get; set; } = null;
    
    public string? SortProperty { get; set; } = null;
    public bool SortByDescending { get; set; } = true;
    public int? PageSize { get; set; }
    public int? PageNumber { get; set; }
}