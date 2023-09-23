using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface ITestSessionRepository
{
    Task<TestSession?> GetBySessionIdAsync(int testSessionId);
    Task<List<TestSession>> GetByUserIdWithTestAsync(int userId);
    Task AddAsync(TestSession testSession);
    Task<bool> SaveAsync();
}