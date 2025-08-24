using FluentValidation;

namespace Infrastructure.DTO;

public class LoginDTO
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginValidator : AbstractValidator<LoginDTO>
{
    public LoginValidator()
    {
        RuleFor(p => p.Email)
            .NotNull()
            .NotEmpty()
            .EmailAddress();

        RuleFor(p => p.Password)
            .NotNull()
            .NotEmpty()
            .MaximumLength(50);
    }
}