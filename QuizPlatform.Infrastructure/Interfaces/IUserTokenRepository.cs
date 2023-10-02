using QuizPlatform.Infrastructure.Entities;
using System.Linq.Expressions;
using QuizPlatform.Infrastructure.Enums;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IUserTokenRepository
{
    Task<UserToken?> GetByUserIdAndTypeAsync(int id, UserTokenType userTokenType);
    Task AddAsync(UserToken entity);
    public Task<List<UserToken>?> GetAllAsync(Expression<Func<UserToken, bool>> expression);
    void DeleteToken(UserToken userToken);
    Task<bool> SaveAsync();
}