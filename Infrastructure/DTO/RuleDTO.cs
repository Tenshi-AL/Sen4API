using FluentValidation;
using Persistence;

namespace Infrastructure.DTO;

public class RuleDTO
{
    public required Guid OperationId { get; init; }
    public string? Controller { get; init; }
    public string? Action { get; init; }
    public string? Description { get; init; }
    public bool Access { get; init; }
}

public class RuleValidator : AbstractValidator<RuleDTO>
{
    public RuleValidator(Sen4Context db)
    {
        RuleFor(task => task.OperationId)
            .Must(operationId => db.Operations.Any(operation => operation.Id == operationId));

        RuleFor(p => p.Controller)
            .NotNull()
            .MaximumLength(50);
        
        RuleFor(p => p.Action)
            .NotNull()
            .MaximumLength(50);
        
        RuleFor(p => p.Description)
            .NotNull()
            .MaximumLength(50);

        RuleFor(p => p.Access)
            .NotNull();
    }
}