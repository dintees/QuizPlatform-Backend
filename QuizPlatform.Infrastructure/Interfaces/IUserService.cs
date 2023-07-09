using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IUserService
{
    Task<List<User>?> GetAllAsync();
    Task<string?> LoginAndGenerateJwtTokenAsync(UserLoginDto dto);
    Task<bool> RegisterUserAsync(UserRegisterDto dto);
    Task<bool> ChangePassword(int id, ChangeUserPasswordDto user);
    Task<bool> CheckIfEmailExists(string email);
    Task<bool> CheckIfUsernameExists(string username);
}