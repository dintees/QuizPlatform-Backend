using FluentValidation;
using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.API.Validation;

public class ChangeUserPasswordValidator : AbstractValidator<ChangeUserPasswordDto>
{
    public ChangeUserPasswordValidator()
    {
        RuleFor(u => u.NewPassword).NotEmpty().WithMessage("Password could not be empty")
            .MinimumLength(8).WithMessage("Passwords must be at least 8 characters long");
        RuleFor(u => u.NewPasswordConfirmation).Equal(u => u.NewPassword)
            .WithMessage("The given passwords are not the same");
    }
}