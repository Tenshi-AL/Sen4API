namespace Infrastructure.Models;

public class FileListRequest
{
    public string? Name { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? TaskId { get; set; }
}