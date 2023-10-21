using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface ILoggingService
{
    Task LogLoginInformation(int userId);
    Task<List<UserSessionDto>?> GetUserSessionsList(string? username);
}