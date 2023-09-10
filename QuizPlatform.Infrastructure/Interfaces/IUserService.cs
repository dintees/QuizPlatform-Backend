using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IUserService
{
    Task<Result<string>> LoginAndGenerateJwtTokenAsync(UserLoginDto dto);
    Task<string?> RegisterUserAsync(UserRegisterDto dto);
    Task<string?> ChangePasswordAsync(int id, ChangeUserPasswordDto user);
    Task<bool> ConfirmAccountAsync(string email, string code);
}