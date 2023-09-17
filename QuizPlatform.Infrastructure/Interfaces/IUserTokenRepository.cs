using QuizPlatform.Infrastructure.Entities;
using System.Linq.Expressions;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IUserTokenRepository
{
    Task<UserToken?> GetByUserIdAsync(int id);
    Task AddAsync(UserToken entity);
    public Task<List<UserToken>?> GetAllAsync(Expression<Func<UserToken, bool>> expression);
    void DeleteToken(UserToken userToken);
    Task<bool> SaveAsync();
}