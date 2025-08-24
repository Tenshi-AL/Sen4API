using FluentValidation;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Infrastructure.DTO;

public class FileWriteDTO
{
    public required IFormFile File { get; init; } 
    public required Guid ProjectId { get; init; } 
    public Guid? TaskId { get; init; }
}

public class FileValidator : AbstractValidator<FileWriteDTO>
{
    public FileValidator(Sen4Context db)
    {
        RuleFor(p => p.File)
            .NotNull();

        RuleFor(p => p.ProjectId)
            .Must(projectId => db.Projects.Any(project => project.Id == projectId));
    }
}