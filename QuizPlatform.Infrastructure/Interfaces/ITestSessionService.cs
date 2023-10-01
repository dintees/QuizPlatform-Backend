using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Test;
using QuizPlatform.Infrastructure.Models.TestSession;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface ITestSessionService
{
    Task<Result<int>> CreateTestSession(CreateTestSessionDto dto, int userId);
    Task<Result<TestDto?>> GetTestByTestSessionIdAsync(int testSessionId, int userId);
    Task<List<UserTestSessionDto>> GetActiveUserTestSessionsAsync(int userId);
    Task<bool> SaveUserAnswersAsync(List<UserAnswersDto> dto, int testSessionId, bool finish, int userId);
}