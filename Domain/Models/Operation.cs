namespace Domain.Models;

public class Operation
{
    public Guid Id { get; set; }
    public string Controller { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string Description { get; set; } = null!;
    public IList<Rule> Rules { get; set; } = new List<Rule>();
}