using FluentValidation;
using Persistence;

namespace Infrastructure.DTO;

public class UserUpdateDTO
{
    public string? Name { get; init; }
    public string? Surname { get; init; }
    public string? MiddleName { get; init; }
    public string? AdditionalEmail { get; init; }
    public string? Telegram { get; init; }
    public string? Instagram { get; init; }
    public string? Facebook { get; init; }
    public string? Viber { get; init; }
    public string? AboutMyself { get; init; }
}

public class UserUpdateValidator : AbstractValidator<UserUpdateDTO>
{
    public UserUpdateValidator(Sen4Context db)
    {
        RuleFor(p => p.Name)
            .MaximumLength(50);
        
        RuleFor(p => p.Surname)
            .MaximumLength(50);
        
        RuleFor(p => p.MiddleName)
            .MaximumLength(50);
        
        RuleFor(p => p.AdditionalEmail)
            .MaximumLength(50);
        
        RuleFor(p => p.Telegram)
            .MaximumLength(50);
        
        RuleFor(p => p.Instagram)
            .MaximumLength(50);
        
        RuleFor(p => p.Facebook)
            .MaximumLength(50);
        
        RuleFor(p => p.Viber)
            .MaximumLength(50);
    }
}