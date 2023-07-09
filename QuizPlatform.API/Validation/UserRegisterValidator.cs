using FluentValidation;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.API.Validation;

public class UserRegisterValidator : AbstractValidator<UserRegisterDto>
{
    private readonly IUserService _userService;

    public UserRegisterValidator(IUserService userService)
    {
        _userService = userService;

        RuleFor(u => u.Email).NotEmpty().WithMessage("Email could not be empty.")
            .EmailAddress().WithMessage("Email has bad format")
            .Must(CheckIfEmailExists).WithMessage("The email already exists in the database");
        RuleFor(u => u.Username).NotEmpty().WithMessage("Username could not be empty.")
            .Must(CheckIfUsernameExists).WithMessage("The username already exists");
        RuleFor(u => u.Password).NotEmpty().WithMessage("Password could not be empty")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long");
        RuleFor(u => u.PasswordConfirmation).Equal(u => u.Password).WithMessage("Given passwords are not the same");
        RuleFor(u => u.RoleId).NotEmpty();
    }

    private bool CheckIfEmailExists(string email)
    {
        return !_userService.CheckIfEmailExists(email).GetAwaiter().GetResult();
    }

    private bool CheckIfUsernameExists(string? username)
    {
        if (username is null) return true;
        return !_userService.CheckIfUsernameExists(username).GetAwaiter().GetResult();
    }
}