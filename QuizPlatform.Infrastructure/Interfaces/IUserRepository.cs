using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task AddNewUserAsync(User user);
        Task<User?> GetUserByEmailAsync(string email, bool readOnly = true);
        Task<User?> GetUserByIdAsync(int id, bool readOnly = true);
        Task<User?> GetUserAsync(string username, string email);
        Task<bool> SaveAsync();
        void UpdateUser(User user);
    }
}