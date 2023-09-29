using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IUserAnswersRepository
{
    Task<UserAnswers?> GetUserAnswerByIdAsync(int id);
    Task<List<UserAnswers>?> GetUserAnswersByTestSessionIdAsync(int testSessionId);
    Task AddAsync(UserAnswers userAnswer);
    Task AddRangeAsync(List<UserAnswers> userAnswers);
    void Update(UserAnswers userAnswer);
    void Delete(UserAnswers userAnswers);
    Task<bool> SaveAsync();
}