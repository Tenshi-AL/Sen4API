namespace Domain.Models;

public class Rule
{
    public Guid Id { get; set; }
    public Guid OperationId { get; set; }
    public Operation Operation { get; set; }
    public bool Access { get; set; }
    
    public Guid UsersProjectsId { get; set; }
    public UsersProjects UsersProjects { get; set; }
}