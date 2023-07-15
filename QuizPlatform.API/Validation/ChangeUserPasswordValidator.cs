using FluentValidation;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.API.Validation;

public class ChangeUserPasswordValidator : AbstractValidator<ChangeUserPasswordDto>
{
    public ChangeUserPasswordValidator()
    {
        RuleFor(u => u.NewPassword).NotEmpty().WithMessage(UserErrorMessages.EmptyPassword)
            .MinimumLength(8).WithMessage(UserErrorMessages.TooShortPassword);
        RuleFor(u => u.NewPasswordConfirmation).Equal(u => u.NewPassword)
            .WithMessage(UserErrorMessages.NotTheSamePasswords);
    }
}