namespace QuizPlatform.Infrastructure.Authentication;

public class AuthenticationSettings
{
    public string? Key { get; set; }
    public int ExpiresDays { get; set; }
    public string? Issuer { get; set; }
}