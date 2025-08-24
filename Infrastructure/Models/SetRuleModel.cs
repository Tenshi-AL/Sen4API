using Infrastructure.DTO;

namespace Infrastructure.Models;

public class SetRuleModel
{
    public List<RuleDTO> Rules { get; set; } 
    public Guid UserId { get; set; } 
    public Guid ProjectId { get; set; }
}