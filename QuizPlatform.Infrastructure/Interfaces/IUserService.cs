using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IUserService
{
    Task<Result<string>> LoginAndGenerateJwtTokenAsync(UserLoginDto dto);
    Task<string?> RegisterUserAsync(UserRegisterDto dto);
    Task<string?> ChangePasswordAsync(int userId, ChangeUserPasswordDto user);
    Task<bool> ConfirmAccountAsync(string email, string code);
    Task<string?> ChangeUserPropertiesAsync(int userId, ChangeUserPropertiesDto dto);
    Task<UserDto?> GetUserProfileInformation(int userId);
    Task<string?> GenerateCodeForNewPasswordAsync(string dtoEmail);
    Task<string?> CheckPasswordCodeValidityAsync(string email, string code);
    Task<string?> ResetPasswordAsync(ForgotPasswordDto dto);
    Task<List<UserSessionDto>?> GetUserSessionsAsync(int userId);
    Task<List<UserDto>?> GetAllUsersAsync();
    Task DeleteUserByIdAsync(int userId);
}