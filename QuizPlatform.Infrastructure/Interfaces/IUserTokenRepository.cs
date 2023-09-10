using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IUserTokenRepository
{
    Task<UserToken?> GetByUserIdAsync(int id);
    Task AddAsync(UserToken entity);
    Task<bool> SaveAsync();
}