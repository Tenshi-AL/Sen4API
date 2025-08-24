using System.Collections;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Domain.Models;

public class User: IdentityUser<Guid>
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string MiddleName { get; set; } = null!;
    public string? PhotoPath { get; set; }
    
    public string? AdditionalEmail { get; set; }
    public string? Telegram { get; set; }
    public string? Instagram { get; set; }
    public string? Facebook { get; set; }
    public string? Viber { get; set; }
    public string? AboutMyself { get; set; }
    
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public IList<Project> Projects { get; set; } = new List<Project>(); 
    public IList<ProjectTask> CreatedTasks { get; set; } = new List<ProjectTask>(); 
    public IList<ProjectTask> ResponsibilitiesTasks { get; set; } = new List<ProjectTask>(); 
    
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
}