namespace Domain.Models;

public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    
    public IList<User> Users { get; set; } = new List<User>();
}