namespace QuizPlatform.Service;

public interface IDatabaseCleaner
{
    Task CleanUserTokensEntity();
}