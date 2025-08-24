namespace Domain.Models;

public class UsersProjects
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; }
    public IList<Rule> Rules { get; set; } = new List<Rule>();
}