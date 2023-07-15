using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task AddNewUserAsync(User user);
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserAsync(string username, string email);
        Task<bool> SaveAsync();
        void UpdateUser(User user);
    }
}