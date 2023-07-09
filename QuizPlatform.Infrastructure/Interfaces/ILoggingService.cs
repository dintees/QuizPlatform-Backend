namespace QuizPlatform.Infrastructure.Interfaces;

public interface ILoggingService
{
    Task LogLoginInformation(int userId);
}