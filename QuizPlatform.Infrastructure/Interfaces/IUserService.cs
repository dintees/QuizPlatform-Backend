using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IUserService
{
    Task<UserDto?> LoginAndGenerateJwtTokenAsync(UserLoginDto dto);
    Task<string?> RegisterUserAsync(UserRegisterDto dto);
    Task<string?> ChangePasswordAsync(int id, ChangeUserPasswordDto user);
}