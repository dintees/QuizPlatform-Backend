using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> AddNewUserAsync(User user);
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserByUsername(string username);
        Task<User?> GetUserByIdAsync(int id);
        Task EditPassword(User user, string newPassword);
    }
}