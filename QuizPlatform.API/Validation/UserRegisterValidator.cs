using FluentValidation;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.API.Validation;

public class UserRegisterValidator : AbstractValidator<UserRegisterDto>
{
    public UserRegisterValidator()
    {
        RuleFor(u => u.FirstName).NotEmpty().WithMessage(UserErrorMessages.EmptyFirstName);
        RuleFor(u => u.LastName).NotEmpty().WithMessage(UserErrorMessages.EmptyLastName);
        RuleFor(u => u.UserName).NotEmpty().WithMessage(UserErrorMessages.EmptyUserName);
        RuleFor(u => u.Email).NotEmpty().WithMessage(UserErrorMessages.EmptyEmail)
            .EmailAddress().WithMessage(UserErrorMessages.WrongEmailFormat);
        RuleFor(u => u.Password).NotEmpty().WithMessage(UserErrorMessages.EmptyPassword)
            .MinimumLength(8).WithMessage(UserErrorMessages.TooShortPassword);
        RuleFor(u => u.PasswordConfirmation).Equal(u => u.Password).WithMessage(UserErrorMessages.NotTheSamePasswords);
        RuleFor(u => u.RoleId).NotEmpty().WithMessage(UserErrorMessages.RoleCouldNotBeEmpty);
    }
}