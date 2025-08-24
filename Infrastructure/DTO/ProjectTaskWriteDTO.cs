using System.Text.Json.Serialization;
using FluentValidation;
using Persistence;

namespace Infrastructure.DTO;

public class ProjectTaskWriteDTO
{
    public string Name { get; set ; }
    public string? Description { get; set; }
    public Guid TaskStatusId { get; set; }
    public Guid UserExecutorId { get; set; }
    public Guid UserCreatedId { get; set; }
    public Guid PriorityId { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime DeadlineDate { get; set; }
}

public class ProjectTaskValidator : AbstractValidator<ProjectTaskWriteDTO>
{
    public ProjectTaskValidator(Sen4Context db)
    {
        RuleFor(task => task.Name)
            .NotNull()
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(p => p.Description)
            .MaximumLength(80);
    }
}