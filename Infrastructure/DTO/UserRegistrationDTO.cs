using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Persistence;

namespace Infrastructure.DTO;

public class UserRegistrationDTO
{
    public required string Email { get; init; } = null!;
    public required string Name { get; init; } = null!;
    public required string Surname { get; init; } = null!;
    public required string MiddleName { get; init; } = null!;
    public required Guid PostId { get; init; }
    public required string Password { get; init; }
}

public class UserRegistrationValidator : AbstractValidator<UserRegistrationDTO>
{
    public UserRegistrationValidator(Sen4Context db)
    {
        RuleFor(p => p.Email)
            .EmailAddress();

        RuleFor(p => p.Name)
            .MaximumLength(50);
        
        RuleFor(p => p.Surname)
            .MaximumLength(50);
        
        RuleFor(p => p.MiddleName)
            .MaximumLength(50);

        RuleFor(p => p.PostId)
            .Must(postId => db.Posts.Any(post => post.Id == postId));
    }
}