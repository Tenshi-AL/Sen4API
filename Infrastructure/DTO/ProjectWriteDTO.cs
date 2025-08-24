using FluentValidation;
using Persistence;

namespace Infrastructure.DTO;

public class ProjectWriteDTO
{
    public required string Name { get; init; }
    public required DateTime CreatedDateTime { get; init; }
    public string? Description { get; init; }
}

public class ProjectValidator : AbstractValidator<ProjectWriteDTO>
{
    public ProjectValidator(Sen4Context dataBase)
    {
        RuleFor(p => p.Name)
            .NotNull()
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(p => p.CreatedDateTime)
            .NotNull();

        RuleFor(p => p.Description)
            .MaximumLength(80);
    }
}